using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.TestMisc;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using GoodBadHabitsTracker.Infrastructure.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using System.Security.Cryptography;
using GoodBadHabitsTracker.Infrastructure.Security.TokenHandler;
using GoodBadHabitsTracker.Core.Models;

namespace GoodBadHabitsTracker.Infrastructure.Tests.Security.AccessTokenHandler
{
    public class TokenHandlerTests
    {
        private readonly TokenHandler _tokenHandler;
        private readonly Mock<IOptions<JwtSettings>> _jwtSettingsMock;

        public TokenHandlerTests()
        {
            _jwtSettingsMock = new Mock<IOptions<JwtSettings>>();
            _jwtSettingsMock.Setup(x => x.Value).Returns(new JwtSettings
            {
                Key = "nf2#fs9sgnwgtn098vn8w4n083_4vsdvgdfbsr",
                Issuer = "TestIssuer",
                Audience = new List<string> { "TestAudience" },
                Jti = Guid.NewGuid().ToString()
            });

            _tokenHandler = new TokenHandler(_jwtSettingsMock.Object);
        }

        [Fact]
        public void GenerateToken_ValidClaimsIdentity_ReturnsToken()
        {
            // Arrange
            var claimsIdentity = new ClaimsIdentity(new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, "12345"),
            new Claim(JwtRegisteredClaimNames.Name, "John Doe"),
            new Claim(JwtRegisteredClaimNames.Email, "john.doe@example.com")
        });
            var expiry = DateTime.UtcNow.AddMinutes(30);

            // Act
            var token = _tokenHandler.GenerateToken(claimsIdentity, expiry);

            // Assert
            token.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void GenerateAccessToken_ValidUserSession_ReturnsAccessToken()
        {
            // Arrange
            var userSession = DataGenerator.SeedUserSession();

            // Act
            var accessToken = _tokenHandler.GenerateAccessToken(userSession, out var userFingerprint);

            // Assert
            accessToken.Should().NotBeNullOrEmpty();
            userFingerprint.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void GenerateRefreshToken_ValidUserSession_ReturnsRefreshToken()
        {
            // Arrange
            var userSession = DataGenerator.SeedUserSession();

            // Act
            var refreshToken = _tokenHandler.GenerateRefreshToken(userSession);

            // Assert
            refreshToken.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void ValidateAndGetPrincipalFromToken_ValidToken_ReturnsClaimsPrincipal()
        {
            // Arrange
            var claimsIdentity = new ClaimsIdentity(new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, "12345"),
            new Claim(JwtRegisteredClaimNames.Name, "John Doe"),
            new Claim(JwtRegisteredClaimNames.Email, "john.doe@example.com")
        });
            var expiry = DateTime.UtcNow.AddMinutes(30);
            var token = _tokenHandler.GenerateToken(claimsIdentity, expiry);

            // Act
            var principal = _tokenHandler.ValidateAndGetPrincipalFromToken(token);

            // Assert
            principal.Should().NotBeNull();
            principal.Identity.Should().BeOfType<ClaimsIdentity>();
        }

        [Fact]
        public void GetClaimsFromToken_ValidToken_ReturnsClaims()
        {
            // Arrange
            var claimsIdentity = new ClaimsIdentity(new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, "12345"),
            new Claim(JwtRegisteredClaimNames.Name, "John Doe"),
            new Claim(JwtRegisteredClaimNames.Email, "john.doe@example.com")
        });
            var expiry = DateTime.UtcNow.AddMinutes(30);
            var token = _tokenHandler.GenerateToken(claimsIdentity, expiry);

            // Act
            var claims = _tokenHandler.GetClaimsFromToken(token);

            // Assert
            claims.Should().NotBeEmpty();
            claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == "12345");
            claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Name && c.Value == "John Doe");
            claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "john.doe@example.com");
        }

        [Fact]
        public void GenerateUserFingerprint_ReturnsNonEmptyString()
        {
            // Act
            var userFingerprint = _tokenHandler.GenerateUserFingerprint();

            // Assert
            userFingerprint.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void GenerateUserFingerprintHash_ValidFingerprint_ReturnsHash()
        {
            // Arrange
            var userFingerprint = _tokenHandler.GenerateUserFingerprint();

            // Act
            var userFingerprintHash = _tokenHandler.GenerateUserFingerprintHash(userFingerprint);

            // Assert
            userFingerprintHash.Should().NotBeNullOrEmpty();
        }
    }

}

