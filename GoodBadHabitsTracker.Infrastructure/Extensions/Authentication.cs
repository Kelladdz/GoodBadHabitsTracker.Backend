using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using GoodBadHabitsTracker.Infrastructure.Configurations;
using GoodBadHabitsTracker.Infrastructure.Security.AccessTokenHandler;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;

namespace GoodBadHabitsTracker.Infrastructure.Extensions
{
    public static class Authentication
    {
        public static void AddJwt(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetSection("JwtSettings:Key").Value!));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var issuer = configuration.GetSection("JwtSettings:Issuer").Value!;
            var audience = configuration.GetSection("JwtSettings:Audience").Value!;

            services.Configure<JwtSettings>(options =>
            {
                options.Issuer = issuer;
                options.Audience = audience;
                options.SigningCredentials = signingCredentials;
            });

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingCredentials.Key,
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
            };

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false; //ONLY IN DEVELOPMENT
                    options.TokenValidationParameters = tokenValidationParameters;
                    options.SaveToken = true;

                    new JwtBearerEvents().OnTokenValidated = (context) =>
                    {
                        var jwtToken = context.SecurityToken as JsonWebToken;
                        if (jwtToken is null)
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            return Task.CompletedTask;
                        }

                        var userFingerprintHash = jwtToken.Claims.FirstOrDefault(c => c.Type == "userFingerprint")?.Value;
                        if (userFingerprintHash is null)
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            return Task.CompletedTask;
                        }

                        var jwtSettings = Options.Create(new JwtSettings());
                        if (userFingerprintHash != new Handler(jwtSettings).GenerateUserFingerprintHash(context.Request.Cookies["__Secure-Fgp"].Replace("__Secure-Fgp=", "", StringComparison.InvariantCultureIgnoreCase)))
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            return Task.CompletedTask;
                        }
                        return Task.CompletedTask;
                    };

                    new JwtBearerEvents().OnAuthenticationFailed = (context) =>
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    };
                });
        }
    }
}
