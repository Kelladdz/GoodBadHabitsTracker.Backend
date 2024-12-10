using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using GoodBadHabitsTracker.Infrastructure.Security.IdTokenHandler;
using FluentAssertions;

namespace GoodBadHabitsTracker.Infrastructure.Tests.Security.IdTokenHandler
{
    public class IdTokenHandlerTests
    {
        private readonly Infrastructure.Security.IdTokenHandler.IdTokenHandler _idTokenHandler;

        public IdTokenHandlerTests()
        {
            _idTokenHandler = new Infrastructure.Security.IdTokenHandler.IdTokenHandler();
        }

        [Fact]
        public void GetClaimsPrincipalFromIdToken_IdTokenIsNullOrEmpty_ReturnsNull()
        {
            // Act
            var result = _idTokenHandler.GetClaimsPrincipalFromIdToken(null);

            // Assert
            result.Should().BeNull();

            // Act
            result = _idTokenHandler.GetClaimsPrincipalFromIdToken(string.Empty);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetClaimsPrincipalFromIdToken_ValidIdToken_ReturnsClaimsPrincipal()
        {
            // Arrange
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim("sub", "12345"),
                new Claim("name", "John Doe"),
                new Claim("email", "john.doe@example.com")
            }),
                Expires = DateTime.UtcNow.AddHours(1)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var idToken = tokenHandler.WriteToken(token);

            // Act
            var result = _idTokenHandler.GetClaimsPrincipalFromIdToken(idToken);

            // Assert
            result.Should().NotBeNull();
            result.Identity.Should().BeOfType<ClaimsIdentity>();
            var claimsIdentity = result.Identity as ClaimsIdentity;
            claimsIdentity.AuthenticationType.Should().Be("Google");
            claimsIdentity.Claims.Should().Contain(c => c.Type == "sub" && c.Value == "12345");
            claimsIdentity.Claims.Should().Contain(c => c.Type == "name" && c.Value == "John Doe");
            claimsIdentity.Claims.Should().Contain(c => c.Type == "email" && c.Value == "john.doe@example.com");
        }
    }
}
