using FluentAssertions;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Infrastructure.Security.RefreshTokenHandler;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Infrastructure.Tests.Security.RefreshTokenHandler
{
    public class HandlerTests
    {
        private readonly IRefreshTokenHandler _handler;
        public HandlerTests()
        {
            _handler = new Handler();
        }
        [Fact]
        public void GenerateRefreshToken_ShouldReturnBase64String()
        {
            // Act
            var token = _handler.GenerateRefreshToken();

            // Assert
            var buffer = new Span<byte>(new byte[token.Length]);
            Convert.TryFromBase64String(token, buffer, out _).Should().BeTrue();
        }

        [Fact]
        public void GenerateRefreshToken_ShouldReturnDifferentValues()
        {
            // Arrange

            // Act
            var token1 = _handler.GenerateRefreshToken();
            var token2 = _handler.GenerateRefreshToken();

            // Assert
            Assert.NotEqual(token1, token2);
        }
    }
}
