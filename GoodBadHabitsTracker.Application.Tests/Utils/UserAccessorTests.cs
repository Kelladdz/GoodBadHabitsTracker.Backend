using FluentAssertions;
using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Application.Utils;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.Tests.Utils
{
    public class UserAccessorTests
    {
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<ITokenHandler> _tokenHandlerMock;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly UserAccessor _userAccessor;

        public UserAccessorTests()
        {
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _tokenHandlerMock = new Mock<ITokenHandler>();
            _userManagerMock = new Mock<UserManager<User>>(
                Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);
            _userAccessor = new UserAccessor(_httpContextAccessorMock.Object, _tokenHandlerMock.Object, _userManagerMock.Object);
        }

        [Fact]
        public async Task GetCurrentUser_ValidToken_ReturnsUserById()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var context = new DefaultHttpContext();
            context.Request.Headers["Authorization"] = "Bearer valid_token";
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

            _tokenHandlerMock.Setup(x => x.GetClaimsFromToken(It.IsAny<string>())).Returns(new List<Claim>
        {
            new Claim("sub", userId)
        });

            var user = new User { Id = Guid.Parse(userId) };
            _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);

            // Act
            var result = await _userAccessor.GetCurrentUser();

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(user);
        }

        [Fact]
        public async Task GetCurrentUser_ValidToken_ReturnsUserByLogin()
        {
            // Arrange
            var sub = "google|12345";
            var context = new DefaultHttpContext();
            context.Request.Headers["Authorization"] = "Bearer valid_token";
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

            _tokenHandlerMock.Setup(x => x.GetClaimsFromToken(It.IsAny<string>())).Returns(new List<Claim>
        {
            new Claim("sub", sub)
        });

            var user = new User { Id = Guid.NewGuid() };
            _userManagerMock.Setup(x => x.FindByLoginAsync("Google", sub)).ReturnsAsync(user);

            // Act
            var result = await _userAccessor.GetCurrentUser();

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(user);
        }
    }
}
