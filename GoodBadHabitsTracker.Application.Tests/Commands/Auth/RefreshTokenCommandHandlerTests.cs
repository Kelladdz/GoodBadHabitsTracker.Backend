using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using GoodBadHabitsTracker.Application.Commands.Auth.Login;
using GoodBadHabitsTracker.Application.Commands.Auth.RefreshToken;
using GoodBadHabitsTracker.Application.DTOs.Request;
using GoodBadHabitsTracker.Application.DTOs.Response;
using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.TestMisc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;
namespace GoodBadHabitsTracker.Application.Tests.Commands.Auth
{
    public class RefreshTokenCommandHandlerTests
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<ITokenHandler> _tokenHandlerMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly RefreshTokenCommandHandler _handler;

        public RefreshTokenCommandHandlerTests()
        {
            _userManagerMock = new Mock<UserManager<User>>(
                Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);
            _tokenHandlerMock = new Mock<ITokenHandler>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _handler = new RefreshTokenCommandHandler(_userManagerMock.Object, _tokenHandlerMock.Object, _httpContextAccessorMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnLoginResponse_WhenTokensAreValid()
        {
            //ARRANGE

            var request = new RefreshTokenRequest { RefreshToken = DataGenerator.SeedRefreshToken() };
            var command = new RefreshTokenCommand(request, CancellationToken.None);
            var user = DataGenerator.SeedUser();
            var userRoles = new[] { "User" };
            var accessTokenPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
             {
                 new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                 new Claim("exp", DateTimeOffset.UtcNow.AddMinutes(15).ToUnixTimeSeconds().ToString())
             }
             ))
            ;
            var refreshTokenPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("exp", DateTimeOffset.UtcNow.AddMinutes(15).ToUnixTimeSeconds().ToString())
            }));

        _httpContextAccessorMock.Setup(x => x.HttpContext.Request.Headers.Authorization).Returns("Bearer validAccessToken");
        _tokenHandlerMock.Setup(x => x.ValidateAndGetPrincipalFromToken(It.IsAny<string>())).Returns(accessTokenPrincipal);
        _tokenHandlerMock.Setup(x => x.ValidateAndGetPrincipalFromToken(It.IsAny<string>())).Returns(refreshTokenPrincipal);
        _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(userRoles);
        _tokenHandlerMock.Setup(x => x.GenerateAccessToken(It.IsAny<UserSession>(), out It.Ref<string>.IsAny)).Returns("newAccessToken");
        _tokenHandlerMock.Setup(x => x.GenerateRefreshToken(It.IsAny<UserSession>())).Returns("newRefreshToken");

            //ACT
            var result = await _handler.Handle(command, CancellationToken.None);

            //ASSERT
            result.Should().NotBeNull();
            result.AccessToken.Should().Be("newAccessToken");
            result.RefreshToken.Should().Be("newRefreshToken");
        }

        [Fact]
        public async Task Handle_ShouldThrowAppException_WhenAccessTokenIsInvalid()
        {
            //ARRANGE
            var request = new RefreshTokenRequest { RefreshToken = DataGenerator.SeedRefreshToken() };
            var command = new RefreshTokenCommand(request, CancellationToken.None);

            _httpContextAccessorMock.Setup(x => x.HttpContext.Request.Headers.Authorization).Returns("Bearer invalidAccessToken");
            _tokenHandlerMock.Setup(x => x.ValidateAndGetPrincipalFromToken("invalidAccessToken")).Returns((ClaimsPrincipal)null);

            //ACT
            Func<Task> action = async () => await _handler.Handle(command, CancellationToken.None);

            //ASSERT
            await action.Should().ThrowAsync<AppException>()
                .Where(x => x.Code == HttpStatusCode.Unauthorized && x.Errors!.Equals("Invalid access token"));
        }

        [Fact]
        public async Task Handle_ShouldThrowAppException_WhenRefreshTokenIsInvalid()
        {
            //ARRANGE
            var request = new RefreshTokenRequest { RefreshToken = DataGenerator.SeedRefreshToken() };
            var command = new RefreshTokenCommand(request, CancellationToken.None);
            var accessTokenPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("sub", "userId")
            }));

            _httpContextAccessorMock.Setup(x => x.HttpContext.Request.Headers.Authorization).Returns("Bearer validAccessToken");
            _tokenHandlerMock.Setup(x => x.ValidateAndGetPrincipalFromToken("validAccessToken")).Returns(accessTokenPrincipal);
            _tokenHandlerMock.Setup(x => x.ValidateAndGetPrincipalFromToken("invalidRefreshToken")).Returns((ClaimsPrincipal)null);

            //ACT
            Func<Task> action = async () => await _handler.Handle(command, CancellationToken.None);

            //ASSERT
            await action.Should().ThrowAsync<AppException>()
                .Where(x => x.Code == HttpStatusCode.Unauthorized && x.Errors!.Equals("Invalid refresh token"));
        }

        [Fact]
        public async Task Handle_ShouldThrowAppException_WhenUserNotFound()
        {
            //ARRANGE
            var request = new RefreshTokenRequest { RefreshToken = DataGenerator.SeedRefreshToken() };
            var accessToken = DataGenerator.SeedAccessToken();
            var command = new RefreshTokenCommand(request, CancellationToken.None);
            var accessTokenPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("sub", "userId")
            }));
            var refreshTokenPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("sub", "userId")
            }));

            _httpContextAccessorMock.Setup(x => x.HttpContext.Request.Headers.Authorization).Returns($"Bearer {accessToken}");
            _tokenHandlerMock.Setup(x => x.ValidateAndGetPrincipalFromToken(accessToken)).Returns(accessTokenPrincipal);
            _tokenHandlerMock.Setup(x => x.ValidateAndGetPrincipalFromToken(request.RefreshToken)).Returns(refreshTokenPrincipal);
            _userManagerMock.Setup(x => x.FindByIdAsync("userId")).ReturnsAsync((User?)null);

            //ACT
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            //ASSERT
            await act.Should().ThrowAsync<AppException>()
                .Where(x => x.Code == HttpStatusCode.Unauthorized && x.Errors!.Equals("User not found"));
        }

        [Fact]
        public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenUserIdsDoNotMatch()
        {
            //ARRANGE
            var request = new RefreshTokenRequest { RefreshToken = DataGenerator.SeedRefreshToken() };
            var command = new RefreshTokenCommand(request, CancellationToken.None);
            var user = DataGenerator.SeedUser();
            var secUser = DataGenerator.SeedUser();
            var accessTokenPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }));
            var refreshTokenPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, secUser.Id.ToString()),
                new Claim("exp", DateTimeOffset.UtcNow.AddMinutes(15).ToUnixTimeSeconds().ToString())
            }));

            _httpContextAccessorMock.Setup(x => x.HttpContext.Request.Headers.Authorization).Returns("Bearer validAccessToken");
            _tokenHandlerMock.Setup(x => x.ValidateAndGetPrincipalFromToken("validAccessToken")).Returns(accessTokenPrincipal);
            _tokenHandlerMock.Setup(x => x.ValidateAndGetPrincipalFromToken(request.RefreshToken)).Returns(refreshTokenPrincipal);
            _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
            //ACT
            Func<Task> action = async () => await _handler.Handle(command, CancellationToken.None);

            //ASSERT
            await action.Should().ThrowAsync<AppException>()
                .Where(x => x.Code == HttpStatusCode.Unauthorized && x.Errors!.Equals("Invalid refresh token"));
        }

        [Fact]
        public async Task Handle_ShouldThrowInvalidOperationException_WhenNewAccessTokenIsNull()
        {
            //ARRANGE
            var request = new RefreshTokenRequest { RefreshToken = DataGenerator.SeedRefreshToken() };
            var command = new RefreshTokenCommand(request, CancellationToken.None);
            var user = DataGenerator.SeedUser();
            var userRoles = new[] { "User" };
            var accessTokenPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }));
            var refreshTokenPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("exp", DateTimeOffset.UtcNow.AddMinutes(15).ToUnixTimeSeconds().ToString())
            }));

            _httpContextAccessorMock.Setup(x => x.HttpContext.Request.Headers.Authorization).Returns("Bearer validAccessToken");
            _tokenHandlerMock.Setup(x => x.ValidateAndGetPrincipalFromToken("validAccessToken")).Returns(accessTokenPrincipal);
            _tokenHandlerMock.Setup(x => x.ValidateAndGetPrincipalFromToken(request.RefreshToken)).Returns(refreshTokenPrincipal);
            _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(userRoles);
            _tokenHandlerMock.Setup(x => x.GenerateAccessToken(It.IsAny<UserSession>(), out It.Ref<string>.IsAny)).Returns((string)null);

            //ACT
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            //ASSERT
            await act.Should().ThrowAsync<AppException>()
                .Where(x => x.Code == HttpStatusCode.Unauthorized && x.Errors!.Equals("New access token cannot be null."));
        }

        [Fact]
        public async Task Handle_ShouldThrowInvalidOperationException_WhenNewRefreshTokenIsNull()
        {
            //ARRANGE
            var request = new RefreshTokenRequest { RefreshToken = DataGenerator.SeedRefreshToken() };
            var command = new RefreshTokenCommand(request, CancellationToken.None);
            var user = DataGenerator.SeedUser();
            var userRoles = new[] { "User" };
            var accessTokenPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }));
            var refreshTokenPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }));

            _httpContextAccessorMock.Setup(x => x.HttpContext.Request.Headers.Authorization).Returns("Bearer validAccessToken");
            _tokenHandlerMock.Setup(x => x.ValidateAndGetPrincipalFromToken("validAccessToken")).Returns(accessTokenPrincipal);
            _tokenHandlerMock.Setup(x => x.ValidateAndGetPrincipalFromToken(request.RefreshToken)).Returns(refreshTokenPrincipal);
            _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(userRoles);
            _tokenHandlerMock.Setup(x => x.GenerateAccessToken(It.IsAny<UserSession>(), out It.Ref<string>.IsAny)).Returns("newAccessToken");
            _tokenHandlerMock.Setup(x => x.GenerateRefreshToken(It.IsAny<UserSession>())).Returns((string)null);

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<AppException>()
                .Where(x => x.Code == HttpStatusCode.Unauthorized && x.Errors!.Equals("New refresh token cannot be null."));
        }
    }
}
