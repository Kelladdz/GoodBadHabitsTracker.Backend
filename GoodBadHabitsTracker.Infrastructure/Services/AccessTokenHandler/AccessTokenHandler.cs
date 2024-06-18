using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.Infrastructure.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Infrastructure.Services.AccessTokenHandler
{
    internal sealed class AccessTokenHandler(IOptions<JwtSettings> jwtSettings, IConfiguration configuration) : IAccessTokenHandler
    {
        public string GenerateAccessToken(UserSession userSession, out string userFingerprint)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:Key"]!));

            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            userFingerprint = GenerateUserFingerprint();
            var userFingerprintHash = GenerateUserFingerprintHash(userFingerprint);

            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, userSession.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Name, userSession.UserName),
            new Claim(JwtRegisteredClaimNames.Email, userSession.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("roles", string.Join(", ", userSession.Roles)),
            new Claim("userFingerprint", userFingerprintHash),
            new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new Claim("authenticationMethod", "EmailPassword")
            };

            var claimsIdentity = new ClaimsIdentity(claims);



            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = "https://localhost:7154",
                Audience = "https://localhost:8080",
                SigningCredentials = credentials,
                Expires = DateTime.UtcNow.AddMinutes(15),
                NotBefore = DateTime.UtcNow,
                Subject = claimsIdentity
            };


            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateToken(tokenDescriptor);
            var encodedJwt = handler.WriteToken(token);
            return encodedJwt;
        }

        public string GenerateUserFingerprint()
        {
            string userFingerprint;
            var randomString = new byte[32];
            using (var secureRandom = RandomNumberGenerator.Create())
            {
                secureRandom.GetBytes(randomString);
                userFingerprint = Convert.ToBase64String(randomString);
            }

            return userFingerprint;
        }

        public string GenerateUserFingerprintHash(string userFingerprint)
        {
            string userFingerprintHash;
            byte[] userFingerprintDigest = SHA256.HashData(Encoding.UTF8.GetBytes(userFingerprint));
            userFingerprintHash = Convert.ToBase64String(userFingerprintDigest);

            return userFingerprintHash;
        }
    }
}
