using AutoMapper;
using Bogus;
using FluentAssertions;
using GoodBadHabitsTracker.Application.Commands.Auth.ConfirmEmail;
using GoodBadHabitsTracker.Application.Commands.Auth.ExternalLogin;
using GoodBadHabitsTracker.Application.DTOs.Request;
using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.TestMisc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.Tests.Commands.Auth
{
    public class ExternalLoginCommandHandlerTests
    {
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IJwtTokenHandler> _tokenHandlerMock;
        private readonly Mock<SignInManager<User>> _signInManagerMock;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<RoleManager<UserRole>> _roleManagerMock;
        private readonly ExternalLoginCommandHandler _handler;

        public ExternalLoginCommandHandlerTests()
        {
            _mapperMock = new Mock<IMapper>();
            _tokenHandlerMock = new Mock<IJwtTokenHandler>();

            var userStoreMock = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(userStoreMock.Object, null, null, null, null, null, null, null, null);

            var roleStoreMock = new Mock<IRoleStore<UserRole>>();
            _roleManagerMock = new Mock<RoleManager<UserRole>>(roleStoreMock.Object, null, null, null, null);

            var contextAccessorMock = new Mock<IHttpContextAccessor>();
            var userClaimsPrincipalFactoryMock = new Mock<IUserClaimsPrincipalFactory<User>>();
            _signInManagerMock = new Mock<SignInManager<User>>(_userManagerMock.Object, contextAccessorMock.Object, userClaimsPrincipalFactoryMock.Object, null, null, null, null);

            _handler = new ExternalLoginCommandHandler(
                _mapperMock.Object,
                _tokenHandlerMock.Object,
                _signInManagerMock.Object,
                _userManagerMock.Object,
                _roleManagerMock.Object);
        }
        [Fact]
        public async Task Handle_ShouldReturnTokens_WhenExternalAuthTokensUpdates()
        {
            //ARRANGE
            var email = new Faker<string>().CustomInstantiator(f => f.Internet.Email()).Generate();
            var request = DataGenerator.SeedExternalLoginRequest();
            var command = new ExternalLoginCommand(request);
            var userFingerprint = "usr_fgp";
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new Claim("sub", "providerKey"),
                new Claim("email", email)
            }));
            _tokenHandlerMock.Setup(x => x.GetClaimsPrincipalFromIdToken(It.IsAny<string>())).Returns(claimsPrincipal);
            _signInManagerMock.Setup(x => x.ExternalLoginSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(SignInResult.Success);
            _userManagerMock.Setup(x => x.FindByLoginAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new User{ Email = email});
            _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<User>())).ReturnsAsync(new List<string> { "User" });
            _roleManagerMock.Setup(x => x.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(true);
            _roleManagerMock.Setup(x => x.CreateAsync(It.IsAny<UserRole>())).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            _signInManagerMock.Setup(x => x.UpdateExternalAuthenticationTokensAsync(It.IsAny<ExternalLoginInfo>())).ReturnsAsync(IdentityResult.Success);

            //ACT
            var result = await _handler.Handle(command, CancellationToken.None);

            //ASSERT
            result.Should().BeEquivalentTo(new
            {
                request.AccessToken,
                request.RefreshToken,
                userFingerprint
            });

            _tokenHandlerMock.Verify(x => x.GetClaimsPrincipalFromIdToken(It.IsAny<string>()), Times.Once);
            _signInManagerMock.Verify(x => x.ExternalLoginSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
            _userManagerMock.Verify(x => x.FindByLoginAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _userManagerMock.Verify(x => x.GetRolesAsync(It.IsAny<User>()), Times.Once);
            _signInManagerMock.Verify(x => x.UpdateExternalAuthenticationTokensAsync(It.IsAny<ExternalLoginInfo>()), Times.Once);
        }
        [Fact]
        public async Task Handle_ShouldThrowAppException_WhenRequestIsNull()
        {
            //ARRANGE
            var command = new ExternalLoginCommand(null);

            //ACT
            Func<Task> action = async () =>  await _handler.Handle(command, default);

            //ASSERT
            await action.Should().ThrowAsync<AppException>()
                .Where(ex => ex.Code == System.Net.HttpStatusCode.BadRequest)
                .Where(ex => ex.Errors.ToString() == "Request cannot be null");
        }

        [Fact]
        public async Task Handle_ShouldThrowAppException_WhenProviderIsInvalid()
        {
            //ARRANGE
            var request = new ExternalLoginRequest
            {
                Provider = "InvalidProvider"
            };
            var command = new ExternalLoginCommand(request);

            //ACT
            Func<Task> action = async () =>  await _handler.Handle(command, default);

            //ASSERT
            await action.Should().ThrowAsync<AppException>()
                .Where(ex => ex.Code == System.Net.HttpStatusCode.BadRequest)
                .Where(ex => ex.Errors.ToString() == "Provider is not correct");
        }

        [Fact]
        public async Task Handle_ShouldThrowAppException_WhenIdTokenIsNull()
        {
            //ARRANGE
            var request = new ExternalLoginRequest
            {
                Provider = "Google",
                IdToken = null
            };
            var command = new ExternalLoginCommand(request);

            //ACT
            Func<Task> action = async () =>  await _handler.Handle(command, default);

            //ASSERT
            await action.Should().ThrowAsync<AppException>()
                .Where(ex => ex.Code == System.Net.HttpStatusCode.BadRequest)
                .Where(ex => ex.Errors.ToString() == "IdToken cannot be null");
        }

        [Fact]
        public async Task Handle_ShouldReturnFailedResult_WhenAccessTokenIsNull()
        {
            //ARRANGE
            var email = new Faker<string>().CustomInstantiator(f => f.Internet.Email()).Generate();
            var request = new ExternalLoginRequest
            {
                Provider = "Google",
                IdToken = DataGenerator.SeedIdToken(email),
                AccessToken = null
            };
            var command = new ExternalLoginCommand(request);

            //ACT
            Func<Task> action = async () =>  await _handler.Handle(command, default);

            //ASSERT
            await action.Should().ThrowAsync<AppException>()
                .Where(ex => ex.Code == System.Net.HttpStatusCode.BadRequest)
                .Where(ex => ex.Errors.ToString() == "Access token cannot be null");
        }

        [Fact]
        public async Task Handle_ShouldReturnFailedResult_WhenRefreshTokenIsNullForGoogle()
        {
            //ARRANGE
            var email = new Faker<string>().CustomInstantiator(f => f.Internet.Email()).Generate();
            var request = new ExternalLoginRequest
            {
                Provider = "Google",
                IdToken = DataGenerator.SeedIdToken(email),
                AccessToken = DataGenerator.SeedAccessToken(),
                RefreshToken = null
            };
            var command = new ExternalLoginCommand(request);

            //ACT
            Func<Task> action = async () =>  await _handler.Handle(command, default);

            //ASSERT
            await action.Should().ThrowAsync<AppException>()
                .Where(ex => ex.Code == System.Net.HttpStatusCode.BadRequest)
                .Where(ex => ex.Errors.ToString() == "Google must return refresh token");
        }

        [Fact]
        public async Task Handle_ShouldReturnFailedResult_WhenClaimsPrincipalIsNull()
        {
            //ARRANGE
            var request = DataGenerator.SeedExternalLoginRequest();
            var command = new ExternalLoginCommand(request);

            _tokenHandlerMock.Setup(x => x.GetClaimsPrincipalFromIdToken(It.IsAny<string>())).Returns((ClaimsPrincipal)null);

            //ACT
            Func<Task> action = async () =>  await _handler.Handle(command, default);

            //ASSERT
            await action.Should().ThrowAsync<AppException>()
                .Where(ex => ex.Code == System.Net.HttpStatusCode.BadRequest)
                .Where(ex => ex.Errors.ToString() == "Claims principal cannot be null");

            _tokenHandlerMock.Verify(x => x.GetClaimsPrincipalFromIdToken(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailedResult_WhenProviderKeyIsNull()
        {
            //ARRANGE
            var request = DataGenerator.SeedExternalLoginRequest();
            var command = new ExternalLoginCommand(request);
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>()));

            _tokenHandlerMock.Setup(x => x.GetClaimsPrincipalFromIdToken(It.IsAny<string>())).Returns(claimsPrincipal);

            //ACT
            Func<Task> action = async () =>  await _handler.Handle(command, default);

            //ASSERT
            await action.Should().ThrowAsync<AppException>()
                .Where(ex => ex.Code == System.Net.HttpStatusCode.BadRequest)
                .Where(ex => ex.Errors.ToString() == "Provider key cannot be null");

            _tokenHandlerMock.Verify(x => x.GetClaimsPrincipalFromIdToken(It.IsAny<string>()), Times.Once);
        }
        [Fact]
        public async Task Handle_ShouldReturnFailedResult_WhenUserLoginNotFound()
        {
            //ARRANGE
            var request = DataGenerator.SeedExternalLoginRequest();
            var command = new ExternalLoginCommand(request);
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new Claim("sub", "providerKey")
            }));

            _tokenHandlerMock.Setup(x => x.GetClaimsPrincipalFromIdToken(It.IsAny<string>())).Returns(claimsPrincipal);
            _signInManagerMock.Setup(x => x.ExternalLoginSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(SignInResult.Success);
            _userManagerMock.Setup(x => x.FindByLoginAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((User)null);

            //ACT
            Func<Task> action = async () =>  await _handler.Handle(command, default);

            //ASSERT
            await action.Should().ThrowAsync<AppException>()
                .Where(ex => ex.Code == System.Net.HttpStatusCode.BadRequest)
                .Where(ex => ex.Errors.ToString() == "User not found");

            _tokenHandlerMock.Verify(x => x.GetClaimsPrincipalFromIdToken(It.IsAny<string>()), Times.Once);
            _signInManagerMock.Verify(x => x.ExternalLoginSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
            _userManagerMock.Verify(x => x.FindByLoginAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailedResult_WhenRoleCreationFails()
        {
            //ARRANGE
            var email = new Faker<string>().CustomInstantiator(f => f.Internet.Email()).Generate();
            var request = DataGenerator.SeedExternalLoginRequest();
            var command = new ExternalLoginCommand(request);
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new Claim("sub", "providerKey"),
                new Claim("email", email)
            }));

            _tokenHandlerMock.Setup(x => x.GetClaimsPrincipalFromIdToken(It.IsAny<string>())).Returns(claimsPrincipal);
            _signInManagerMock.Setup(x => x.ExternalLoginSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(SignInResult.Success);
            _userManagerMock.Setup(x => x.FindByLoginAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new User{ Email = email});
            _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<User>())).ReturnsAsync(new List<string>());
            _roleManagerMock.Setup(x => x.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _roleManagerMock.Setup(x => x.CreateAsync(It.IsAny<UserRole>())).ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "RoleCreationFailed" }));

            //ACT
            Func<Task> action = async () =>  await _handler.Handle(command, default);

            //ASSERT
            await action.Should().ThrowAsync<AppException>()
                .Where(ex => ex.Code == System.Net.HttpStatusCode.BadRequest)
                .Where(ex => ex.Errors.ToString() == "Create role failed");

            _tokenHandlerMock.Verify(x => x.GetClaimsPrincipalFromIdToken(It.IsAny<string>()), Times.Once);
            _signInManagerMock.Verify(x => x.ExternalLoginSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
            _userManagerMock.Verify(x => x.FindByLoginAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _userManagerMock.Verify(x => x.GetRolesAsync(It.IsAny<User>()), Times.Once);
            _roleManagerMock.Verify(x => x.RoleExistsAsync(It.IsAny<string>()), Times.Once);
            _roleManagerMock.Verify(x => x.CreateAsync(It.IsAny<UserRole>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailedResult_WhenUserSignedInAndAddUserToRoleFails()
        {
            //ARRANGE
            var email = new Faker<string>().CustomInstantiator(f => f.Internet.Email()).Generate();
            var request = DataGenerator.SeedExternalLoginRequest();
            var command = new ExternalLoginCommand(request);
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new Claim("sub", "providerKey"),
                new Claim("email", email)
            }));

            _tokenHandlerMock.Setup(x => x.GetClaimsPrincipalFromIdToken(It.IsAny<string>())).Returns(claimsPrincipal);
            _signInManagerMock.Setup(x => x.ExternalLoginSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(SignInResult.Success);
            _userManagerMock.Setup(x => x.FindByLoginAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new User { Email = email });
            _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<User>())).ReturnsAsync(new List<string>());
            _roleManagerMock.Setup(x => x.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _roleManagerMock.Setup(x => x.CreateAsync(It.IsAny<UserRole>())).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "AddToRoleFailed" }));

            //ACT
            Func<Task> action = async () =>  await _handler.Handle(command, default);

            //ASSERT
            await action.Should().ThrowAsync<AppException>()
                .Where(ex => ex.Code == System.Net.HttpStatusCode.BadRequest)
                .Where(ex => ex.Errors.ToString() == "Add role failed");

            _tokenHandlerMock.Verify(x => x.GetClaimsPrincipalFromIdToken(It.IsAny<string>()), Times.Once);
            _signInManagerMock.Verify(x => x.ExternalLoginSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
            _userManagerMock.Verify(x => x.FindByLoginAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _userManagerMock.Verify(x => x.GetRolesAsync(It.IsAny<User>()), Times.Once);
            _roleManagerMock.Verify(x => x.RoleExistsAsync(It.IsAny<string>()), Times.Once);
            _roleManagerMock.Verify(x => x.CreateAsync(It.IsAny<UserRole>()), Times.Once);
            _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Once);
        }
        [Fact]
        public async Task Handle_ShouldReturnFailedResult_WhenCreateUserFails()
        {
            //ARRANGE
            var email = new Faker<string>().CustomInstantiator(f => f.Internet.Email()).Generate();
            var request = DataGenerator.SeedExternalLoginRequest();
            var command = new ExternalLoginCommand(request);
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new Claim("sub", "providerKey"),
                new Claim("sub", Guid.NewGuid().ToString()),
                new Claim("email", email),
                new Claim("name", new Faker<string>().CustomInstantiator(f => f.Internet.UserName())),
                new Claim("picture", new Faker<string>().CustomInstantiator(f => f.Internet.Avatar()))

            }));

            _tokenHandlerMock.Setup(x => x.GetClaimsPrincipalFromIdToken(It.IsAny<string>())).Returns(claimsPrincipal);
            _signInManagerMock.Setup(x => x.ExternalLoginSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(SignInResult.Failed);
            _userManagerMock.Setup(x => x.FindByEmailAsync(email)).ReturnsAsync((User)null);
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>())).ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "CreateUserFailed" }));

            //ACT
            Func<Task> action = async () =>  await _handler.Handle(command, default);

            //ASSERT
            await action.Should().ThrowAsync<AppException>()
                .Where(ex => ex.Code == System.Net.HttpStatusCode.BadRequest)
                .Where(ex => ex.Errors.ToString() == "Create user failed");

            _tokenHandlerMock.Verify(x => x.GetClaimsPrincipalFromIdToken(It.IsAny<string>()), Times.Once);
            _signInManagerMock.Verify(x => x.ExternalLoginSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
            _userManagerMock.Verify(x => x.FindByEmailAsync(email), Times.Once);
            _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Once);
        }
        [Fact]
        public async Task Handle_ShouldReturnFailedResult_WhenAddClaimFails()
        {
            //ARRANGE
            var email = new Faker<string>().CustomInstantiator(f => f.Internet.Email()).Generate();
            var request = DataGenerator.SeedExternalLoginRequest();
            var command = new ExternalLoginCommand(request);
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                {
                                   new Claim("sub", "providerKey"),
                new Claim("sub", Guid.NewGuid().ToString()),
                new Claim("email", email),
                new Claim("name", new Faker<string>().CustomInstantiator(f => f.Internet.UserName())),
                new Claim("picture", new Faker<string>().CustomInstantiator(f => f.Internet.Avatar()))

                }
            ));

            _tokenHandlerMock.Setup(x => x.GetClaimsPrincipalFromIdToken(It.IsAny<string>())).Returns(claimsPrincipal);
            _signInManagerMock.Setup(x => x.ExternalLoginSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(SignInResult.Failed);
            _userManagerMock.Setup(x => x.FindByEmailAsync(email)).ReturnsAsync((User)null);
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>())).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.AddClaimAsync(It.IsAny<User>(), It.IsAny<Claim>())).ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "AddClaimFailed" }));

            //ACT
            Func<Task> action = async () =>  await _handler.Handle(command, default);

            //ASSERT
            await action.Should().ThrowAsync<AppException>()
                .Where(ex => ex.Code == System.Net.HttpStatusCode.BadRequest)
                .Where(ex => ex.Errors.ToString() == "Add claim failed");

            _tokenHandlerMock.Verify(x => x.GetClaimsPrincipalFromIdToken(It.IsAny<string>()), Times.Once);
            _signInManagerMock.Verify(x => x.ExternalLoginSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
            _userManagerMock.Verify(x => x.FindByEmailAsync(email), Times.Once);
            _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Once);
            _userManagerMock.Verify(x => x.AddClaimAsync(It.IsAny<User>(), It.IsAny<Claim>()), Times.Once);
        }
        [Fact]
        public async Task Handle_ShouldReturnFailedResult_WhenAddLoginFails()
        {
            //ARRANGE
            var email = new Faker<string>().CustomInstantiator(f => f.Internet.Email()).Generate();
            var request = DataGenerator.SeedExternalLoginRequest();
            var command = new ExternalLoginCommand(request);
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                {
                                   new Claim("sub", "providerKey"),
                new Claim("sub", Guid.NewGuid().ToString()),
                new Claim("email", email),
                new Claim("name", new Faker<string>().CustomInstantiator(f => f.Internet.UserName())),
                new Claim("picture", new Faker<string>().CustomInstantiator(f => f.Internet.Avatar()))

                }
            ));

            _tokenHandlerMock.Setup(x => x.GetClaimsPrincipalFromIdToken(It.IsAny<string>())).Returns(claimsPrincipal);
            _signInManagerMock.Setup(x => x.ExternalLoginSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(SignInResult.Failed);
            _userManagerMock.Setup(x => x.FindByEmailAsync(email)).ReturnsAsync((User)null);
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>())).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.AddClaimAsync(It.IsAny<User>(), It.IsAny<Claim>())).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.AddLoginAsync(It.IsAny<User>(), It.IsAny<ExternalLoginInfo>())).ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "AddLoginFailed" }));

            //ACT
            Func<Task> action = async () =>  await _handler.Handle(command, default);

            //ASSERT
            await action.Should().ThrowAsync<AppException>()
                .Where(ex => ex.Code == System.Net.HttpStatusCode.BadRequest)
                .Where(ex => ex.Errors.ToString() == "Add login failed");

            _tokenHandlerMock.Verify(x => x.GetClaimsPrincipalFromIdToken(It.IsAny<string>()), Times.Once);
            _signInManagerMock.Verify(x => x.ExternalLoginSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
            _userManagerMock.Verify(x => x.FindByEmailAsync(email), Times.Once);
            _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Once);
            _userManagerMock.Verify(x => x.AddClaimAsync(It.IsAny<User>(), It.IsAny<Claim>()), Times.Once);
        }
        [Fact]
        public async Task Handle_ShouldReturnFailedResult_WhenUserSignInFailsAndAddUserToRoleFails()
        {
            //ARRANGE
            var email = new Faker<string>().CustomInstantiator(f => f.Internet.Email()).Generate();
            var request = DataGenerator.SeedExternalLoginRequest();
            var command = new ExternalLoginCommand(request);
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new Claim("sub", "providerKey"),
                                new Claim("sub", Guid.NewGuid().ToString()),
                new Claim("email", email),
                new Claim("name", new Faker<string>().CustomInstantiator(f => f.Internet.UserName())),
                new Claim("picture", new Faker<string>().CustomInstantiator(f => f.Internet.Avatar()))

            }));

            _tokenHandlerMock.Setup(x => x.GetClaimsPrincipalFromIdToken(It.IsAny<string>())).Returns(claimsPrincipal);
            _signInManagerMock.Setup(x => x.ExternalLoginSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(SignInResult.Failed);
            _userManagerMock.Setup(x => x.FindByEmailAsync(email)).ReturnsAsync((User)null);
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>())).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.AddClaimAsync(It.IsAny<User>(), It.IsAny<Claim>())).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.AddLoginAsync(It.IsAny<User>(), It.IsAny<ExternalLoginInfo>())).ReturnsAsync(IdentityResult.Success);
           _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<User>())).ReturnsAsync(new List<string>());
            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "AddToRoleFailed" }));
            //ACT
            Func<Task> action = async () =>  await _handler.Handle(command, default);

            //ASSERT
            await action.Should().ThrowAsync<AppException>()
                .Where(ex => ex.Code == System.Net.HttpStatusCode.BadRequest)
                .Where(ex => ex.Errors.ToString() == "Add role failed");

            _tokenHandlerMock.Verify(x => x.GetClaimsPrincipalFromIdToken(It.IsAny<string>()), Times.Once);
            _signInManagerMock.Verify(x => x.ExternalLoginSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
            _userManagerMock.Verify(x => x.FindByEmailAsync(email), Times.Once);
            _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Once);
            _userManagerMock.Verify(x => x.AddClaimAsync(It.IsAny<User>(), It.IsAny<Claim>()), Times.Once);
            _userManagerMock.Verify(x => x.AddLoginAsync(It.IsAny<User>(), It.IsAny<ExternalLoginInfo>()), Times.Once);
            _userManagerMock.Verify(x => x.GetRolesAsync(It.IsAny<User>()), Times.Once);
            _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Once);
        }
        [Fact]
        public async Task Handle_ShouldReturnFailedResult_WhenUserSecondSignInFails()
        {
            //ARRANGE
            var email = new Faker<string>().CustomInstantiator(f => f.Internet.Email()).Generate();
            var request = DataGenerator.SeedExternalLoginRequest();
            var command = new ExternalLoginCommand(request);
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                                new Claim("sub", "providerKey"),
                new Claim("sub", Guid.NewGuid().ToString()),
                new Claim("email", email),
                new Claim("name", new Faker<string>().CustomInstantiator(f => f.Internet.UserName())),
                new Claim("picture", new Faker<string>().CustomInstantiator(f => f.Internet.Avatar()))

            }));

            _tokenHandlerMock.Setup(x => x.GetClaimsPrincipalFromIdToken(It.IsAny<string>())).Returns(claimsPrincipal);
            _signInManagerMock.Setup(x => x.ExternalLoginSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(SignInResult.Failed);
            _userManagerMock.Setup(x => x.FindByEmailAsync(email)).ReturnsAsync((User)null);
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>())).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.AddClaimAsync(It.IsAny<User>(), It.IsAny<Claim>())).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.AddLoginAsync(It.IsAny<User>(), It.IsAny<ExternalLoginInfo>())).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<User>())).ReturnsAsync(new List<string>());
            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            _signInManagerMock.Setup(x => x.ExternalLoginSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(SignInResult.Failed);
            //ACT
            Func<Task> action = async () =>  await _handler.Handle(command, default);

            //ASSERT
            await action.Should().ThrowAsync<AppException>()
                .Where(ex => ex.Code == System.Net.HttpStatusCode.BadRequest)
                .Where(ex => ex.Errors.ToString() == "External Login failed");

            _tokenHandlerMock.Verify(x => x.GetClaimsPrincipalFromIdToken(It.IsAny<string>()), Times.Once);
            _signInManagerMock.Verify(x => x.ExternalLoginSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Exactly(2));
            _userManagerMock.Verify(x => x.FindByEmailAsync(email), Times.Once);
            _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Once);
            _userManagerMock.Verify(x => x.AddClaimAsync(It.IsAny<User>(), It.IsAny<Claim>()), Times.Once);
            _userManagerMock.Verify(x => x.AddLoginAsync(It.IsAny<User>(), It.IsAny<ExternalLoginInfo>()), Times.Once);
            _userManagerMock.Verify(x => x.GetRolesAsync(It.IsAny<User>()), Times.Once);
            _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Once);
        }
        [Fact]
        public async Task Handle_ShouldReturnFailedResult_WhenEmailIsNull()
        {
            //ARRANGE
            var request = DataGenerator.SeedExternalLoginRequest();
            var command = new ExternalLoginCommand(request);
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new Claim("sub", "providerKey")
            }));

            _tokenHandlerMock.Setup(x => x.GetClaimsPrincipalFromIdToken(It.IsAny<string>())).Returns(claimsPrincipal);
            _signInManagerMock.Setup(x => x.ExternalLoginSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(SignInResult.Failed);

            //ACT
            Func<Task> action = async () =>  await _handler.Handle(command, default);

            //ASSERT
            await action.Should().ThrowAsync<AppException>()
                .Where(ex => ex.Code == System.Net.HttpStatusCode.BadRequest)
                .Where(ex => ex.Errors.ToString() == "Email cannot be null");

            _tokenHandlerMock.Verify(x => x.GetClaimsPrincipalFromIdToken(It.IsAny<string>()), Times.Once);
            _signInManagerMock.Verify(x => x.ExternalLoginSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
        }
    }
}