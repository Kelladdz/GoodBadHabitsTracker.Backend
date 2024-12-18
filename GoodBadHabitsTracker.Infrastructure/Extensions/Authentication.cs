using Microsoft.Extensions.DependencyInjection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using GoodBadHabitsTracker.Infrastructure.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;

namespace GoodBadHabitsTracker.Infrastructure.Extensions
{
    public static class Authentication
    {
        public static void AddJwt(this IServiceCollection services, IConfiguration configuration)
        {
           
            services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = "PasswordLogin";
                options.DefaultChallengeScheme = "PasswordLogin";
            })
                .AddJwtBearer("PasswordLogin", options =>
                {
                    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetSection("JwtSettings:Key").Value!));
                    var issuer = configuration["JwtSettings:Issuer"];
                    var audiences = new List<string>();
                    foreach (var audience in configuration.GetSection("JwtSettings:Audience").GetChildren())
                    {
                        audiences.Add(audience.Value);
                    }

                    var tokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = false,
                        IssuerSigningKey = securityKey,
                        ValidateIssuer = true,
                        ValidIssuer = issuer,
                        ValidateAudience = true,
                        ValidAudiences = audiences,
                        ValidateLifetime = true,
                    };

                    options.RequireHttpsMetadata = false; //ONLY IN DEVELOPMENT
                    options.TokenValidationParameters = tokenValidationParameters;
                    options.SaveToken = true;

                    new JwtBearerEvents().OnTokenValidated = (context) =>
                    {
                        var jwtToken = context.SecurityToken as JsonWebToken;
                        if (jwtToken is null)
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            context.Response.Body.Write(Encoding.UTF8.GetBytes("Null token"));
                            return Task.CompletedTask;
                        }

                        var userFingerprintHash = jwtToken.Claims.FirstOrDefault(c => c.Type == CustomClaimNames.USER_FINGERPRINT)?.Value;
                        if (userFingerprintHash is null)
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            context.Response.Body.Write(Encoding.UTF8.GetBytes("Null userFingerprintHash"));
                            return Task.CompletedTask;
                        }

                        var jwtSettings = Options.Create(new JwtSettings());
                        if (userFingerprintHash != new Security.JwtTokenHandler.JwtTokenHandler(jwtSettings).GenerateUserFingerprintHash(context.Request.Cookies[CookieNames.USER_FINGERPRINT_COOKIE_NAME]!.Replace($"{CookieNames.USER_FINGERPRINT_COOKIE_NAME }= ", "", StringComparison.InvariantCultureIgnoreCase)))
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            context.Response.Body.Write(Encoding.UTF8.GetBytes($"userFingerprintHash != {CookieNames.USER_FINGERPRINT_COOKIE_NAME}"));
                            return Task.CompletedTask;
                        }
                        return Task.CompletedTask;
                    };

                    new JwtBearerEvents().OnAuthenticationFailed = (context) =>
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.Body.Write(Encoding.UTF8.GetBytes($"Authentication failed"));
                        return Task.CompletedTask;
                    };


                })
                .AddJwtBearer("Auth0Login", options =>
                {
                    options.UseSecurityTokenValidators = true;
                    options.MetadataAddress = configuration["JwtSettings:Configuration"]!;
                    options.Authority = configuration["JwtSettings:Auth0Issuer"];
                    var audiences = new List<string>();
                    foreach (var audience in configuration.GetSection("JwtSettings:Auth0Audience").GetChildren())
                    {
                        audiences.Add(audience.Value);
                    }
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = configuration["JwtSettings:Auth0Issuer"],
                        ValidAudiences = audiences,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:Auth0Key"]!)),
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true
                    };
                    new JwtBearerEvents().OnAuthenticationFailed = (context) =>
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    };
                })
                .AddCookie("Identity.External")
                .AddCookie("Identity.Application");
        }
    }
}