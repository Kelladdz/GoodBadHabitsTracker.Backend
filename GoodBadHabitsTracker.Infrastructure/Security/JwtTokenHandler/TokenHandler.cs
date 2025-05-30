﻿using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Infrastructure.Settings;

namespace GoodBadHabitsTracker.Infrastructure.Security.JwtTokenHandler
{
    internal sealed class TokenHandler(IOptions<JwtSettings> jwtSettings) : ITokenHandler
    {
        private readonly JwtSettings _jwtSettings = jwtSettings.Value;
        public string GenerateToken(ClaimsIdentity claimsIdentity, DateTime expiry)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var header = new JwtHeader(signingCredentials);
            header["kid"] = Guid.NewGuid().ToString();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience![0],
                SigningCredentials = signingCredentials,
                Expires = expiry,
                NotBefore = _jwtSettings.NotBefore,
                Subject = claimsIdentity,
                
            };

            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateToken(tokenDescriptor);
            var tokenString = handler.WriteToken(token);

            return tokenString;
        }

        public string GenerateAccessToken(UserSession userSession, out string userFingerprint)
        {
            userFingerprint = GenerateUserFingerprint();
            var userFingerprintHash = GenerateUserFingerprintHash(userFingerprint);

            var claimsIdentity = GetClaimsIdentity(userSession);
            claimsIdentity.AddClaim(new Claim(CustomClaimNames.USER_FINGERPRINT, userFingerprintHash));
            var expiry = _jwtSettings.Expiration;

            return GenerateToken(claimsIdentity, expiry);
        }

        public string GenerateRefreshToken(UserSession userSession)
        {
            var claimsIdentity = GetClaimsIdentity(userSession);
            
            var expiry = _jwtSettings.RefreshTokenExpiration;

            return GenerateToken(claimsIdentity, expiry);
        }
        public ClaimsPrincipal ValidateAndGetPrincipalFromToken(string token)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingCredentials.Key,
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudiences = _jwtSettings.Audience,
                ValidateLifetime = false,
            };

            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken is null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }

        public ClaimsPrincipal GetClaimsPrincipalFromIdToken(string idToken)
        {
            if (string.IsNullOrEmpty(idToken)) return null;

            var handler = new JwtSecurityTokenHandler();
            var decodedIdToken = handler.ReadJwtToken(idToken);
            var claims = decodedIdToken.Claims;
            var claimsIdentity = new ClaimsIdentity(claims, ExternalTokenProviders.GOOGLE);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            return claimsPrincipal;
        }

        public IEnumerable<Claim> GetClaimsFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var claims = handler.ReadJwtToken(token).Claims;

            return claims;
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

        private ClaimsIdentity GetClaimsIdentity(UserSession userSession)
        {
            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, userSession.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Name, userSession.UserName),
            new Claim(JwtRegisteredClaimNames.Email, userSession.Email),
            new Claim(JwtRegisteredClaimNames.Jti, _jwtSettings.Jti),
            new Claim(CustomClaimNames.ROLES, string.Join(", ", userSession.Roles)),
            new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            };

            return new ClaimsIdentity(claims);
        }
    }}
