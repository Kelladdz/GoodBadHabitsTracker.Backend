using FluentAssertions;
using Moq;
using Microsoft.AspNetCore.Identity;
using GoodBadHabitsTracker.Application.Commands.Auth.Login;
using GoodBadHabitsTracker.Application.DTOs.Auth.Response;
using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.TestMisc;

namespace GoodBadHabitsTracker.Application.Tests.Commands.Auth.Login
{
    public class HandlerTests
    {
        private readonly Mock<IUserStore<User>> _userStoreMock;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly DataGenerator _dataGenerator;
        private readonly Mock<IAccessTokenHandler> _accessTokenHandlerMock;
        private readonly Mock<IRefreshTokenHandler> _refreshTokenHandlerMock;

        public HandlerTests()
        {
            _userStoreMock = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(_userStoreMock.Object, null, null, null, null, null, null, null, null);
            _dataGenerator = new DataGenerator();
            _accessTokenHandlerMock = new Mock<IAccessTokenHandler>();
            _refreshTokenHandlerMock = new Mock<IRefreshTokenHandler>();
        }

        public delegate void GenerateAccessTokenCallback(UserSession userSession, out string userFingerprint);
        public delegate string GenerateAccessTokenReturns(UserSession userSession, out string userFingerprint);

        [Fact]
        public async Task Handle_CorrectCredentials_AndUserHasRole_ReturnsLoginResponse()
        {
            //ARRANGE
            var request = _dataGenerator.SeedLoginRequest();
            var command = new LoginCommand(request, default);
            var handler = new LoginCommandHandler(_userManagerMock.Object, _accessTokenHandlerMock.Object, _refreshTokenHandlerMock.Object);
            var userId = Guid.NewGuid();
            var userSession = new UserSession(userId, request.Email, request.Email, ["User"]);
            var accessToken = _dataGenerator.SeedAccessToken(request.Email);
            var refreshToken = _dataGenerator.SeedRandomString(32);
            string expectedUserFingerprint = _dataGenerator.SeedRandomString(32);

            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync(new User { Email = request.Email});
            _userManagerMock.Setup(x => x.CheckPasswordAsync(It.IsAny<User>(), request.Password))
                .ReturnsAsync(true);
            _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<User>()))
                .ReturnsAsync(new List<string>() { "User"});
            _accessTokenHandlerMock.Setup(x => x.GenerateAccessToken(It.IsAny<UserSession>(), out It.Ref<string>.IsAny))
                .Callback(new GenerateAccessTokenCallback((UserSession session, out string userFingerprint) =>
                {
                    userFingerprint = _accessTokenHandlerMock.Object.GenerateUserFingerprint();
                }))
                .Returns(new GenerateAccessTokenReturns((UserSession session, out string userFingerprint) =>
                {
                    userFingerprint = expectedUserFingerprint;
                    return accessToken;
                }));
            _refreshTokenHandlerMock.Setup(x => x.GenerateRefreshToken()).Returns(refreshToken);
            _userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<User>())).ReturnsAsync(IdentityResult.Success);

            //ACT
            var result = await handler.Handle(command, default);

            //ASSERT
            result.Should().BeAssignableTo<LoginResponse>();
            result.Should().BeEquivalentTo(new LoginResponse(accessToken, refreshToken, expectedUserFingerprint));
           
            _userManagerMock.Verify(x => x.FindByEmailAsync(request.Email), Times.Once);
            _userManagerMock.Verify(x => x.CheckPasswordAsync(It.IsAny<User>(), request.Password), Times.Once);
            _userManagerMock.Verify(x => x.GetRolesAsync(It.IsAny<User>()), Times.Once);
            _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
            _accessTokenHandlerMock.Verify(x => x.GenerateAccessToken(It.IsAny<UserSession>(), out It.Ref<string>.IsAny), Times.Once);
            _refreshTokenHandlerMock.Verify(x => x.GenerateRefreshToken(), Times.Once);
            _userManagerMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task Handle_UserDoesntExist_ThrowsAppException_AndUnauthorized()
        {
            //ARRANGE
            var request = _dataGenerator.SeedLoginRequest();
            var command = new LoginCommand(request, default);
            var handler = new LoginCommandHandler(_userManagerMock.Object, _accessTokenHandlerMock.Object, _refreshTokenHandlerMock.Object);

            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync((User)null);

            //ACT
            Func<Task> action = async () => await handler.Handle(command, default);

            //ASSERT
            await action.Should().ThrowAsync<AppException>()
                .Where(ex => ex.Code == System.Net.HttpStatusCode.Unauthorized)
                .Where(ex => ex.Errors.ToString() == "Invalid email or password");

            _userManagerMock.Verify(x => x.FindByEmailAsync(request.Email), Times.Once);
            _userManagerMock.Verify(x => x.CheckPasswordAsync(It.IsAny<User>(), request.Password), Times.Never);
            _userManagerMock.Verify(x => x.GetRolesAsync(It.IsAny<User>()), Times.Never);
            _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
            _accessTokenHandlerMock.Verify(x => x.GenerateAccessToken(It.IsAny<UserSession>(), out It.Ref<string>.IsAny), Times.Never);
            _refreshTokenHandlerMock.Verify(x => x.GenerateRefreshToken(), Times.Never);
            _userManagerMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task Handle_InvalidPassword_ThrowsAppException_AndUnauthorized()
        {
            //ARRANGE
            var request = _dataGenerator.SeedLoginRequest();
            var command = new LoginCommand(request, default);
            var handler = new LoginCommandHandler(_userManagerMock.Object, _accessTokenHandlerMock.Object, _refreshTokenHandlerMock.Object);

            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync(new User { Email = request.Email });
            _userManagerMock.Setup(x => x.CheckPasswordAsync(It.IsAny<User>(), request.Password))
                .ReturnsAsync(false);

            //ACT
            Func<Task> action = async () => await handler.Handle(command, default);

            //ASSERT
            await action.Should().ThrowAsync<AppException>()
                .Where(ex => ex.Code == System.Net.HttpStatusCode.Unauthorized)
                .Where(ex => ex.Errors.ToString() == "Invalid email or password");

            _userManagerMock.Verify(x => x.FindByEmailAsync(request.Email), Times.Once);
            _userManagerMock.Verify(x => x.CheckPasswordAsync(It.IsAny<User>(), request.Password), Times.Once);
            _userManagerMock.Verify(x => x.GetRolesAsync(It.IsAny<User>()), Times.Never);
            _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
            _accessTokenHandlerMock.Verify(x => x.GenerateAccessToken(It.IsAny<UserSession>(), out It.Ref<string>.IsAny), Times.Never);
            _refreshTokenHandlerMock.Verify(x => x.GenerateRefreshToken(), Times.Never);
            _userManagerMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task Handle_CorrectCredentials_AndUserDoesntHaveRole_AddsUserToRole_AndReturnsLoginResponse()
        {
            //ARRANGE
            var request = _dataGenerator.SeedLoginRequest();
            var command = new LoginCommand(request, default);
            var handler = new LoginCommandHandler(_userManagerMock.Object, _accessTokenHandlerMock.Object, _refreshTokenHandlerMock.Object);
            var userId = Guid.NewGuid();
            var userSession = new UserSession(userId, request.Email, request.Email, ["User"]);
            var accessToken = _dataGenerator.SeedAccessToken(request.Email);
            var refreshToken = _dataGenerator.SeedRandomString(32);
            string expectedUserFingerprint = _dataGenerator.SeedRandomString(32);

            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync(new User { Email = request.Email });
            _userManagerMock.Setup(x => x.CheckPasswordAsync(It.IsAny<User>(), request.Password))
                .ReturnsAsync(true);
            _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<User>()))
                .ReturnsAsync([]);
            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            _accessTokenHandlerMock.Setup(x => x.GenerateAccessToken(It.IsAny<UserSession>(), out It.Ref<string>.IsAny))
                .Callback(new GenerateAccessTokenCallback((UserSession session, out string userFingerprint) =>
                {
                    userFingerprint = _accessTokenHandlerMock.Object.GenerateUserFingerprint();
                }))
                .Returns(new GenerateAccessTokenReturns((UserSession session, out string userFingerprint) =>
                {
                    userFingerprint = expectedUserFingerprint;
                    return accessToken;
                }));
            _refreshTokenHandlerMock.Setup(x => x.GenerateRefreshToken()).Returns(refreshToken);
            _userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<User>())).ReturnsAsync(IdentityResult.Success);

            //ACT
            var result = await handler.Handle(command, default);

            //ASSERT
            result.Should().BeAssignableTo<LoginResponse>();
            result.Should().BeEquivalentTo(new LoginResponse(accessToken, refreshToken, expectedUserFingerprint));

            _userManagerMock.Verify(x => x.FindByEmailAsync(request.Email), Times.Once);
            _userManagerMock.Verify(x => x.CheckPasswordAsync(It.IsAny<User>(), request.Password), Times.Once);
            _userManagerMock.Verify(x => x.GetRolesAsync(It.IsAny<User>()), Times.Once);
            _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Once);
            _accessTokenHandlerMock.Verify(x => x.GenerateAccessToken(It.IsAny<UserSession>(), out It.Ref<string>.IsAny), Times.Once);
            _refreshTokenHandlerMock.Verify(x => x.GenerateRefreshToken(), Times.Once);
            _userManagerMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task Handle_CorrectCredentials_UserDoesntHaveRole_AndCannotAddUserToRole_ThrowsAppException_AndBadRequest()
        {
            //ARRANGE
            var request = _dataGenerator.SeedLoginRequest();
            var command = new LoginCommand(request, default);
            var handler = new LoginCommandHandler(_userManagerMock.Object, _accessTokenHandlerMock.Object, _refreshTokenHandlerMock.Object);
            var userId = Guid.NewGuid();

            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync(new User { Email = request.Email });
            _userManagerMock.Setup(x => x.CheckPasswordAsync(It.IsAny<User>(), request.Password))
                .ReturnsAsync(true);
            _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<User>()))
                .ReturnsAsync([]);
            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed());

            //ACT
            Func<Task> action = async () => await handler.Handle(command, default);

            //ASSERT
            await action.Should().ThrowAsync<AppException>()
                .Where(ex => ex.Code == System.Net.HttpStatusCode.BadRequest)
                .Where(ex => ex.Errors.ToString().StartsWith("Failed to add user to role: "));

            _userManagerMock.Verify(x => x.FindByEmailAsync(request.Email), Times.Once);
            _userManagerMock.Verify(x => x.CheckPasswordAsync(It.IsAny<User>(), request.Password), Times.Once);
            _userManagerMock.Verify(x => x.GetRolesAsync(It.IsAny<User>()), Times.Once);
            _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Once);
            _accessTokenHandlerMock.Verify(x => x.GenerateAccessToken(It.IsAny<UserSession>(), out It.Ref<string>.IsAny), Times.Never);
            _refreshTokenHandlerMock.Verify(x => x.GenerateRefreshToken(), Times.Never);
            _userManagerMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task Handle_CorrectCredentials_CannotGenerateAccessToken_ThrowsInvalidOperationException()
        {
            //ARRANGE
            var request = _dataGenerator.SeedLoginRequest();
            var command = new LoginCommand(request, default);
            var handler = new LoginCommandHandler(_userManagerMock.Object, _accessTokenHandlerMock.Object, _refreshTokenHandlerMock.Object);
            var userId = Guid.NewGuid();
            var userSession = new UserSession(userId, request.Email, request.Email, ["User"]);

            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync(new User { Email = request.Email });
            _userManagerMock.Setup(x => x.CheckPasswordAsync(It.IsAny<User>(), request.Password))
                .ReturnsAsync(true);
            _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<User>()))
                .ReturnsAsync(new List<string>() { "User" });
            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            _accessTokenHandlerMock.Setup(x => x.GenerateAccessToken(It.IsAny<UserSession>(), out It.Ref<string>.IsAny))
                .Returns((string)null);

            //ACT
            Func<Task> action = async () => await handler.Handle(command, default);

            //ASSERT
            await action.Should().ThrowAsync<AppException>()
                .Where(ex => ex.Code == System.Net.HttpStatusCode.Unauthorized)
                .Where(ex => ex.Errors.ToString() == "Something goes wrong. Try again.");

            _userManagerMock.Verify(x => x.FindByEmailAsync(request.Email), Times.Once);
            _userManagerMock.Verify(x => x.CheckPasswordAsync(It.IsAny<User>(), request.Password), Times.Once);
            _userManagerMock.Verify(x => x.GetRolesAsync(It.IsAny<User>()), Times.Once);
            _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
            _accessTokenHandlerMock.Verify(x => x.GenerateAccessToken(It.IsAny<UserSession>(), out It.Ref<string>.IsAny), Times.Once);
            _refreshTokenHandlerMock.Verify(x => x.GenerateRefreshToken(), Times.Never);
            _userManagerMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task Handle_CorrectCredentials_CannotGenerateRefreshToken_ThrowsInvalidOperationException()
        {
            //ARRANGE
            var request = _dataGenerator.SeedLoginRequest();
            var command = new LoginCommand(request, default);
            var handler = new LoginCommandHandler(_userManagerMock.Object, _accessTokenHandlerMock.Object, _refreshTokenHandlerMock.Object);
            var userId = Guid.NewGuid();
            var userSession = new UserSession(userId, request.Email, request.Email, ["User"]);
            var accessToken = _dataGenerator.SeedAccessToken(request.Email);
            string expectedUserFingerprint = _dataGenerator.SeedRandomString(32);

            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync(new User { Email = request.Email });
            _userManagerMock.Setup(x => x.CheckPasswordAsync(It.IsAny<User>(), request.Password))
                .ReturnsAsync(true);
            _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<User>()))
                .ReturnsAsync(new List<string>() { "User" });
            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            _accessTokenHandlerMock.Setup(x => x.GenerateAccessToken(It.IsAny<UserSession>(), out It.Ref<string>.IsAny))
                .Callback(new GenerateAccessTokenCallback((UserSession session, out string userFingerprint) =>
                {
                    userFingerprint = _accessTokenHandlerMock.Object.GenerateUserFingerprint();
                }))
                .Returns(new GenerateAccessTokenReturns((UserSession session, out string userFingerprint) =>
                {
                    userFingerprint = expectedUserFingerprint;
                    return accessToken;
                }));
            _refreshTokenHandlerMock.Setup(x => x.GenerateRefreshToken()).Returns((string)null);

            //ACT
            Func<Task> action = async () => await handler.Handle(command, default);

            //ASSERT
            await action.Should().ThrowAsync<AppException>()
                .Where(ex => ex.Code == System.Net.HttpStatusCode.Unauthorized)
                .Where(ex => ex.Errors.ToString() == "Something goes wrong. Try again.");

            _userManagerMock.Verify(x => x.FindByEmailAsync(request.Email), Times.Once);
            _userManagerMock.Verify(x => x.CheckPasswordAsync(It.IsAny<User>(), request.Password), Times.Once);
            _userManagerMock.Verify(x => x.GetRolesAsync(It.IsAny<User>()), Times.Once);
            _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
            _accessTokenHandlerMock.Verify(x => x.GenerateAccessToken(It.IsAny<UserSession>(), out It.Ref<string>.IsAny), Times.Once);
            _refreshTokenHandlerMock.Verify(x => x.GenerateRefreshToken(), Times.Once);
            _userManagerMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task Handle_CorrectCredentials_CannotUpdateUser_ThrowsInvalidOperationException()
        {
            //ARRANGE
            var request = _dataGenerator.SeedLoginRequest();
            var command = new LoginCommand(request, default);
            var handler = new LoginCommandHandler(_userManagerMock.Object, _accessTokenHandlerMock.Object, _refreshTokenHandlerMock.Object);
            var userId = Guid.NewGuid();
            var userSession = new UserSession(userId, request.Email, request.Email, ["User"]);
            var accessToken = _dataGenerator.SeedAccessToken(request.Email);
            var refreshToken = _dataGenerator.SeedRandomString(32);
            string expectedUserFingerprint = _dataGenerator.SeedRandomString(32);

            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync(new User { Email = request.Email });
            _userManagerMock.Setup(x => x.CheckPasswordAsync(It.IsAny<User>(), request.Password))
                .ReturnsAsync(true);
            _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<User>()))
                .ReturnsAsync(new List<string>() { "User" });
            _accessTokenHandlerMock.Setup(x => x.GenerateAccessToken(It.IsAny<UserSession>(), out It.Ref<string>.IsAny))
                .Callback(new GenerateAccessTokenCallback((UserSession session, out string userFingerprint) =>
                {
                    userFingerprint = _accessTokenHandlerMock.Object.GenerateUserFingerprint();
                }))
                .Returns(new GenerateAccessTokenReturns((UserSession session, out string userFingerprint) =>
                {
                    userFingerprint = expectedUserFingerprint;
                    return accessToken;
                }));
            _refreshTokenHandlerMock.Setup(x => x.GenerateRefreshToken()).Returns(refreshToken);
            _userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<User>())).ReturnsAsync(IdentityResult.Failed());

            //ACT
            Func<Task> action = async () => await handler.Handle(command, default);

            //ASSERT
            await action.Should().ThrowAsync<AppException>()
                .Where(ex => ex.Code == System.Net.HttpStatusCode.Unauthorized)
                .Where(ex => ex.Errors.ToString() == "Something goes wrong. Try again.");

            _userManagerMock.Verify(x => x.FindByEmailAsync(request.Email), Times.Once);
            _userManagerMock.Verify(x => x.CheckPasswordAsync(It.IsAny<User>(), request.Password), Times.Once);
            _userManagerMock.Verify(x => x.GetRolesAsync(It.IsAny<User>()), Times.Once);
            _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
            _accessTokenHandlerMock.Verify(x => x.GenerateAccessToken(It.IsAny<UserSession>(), out It.Ref<string>.IsAny), Times.Once);
            _refreshTokenHandlerMock.Verify(x => x.GenerateRefreshToken(), Times.Once);
            _userManagerMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);
        }
    }
}
