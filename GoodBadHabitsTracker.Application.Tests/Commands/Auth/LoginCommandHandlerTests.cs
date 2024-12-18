using FluentAssertions;
using Moq;
using Microsoft.AspNetCore.Identity;
using GoodBadHabitsTracker.Application.Commands.Auth.Login;
using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.TestMisc;
using GoodBadHabitsTracker.Application.DTOs.Response;

namespace GoodBadHabitsTracker.Application.Tests.Commands.Auth
{
    public class LoginCommandHandlerTests
    {
        private readonly Mock<IUserStore<User>> _userStoreMock;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<IJwtTokenHandler> _tokenHandlerMock;
        private readonly LoginCommandHandler _handler;

        public LoginCommandHandlerTests()
        {
            _userStoreMock = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(_userStoreMock.Object, null, null, null, null, null, null, null, null);
            _tokenHandlerMock = new Mock<IJwtTokenHandler>();
            _handler = new LoginCommandHandler(_userManagerMock.Object, _tokenHandlerMock.Object);
        }

        public delegate void GenerateAccessTokenCallback(UserSession userSession, out string userFingerprint);
        public delegate string GenerateAccessTokenReturns(UserSession userSession, out string userFingerprint);

        [Fact]
        public async Task Handle_CorrectCredentials_AndUserHasRole_ReturnsLoginResponse()
        {
            //ARRANGE
            var request = DataGenerator.SeedLoginRequest();
            var command = new LoginCommand(request);
            var user = DataGenerator.SeedUser();
            var userSession = new UserSession(user.Id, user.UserName!, user.Email, ["User"]);
            var accessToken = DataGenerator.SeedAccessToken();
            var refreshToken = DataGenerator.SeedRefreshToken();
            string expectedUserFingerprint = DataGenerator.SeedRandomString(32);

            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync(new User { Email = request.Email });
            _userManagerMock.Setup(x => x.CheckPasswordAsync(It.IsAny<User>(), request.Password))
                .ReturnsAsync(true);
            _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<User>()))
                .ReturnsAsync(new List<string>() { "User" });
            _tokenHandlerMock.Setup(x => x.GenerateAccessToken(It.IsAny<UserSession>(), out It.Ref<string>.IsAny))
                .Callback(new GenerateAccessTokenCallback((UserSession session, out string userFingerprint) =>
                {
                    userFingerprint = _tokenHandlerMock.Object.GenerateUserFingerprint();
                }))
                .Returns(new GenerateAccessTokenReturns((UserSession userSession, out string userFingerprint) =>
                {
                    userFingerprint = expectedUserFingerprint;
                    return accessToken;
                }));
            _tokenHandlerMock.Setup(x => x.GenerateRefreshToken(It.IsAny<UserSession>())).Returns(refreshToken);
            

            //ACT
            var result = await _handler.Handle(command, default);

            //ASSERT
            result.Should().BeAssignableTo<LoginResponse>();
            result.Should().BeEquivalentTo(new LoginResponse(accessToken, refreshToken, expectedUserFingerprint));

            _userManagerMock.Verify(x => x.FindByEmailAsync(request.Email), Times.Once);
            _userManagerMock.Verify(x => x.CheckPasswordAsync(It.IsAny<User>(), request.Password), Times.Once);
            _userManagerMock.Verify(x => x.GetRolesAsync(It.IsAny<User>()), Times.Once);
            _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
            _tokenHandlerMock.Verify(x => x.GenerateAccessToken(It.IsAny<UserSession>(), out It.Ref<string>.IsAny), Times.Once);
            _tokenHandlerMock.Verify(x => x.GenerateRefreshToken(It.IsAny<UserSession>()));
        }

        [Fact]
        public async Task Handle_UserDoesntExist_ThrowsAppExceptionAndUnauthorized()
        {
            //ARRANGE
            var request = DataGenerator.SeedLoginRequest();
            var command = new LoginCommand(request);

            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync((User)null);

            //ACT
            Func<Task> action = async () => await _handler.Handle(command, default);

            //ASSERT
            await action.Should().ThrowAsync<AppException>()
                .Where(ex => ex.Code == System.Net.HttpStatusCode.Unauthorized)
                .Where(ex => ex.Errors!.Equals("Invalid email or password"));

            _userManagerMock.Verify(x => x.FindByEmailAsync(request.Email), Times.Once);
        }

        [Fact]
        public async Task Handle_InvalidPassword_ThrowsAppException_AndUnauthorized()
        {
            //ARRANGE
            var request = DataGenerator.SeedLoginRequest();
            var command = new LoginCommand(request);

            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync(new User { Email = request.Email });
            _userManagerMock.Setup(x => x.CheckPasswordAsync(It.IsAny<User>(), request.Password))
                .ReturnsAsync(false);

            //ACT
            Func<Task> action = async () => await _handler.Handle(command, default);

            //ASSERT
            await action.Should().ThrowAsync<AppException>()
                .Where(ex => ex.Code == System.Net.HttpStatusCode.Unauthorized)
                .Where(ex => ex.Errors!.Equals("Invalid email or password"));


            _userManagerMock.Verify(x => x.FindByEmailAsync(request.Email), Times.Once);
            _userManagerMock.Verify(x => x.CheckPasswordAsync(It.IsAny<User>(), request.Password), Times.Once);
        }

        [Fact]
        public async Task Handle_CorrectCredentials_AndUserDoesntHaveRole_AddsUserToRole_AndReturnsLoginResponse()
        {
            //ARRANGE
            var request = DataGenerator.SeedLoginRequest();
            var command = new LoginCommand(request);
            var userId = Guid.NewGuid();
            var userSession = new UserSession(userId, request.Email, request.Email, ["User"]);
            var accessToken = DataGenerator.SeedAccessToken();
            var refreshToken = DataGenerator.SeedRandomString(32);
            string expectedUserFingerprint = DataGenerator.SeedRandomString(32);

            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync(new User { Email = request.Email });
            _userManagerMock.Setup(x => x.CheckPasswordAsync(It.IsAny<User>(), request.Password))
                .ReturnsAsync(true);
            _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<User>()))
                .ReturnsAsync([]);
            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            _tokenHandlerMock.Setup(x => x.GenerateAccessToken(It.IsAny<UserSession>(), out It.Ref<string>.IsAny))
                .Callback(new GenerateAccessTokenCallback((UserSession session, out string userFingerprint) =>
                {
                    userFingerprint = _tokenHandlerMock.Object.GenerateUserFingerprint();
                }))
                .Returns(new GenerateAccessTokenReturns((UserSession session, out string userFingerprint) =>
                {
                    userFingerprint = expectedUserFingerprint;
                    return accessToken;
                }));
            _tokenHandlerMock.Setup(x => x.GenerateRefreshToken((It.IsAny<UserSession>()))).Returns(refreshToken);

            //ACT
            var result = await _handler.Handle(command, default);

            //ASSERT
            result.Should().BeAssignableTo<LoginResponse>();
            result.Should().BeEquivalentTo(new LoginResponse(accessToken, refreshToken, expectedUserFingerprint));

            _userManagerMock.Verify(x => x.FindByEmailAsync(request.Email), Times.Once);
            _userManagerMock.Verify(x => x.CheckPasswordAsync(It.IsAny<User>(), request.Password), Times.Once);
            _userManagerMock.Verify(x => x.GetRolesAsync(It.IsAny<User>()), Times.Once);
            _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Once);
            _tokenHandlerMock.Verify(x => x.GenerateAccessToken(It.IsAny<UserSession>(), out It.Ref<string>.IsAny), Times.Once);
            _tokenHandlerMock.Verify(x => x.GenerateRefreshToken(It.IsAny<UserSession>()), Times.Once);
        }

        [Fact]
        public async Task Handle_CorrectCredentials_UserDoesntHaveRole_AndCannotAddUserToRole_ThrowsAppException_AndBadRequest()
        {
            //ARRANGE
            var request = DataGenerator.SeedLoginRequest();
            var command = new LoginCommand(request);
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
            Func<Task> action = async () => await _handler.Handle(command, default);

            //ASSERT
            await action.Should().ThrowAsync<AppException>()
                .Where(ex => ex.Code == System.Net.HttpStatusCode.BadRequest)
                .Where(ex => ex.Errors!.Equals("Failed to add user to role: "));

            _userManagerMock.Verify(x => x.FindByEmailAsync(request.Email), Times.Once);
            _userManagerMock.Verify(x => x.CheckPasswordAsync(It.IsAny<User>(), request.Password), Times.Once);
            _userManagerMock.Verify(x => x.GetRolesAsync(It.IsAny<User>()), Times.Once);
            _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Once);
            _tokenHandlerMock.Verify(x => x.GenerateAccessToken(It.IsAny<UserSession>(), out It.Ref<string>.IsAny), Times.Never);
            _tokenHandlerMock.Verify(x => x.GenerateRefreshToken(It.IsAny<UserSession>()), Times.Never);
        }

        [Fact]
        public async Task Handle_CorrectCredentials_CannotGenerateAccessToken_ThrowsInvalidOperationException()
        {
            //ARRANGE
            var request = DataGenerator.SeedLoginRequest();
            var command = new LoginCommand(request);
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
            _tokenHandlerMock.Setup(x => x.GenerateAccessToken(It.IsAny<UserSession>(), out It.Ref<string>.IsAny))
                .Returns((string)null);

            //ACT
            Func<Task> action = async () => await _handler.Handle(command, default);

            //ASSERT
            await action.Should().ThrowAsync<AppException>()
                .Where(ex => ex.Code == System.Net.HttpStatusCode.Unauthorized)
                .Where(ex => ex.Errors!.Equals("Something goes wrong. Try again."));

            _userManagerMock.Verify(x => x.FindByEmailAsync(request.Email), Times.Once);
            _userManagerMock.Verify(x => x.CheckPasswordAsync(It.IsAny<User>(), request.Password), Times.Once);
            _userManagerMock.Verify(x => x.GetRolesAsync(It.IsAny<User>()), Times.Once);
            _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
            _tokenHandlerMock.Verify(x => x.GenerateAccessToken(It.IsAny<UserSession>(), out It.Ref<string>.IsAny), Times.Once);
            _tokenHandlerMock.Verify(x => x.GenerateRefreshToken(It.IsAny<UserSession>()), Times.Never);
        }

        [Fact]
        public async Task Handle_CorrectCredentials_CannotGenerateRefreshToken_ThrowsInvalidOperationException()
        {
            //ARRANGE
            var request = DataGenerator.SeedLoginRequest();
            var command = new LoginCommand(request);
            var userId = Guid.NewGuid();
            var userSession = new UserSession(userId, request.Email, request.Email, ["User"]);
            var accessToken = DataGenerator.SeedAccessToken();
            var expectedUserFingerprint = DataGenerator.SeedRandomString(32);

            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync(new User { Email = request.Email });
            _userManagerMock.Setup(x => x.CheckPasswordAsync(It.IsAny<User>(), request.Password))
                .ReturnsAsync(true);
            _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<User>()))
                .ReturnsAsync(new List<string>() { "User" });
            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            _tokenHandlerMock.Setup(x => x.GenerateAccessToken(It.IsAny<UserSession>(), out It.Ref<string>.IsAny))
                .Callback(new GenerateAccessTokenCallback((UserSession session, out string userFingerprint) =>
                {
                    userFingerprint = _tokenHandlerMock.Object.GenerateUserFingerprint();
                }))
                .Returns(new GenerateAccessTokenReturns((UserSession session, out string userFingerprint) =>
                {
                    userFingerprint = expectedUserFingerprint;
                    return accessToken;
                }));
            _tokenHandlerMock.Setup(x => x.GenerateRefreshToken(It.IsAny<UserSession>())).Returns((string)null);

            //ACT
            Func<Task> action = async () => await _handler.Handle(command, default);

            //ASSERT
            await action.Should().ThrowAsync<AppException>()
                .Where(ex => ex.Code == System.Net.HttpStatusCode.Unauthorized)
                .Where(ex => ex.Errors.ToString() == "Something goes wrong. Try again.");

            _userManagerMock.Verify(x => x.FindByEmailAsync(request.Email), Times.Once);
            _userManagerMock.Verify(x => x.CheckPasswordAsync(It.IsAny<User>(), request.Password), Times.Once);
            _userManagerMock.Verify(x => x.GetRolesAsync(It.IsAny<User>()), Times.Once);
            _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
            _tokenHandlerMock.Verify(x => x.GenerateAccessToken(It.IsAny<UserSession>(), out It.Ref<string>.IsAny), Times.Once);
            _tokenHandlerMock.Verify(x => x.GenerateRefreshToken(It.IsAny<UserSession>()), Times.Once);
        }
    }
}
