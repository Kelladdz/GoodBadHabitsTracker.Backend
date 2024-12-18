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
        private readonly Mock<IIdTokenHandler> _idTokenHandlerMock;
        private readonly Mock<IJwtTokenHandler> _tokenHandlerMock;
        private readonly Mock<SignInManager<User>> _signInManagerMock;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<RoleManager<UserRole>> _roleManagerMock;
        private readonly ExternalLoginCommandHandler _handler;

        public ExternalLoginCommandHandlerTests()
        {
            _mapperMock = new Mock<IMapper>();
            _idTokenHandlerMock = new Mock<IIdTokenHandler>();
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
                _idTokenHandlerMock.Object,
                _tokenHandlerMock.Object,
                _signInManagerMock.Object,
                _userManagerMock.Object,
                _roleManagerMock.Object);
        }
        [Fact]
        public async Task Handle_ShouldReturnSuccessResult_WhenExternalAuthTokensUpdates()
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
            _idTokenHandlerMock.Setup(x => x.GetClaimsPrincipalFromIdToken(It.IsAny<string>())).Returns(claimsPrincipal);
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
            result.Succeeded.Should().BeTrue();

            _idTokenHandlerMock.Verify(x => x.GetClaimsPrincipalFromIdToken(It.IsAny<string>()), Times.Once);
            _signInManagerMock.Verify(x => x.ExternalLoginSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
            _userManagerMock.Verify(x => x.FindByLoginAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _userManagerMock.Verify(x => x.GetRolesAsync(It.IsAny<User>()), Times.Once);
            _signInManagerMock.Verify(x => x.UpdateExternalAuthenticationTokensAsync(It.IsAny<ExternalLoginInfo>()), Times.Once);
        }
        [Fact]
        public async Task Handle_ShouldReturnFailedResult_WhenRequestIsNull()
        {
            //ARRANGE
            var command = new ExternalLoginCommand(null);

            //ACT
            var result = await _handler.Handle(command, CancellationToken.None);

            //ASSERT
            result.Succeeded.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.Code == "NullRequest" && e.Description == "Request cannot be null");
        }

        [Fact]
        public async Task Handle_ShouldReturnFailedResult_WhenProviderIsInvalid()
        {
            //ARRANGE
            var request = new ExternalLoginRequest
            {
                Provider = "InvalidProvider"
            };
            var command = new ExternalLoginCommand(request);

            //ACT
            var result = await _handler.Handle(command, CancellationToken.None);

            //ASSERT
            result.Succeeded.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.Code == "InvalidProvider" && e.Description == "Provider is not correct");
        }

        [Fact]
        public async Task Handle_ShouldReturnFailedResult_WhenIdTokenIsNull()
        {
            //ARRANGE
            var request = new ExternalLoginRequest
            {
                Provider = "Google",
                IdToken = null
            };
            var command = new ExternalLoginCommand(request);

            //ACT
            var result = await _handler.Handle(command, CancellationToken.None);

            //ASSERT
            result.Succeeded.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.Code == "NullIdToken" && e.Description == "IdToken cannot be null");
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
            var result = await _handler.Handle(command, CancellationToken.None);

            //ASSERT
            result.Succeeded.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.Code == "NullAccessToken" && e.Description == "Access token cannot be null");
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
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.Code == "NullRefreshToken" && e.Description == "Google must return refresh token");
        }

        [Fact]
        public async Task Handle_ShouldReturnFailedResult_WhenClaimsPrincipalIsNull()
        {
            //ARRANGE
            var request = DataGenerator.SeedExternalLoginRequest();
            var command = new ExternalLoginCommand(request);

            _idTokenHandlerMock.Setup(x => x.GetClaimsPrincipalFromIdToken(It.IsAny<string>())).Returns((ClaimsPrincipal)null);

            //ACT
            var result = await _handler.Handle(command, CancellationToken.None);

            //ASSERT
            result.Succeeded.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.Code == "NullClaimsPrincipal" && e.Description == "Claims principal cannot be null");

            _idTokenHandlerMock.Verify(x => x.GetClaimsPrincipalFromIdToken(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailedResult_WhenProviderKeyIsNull()
        {
            //ARRANGE
            var request = DataGenerator.SeedExternalLoginRequest();
            var command = new ExternalLoginCommand(request);
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>()));

            _idTokenHandlerMock.Setup(x => x.GetClaimsPrincipalFromIdToken(It.IsAny<string>())).Returns(claimsPrincipal);

            //ACT
            var result = await _handler.Handle(command, CancellationToken.None);

            //ASSERT
            result.Succeeded.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.Code == "NullProviderKey" && e.Description == "Provider key cannot be null");
        
            _idTokenHandlerMock.Verify(x => x.GetClaimsPrincipalFromIdToken(It.IsAny<string>()), Times.Once);
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

            _idTokenHandlerMock.Setup(x => x.GetClaimsPrincipalFromIdToken(It.IsAny<string>())).Returns(claimsPrincipal);
            _signInManagerMock.Setup(x => x.ExternalLoginSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(SignInResult.Success);
            _userManagerMock.Setup(x => x.FindByLoginAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((User)null);

            //ACT
            var result = await _handler.Handle(command, CancellationToken.None);

            //ASSERT
            result.Succeeded.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.Code == "UserLoginNotFound" && e.Description == "User not found");
        
            _idTokenHandlerMock.Verify(x => x.GetClaimsPrincipalFromIdToken(It.IsAny<string>()), Times.Once);
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

            _idTokenHandlerMock.Setup(x => x.GetClaimsPrincipalFromIdToken(It.IsAny<string>())).Returns(claimsPrincipal);
            _signInManagerMock.Setup(x => x.ExternalLoginSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(SignInResult.Success);
            _userManagerMock.Setup(x => x.FindByLoginAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new User{ Email = email});
            _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<User>())).ReturnsAsync(new List<string>());
            _roleManagerMock.Setup(x => x.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _roleManagerMock.Setup(x => x.CreateAsync(It.IsAny<UserRole>())).ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "RoleCreationFailed" }));

            //ACT
            var result = await _handler.Handle(command, CancellationToken.None);

            //ASSERT
            result.Succeeded.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.Code == "RoleCreationFailed");
        
            _idTokenHandlerMock.Verify(x => x.GetClaimsPrincipalFromIdToken(It.IsAny<string>()), Times.Once);
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

            _idTokenHandlerMock.Setup(x => x.GetClaimsPrincipalFromIdToken(It.IsAny<string>())).Returns(claimsPrincipal);
            _signInManagerMock.Setup(x => x.ExternalLoginSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(SignInResult.Success);
            _userManagerMock.Setup(x => x.FindByLoginAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new User { Email = email });
            _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<User>())).ReturnsAsync(new List<string>());
            _roleManagerMock.Setup(x => x.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _roleManagerMock.Setup(x => x.CreateAsync(It.IsAny<UserRole>())).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "AddToRoleFailed" }));
            //ACT
            var result = await _handler.Handle(command, CancellationToken.None);

            //ASSERT
            result.Succeeded.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.Code == "AddToRoleFailed");
        
            _idTokenHandlerMock.Verify(x => x.GetClaimsPrincipalFromIdToken(It.IsAny<string>()), Times.Once);
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

            _idTokenHandlerMock.Setup(x => x.GetClaimsPrincipalFromIdToken(It.IsAny<string>())).Returns(claimsPrincipal);
            _signInManagerMock.Setup(x => x.ExternalLoginSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(SignInResult.Failed);
            _userManagerMock.Setup(x => x.FindByEmailAsync(email)).ReturnsAsync((User)null);
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>())).ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "CreateUserFailed" }));

            //ACT
            var result = await _handler.Handle(command, CancellationToken.None);

            //ASSERT
            result.Succeeded.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.Code == "CreateUserFailed");
       
            _idTokenHandlerMock.Verify(x => x.GetClaimsPrincipalFromIdToken(It.IsAny<string>()), Times.Once);
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

            _idTokenHandlerMock.Setup(x => x.GetClaimsPrincipalFromIdToken(It.IsAny<string>())).Returns(claimsPrincipal);
            _signInManagerMock.Setup(x => x.ExternalLoginSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(SignInResult.Failed);
            _userManagerMock.Setup(x => x.FindByEmailAsync(email)).ReturnsAsync((User)null);
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>())).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.AddClaimAsync(It.IsAny<User>(), It.IsAny<Claim>())).ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "AddClaimFailed" }));
            //ACT
            var result = await _handler.Handle(command, CancellationToken.None);

            //ASSERT
            result.Succeeded.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.Code == "AddClaimFailed");
       
            _idTokenHandlerMock.Verify(x => x.GetClaimsPrincipalFromIdToken(It.IsAny<string>()), Times.Once);
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

            _idTokenHandlerMock.Setup(x => x.GetClaimsPrincipalFromIdToken(It.IsAny<string>())).Returns(claimsPrincipal);
            _signInManagerMock.Setup(x => x.ExternalLoginSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(SignInResult.Failed);
            _userManagerMock.Setup(x => x.FindByEmailAsync(email)).ReturnsAsync((User)null);
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>())).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.AddClaimAsync(It.IsAny<User>(), It.IsAny<Claim>())).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.AddLoginAsync(It.IsAny<User>(), It.IsAny<ExternalLoginInfo>())).ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "AddLoginFailed" }));
            //ACT
            var result = await _handler.Handle(command, CancellationToken.None);

            //ASSERT
            result.Succeeded.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.Code == "AddLoginFailed");

            _idTokenHandlerMock.Verify(x => x.GetClaimsPrincipalFromIdToken(It.IsAny<string>()), Times.Once);
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

            _idTokenHandlerMock.Setup(x => x.GetClaimsPrincipalFromIdToken(It.IsAny<string>())).Returns(claimsPrincipal);
            _signInManagerMock.Setup(x => x.ExternalLoginSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(SignInResult.Failed);
            _userManagerMock.Setup(x => x.FindByEmailAsync(email)).ReturnsAsync((User)null);
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>())).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.AddClaimAsync(It.IsAny<User>(), It.IsAny<Claim>())).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.AddLoginAsync(It.IsAny<User>(), It.IsAny<ExternalLoginInfo>())).ReturnsAsync(IdentityResult.Success);
           _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<User>())).ReturnsAsync(new List<string>());
            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "AddToRoleFailed" }));
            //ACT
            var result = await _handler.Handle(command, CancellationToken.None);

            //ASSERT
            result.Succeeded.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.Code == "AddToRoleFailed");

            _idTokenHandlerMock.Verify(x => x.GetClaimsPrincipalFromIdToken(It.IsAny<string>()), Times.Once);
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

            _idTokenHandlerMock.Setup(x => x.GetClaimsPrincipalFromIdToken(It.IsAny<string>())).Returns(claimsPrincipal);
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
            var result = await _handler.Handle(command, CancellationToken.None);

            //ASSERT
            result.Succeeded.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.Code == "ExternalLoginFailed" && e.Description == "External Login failed");
       
            _idTokenHandlerMock.Verify(x => x.GetClaimsPrincipalFromIdToken(It.IsAny<string>()), Times.Once);
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

            _idTokenHandlerMock.Setup(x => x.GetClaimsPrincipalFromIdToken(It.IsAny<string>())).Returns(claimsPrincipal);
            _signInManagerMock.Setup(x => x.ExternalLoginSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(SignInResult.Failed);

            //ACT
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.Code == "NullEmail" && e.Description == "Email cannot be null");
      
            _idTokenHandlerMock.Verify(x => x.GetClaimsPrincipalFromIdToken(It.IsAny<string>()), Times.Once);
            _signInManagerMock.Verify(x => x.ExternalLoginSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
        }
    }
}
