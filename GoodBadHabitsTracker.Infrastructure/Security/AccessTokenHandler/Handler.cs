﻿using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.Infrastructure.Configurations;
using GoodBadHabitsTracker.Core.Interfaces;

namespace GoodBadHabitsTracker.Infrastructure.Security.AccessTokenHandler
{
    internal sealed class Handler(IOptions<JwtSettings> jwtSettings) : IAccessTokenHandler
    {
        private readonly JwtSettings _jwtSettings = jwtSettings.Value;
        public string GenerateAccessToken(UserSession userSession, out string userFingerprint)
        {
            userFingerprint = GenerateUserFingerprint();
            var userFingerprintHash = GenerateUserFingerprintHash(userFingerprint);

            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, userSession.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Name, userSession.UserName),
            new Claim(JwtRegisteredClaimNames.Email, userSession.Email),
            new Claim(JwtRegisteredClaimNames.Jti, _jwtSettings.Jti),
            new Claim("roles", string.Join(", ", userSession.Roles)),
            new Claim("userFingerprint", userFingerprintHash),
            new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new Claim("authenticationMethod", "EmailPassword")
            };

            var claimsIdentity = new ClaimsIdentity(claims);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = _jwtSettings.SigningCredentials,
                Expires = _jwtSettings.Expiration,
                NotBefore = _jwtSettings.NotBefore,
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
