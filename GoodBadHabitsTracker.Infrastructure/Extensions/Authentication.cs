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
using GoodBadHabitsTracker.Infrastructure.Security.TokenHandler;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;

namespace GoodBadHabitsTracker.Infrastructure.Extensions
{
    public static class Authentication
    {
        public static void AddJwt(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

            

            

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer("PasswordLogin", options =>
                {
                    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetSection("JwtSettings:Key").Value!));
                    var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
                    var issuer = configuration["JwtSettings:Issuer"];
                    var audience = configuration["JwtSettings:Audience"];


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

                    options.Authority = configuration["JwtSettings:Authority"];
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
                        if (userFingerprintHash != new Security.TokenHandler.TokenHandler(jwtSettings).GenerateUserFingerprintHash(context.Request.Cookies["__Secure-Fgp"].Replace("__Secure-Fgp=", "", StringComparison.InvariantCultureIgnoreCase)))
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
                })
                .AddJwtBearer("Auth0Login", options =>
                {
                    options.UseSecurityTokenValidators = true;
                    options.MetadataAddress = configuration["JwtSettings:Configuration"];
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
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:Auth0Key"])),
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
                .AddCookie("Identity.Application")
                .AddPolicyScheme("MultiAuthSchemes", JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.ForwardDefaultSelector = context =>
                    {
                        string authorization = context.Request.Headers["Authorization"]!;
                        if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer "))
                        {
                            var token = authorization.Substring("Bearer ".Length).Trim();
                            var jwtHandler = new JwtSecurityTokenHandler();
                            return (jwtHandler.CanReadToken(token) && jwtHandler.ReadJwtToken(token).Issuer.Equals("https://localhost:7208/"))
                                ? JwtBearerDefaults.AuthenticationScheme : "SecondJwtScheme";
                        }
                        return CookieAuthenticationDefaults.AuthenticationScheme;
                    };
                });
        }
    }
}
