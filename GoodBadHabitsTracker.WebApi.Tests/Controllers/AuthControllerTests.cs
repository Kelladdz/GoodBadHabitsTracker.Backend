using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using FluentAssertions;
using MediatR;
using GoodBadHabitsTracker.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using GoodBadHabitsTracker.Application.Commands.Auth.Login;
using GoodBadHabitsTracker.Application.Commands.Auth.ConfirmEmail;
using GoodBadHabitsTracker.Application.Commands.Auth.DeleteAccount;
using GoodBadHabitsTracker.Application.Commands.Auth.ExternalLogin;
using GoodBadHabitsTracker.Application.Commands.Auth.ForgetPassword;
using GoodBadHabitsTracker.Application.Commands.Auth.RefreshToken;
using GoodBadHabitsTracker.Application.Commands.Auth.Register;
using GoodBadHabitsTracker.Application.Commands.Auth.ResetPassword;
using GoodBadHabitsTracker.Application.DTOs.Request;
using GoodBadHabitsTracker.Application.DTOs.Response;
using GoodBadHabitsTracker.Application.Queries.Auth.GetExternalTokens;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.TestMisc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;

namespace GoodBadHabitsTracker.WebApi.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<IEmailSender> _emailSenderMock;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _emailSenderMock = new Mock<IEmailSender>();
            _controller = new AuthController(_mediatorMock.Object, _emailSenderMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        [Fact]
        public async Task Register_ShouldReturnCreatedAtAction_WhenSuccessful()
        {
            // Arrange
            var request = new RegisterRequest();
            var user = DataGenerator.SeedUser();
            var accessToken = DataGenerator.SeedAccessToken();
            var response = new RegisterResponse(user, accessToken);

            _mediatorMock.Setup(m => m.Send(It.IsAny<RegisterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Register(request, CancellationToken.None);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
        }

        [Fact]
        public async Task Login_ShouldReturnOk_WhenSuccessful()
        {
            // Arrange
            var request = new LoginRequest();
            var accessToken = DataGenerator.SeedAccessToken();  
            var refreshToken = DataGenerator.SeedRefreshToken();
            var userFingerprint = DataGenerator.SeedRandomString(32);

            var response = new LoginResponse(accessToken, refreshToken, userFingerprint);
            _mediatorMock.Setup(m => m.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Login(request, CancellationToken.None);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task ExternalLogin_ShouldReturnOk_WhenSuccessful()
        {
            // Arrange
            var request = new ExternalLoginRequest();

            _mediatorMock.Setup(m => m.Send(It.IsAny<ExternalLoginCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.ExternalLogin(request, CancellationToken.None);

            // Assert
            result.Should().BeOfType<OkResult>();
        }

        [Fact]
        public async Task ConfirmEmail_ShouldReturnNoContent_WhenSuccessful()
        {
            // Arrange
            var user = DataGenerator.SeedUser();
            var userId = user.Id;
            var token = DataGenerator.SeedRandomString(32);
            var request = new ConfirmEmailRequest(userId, token);

            _mediatorMock.Setup(m => m.Send(It.IsAny<ConfirmEmailCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.ConfirmEmail(request, CancellationToken.None);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task ForgetPassword_ShouldReturnOk_WhenSuccessful()
        {
            // Arrange
            var request = new ForgetPasswordRequest("email@example.com");
            var user = DataGenerator.SeedUser();
            var token = DataGenerator.SeedRandomString(32);
            var response = new ForgetPasswordResponse(user, token);
            _mediatorMock.Setup(m => m.Send(It.IsAny<ForgetPasswordCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.ForgetPassword(request, CancellationToken.None);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task ResetPassword_ShouldReturnNoContent_WhenSuccessful()
        {
            // Arrange
            var request = new ResetPasswordRequest();

            _mediatorMock.Setup(m => m.Send(It.IsAny<ResetPasswordCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.ResetPassword(request, CancellationToken.None);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task RefreshToken_ShouldReturnOk_WhenSuccessful()
        {
            // Arrange
            var request = new RefreshTokenRequest();
            var accessToken = DataGenerator.SeedAccessToken();
            var refreshToken = DataGenerator.SeedRefreshToken();
            var userFingerprint = DataGenerator.SeedRandomString(32);
            var response = new LoginResponse(accessToken, refreshToken, userFingerprint);

            _mediatorMock.Setup(m => m.Send(It.IsAny<RefreshTokenCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.RefreshToken(request, CancellationToken.None);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetExternalTokens_ShouldReturnOk_WhenSuccessful()
        {
            // Arrange
            var request = new GetExternalTokensRequest();
            var provider = "provider";
            var response = new GetExternalTokensResponse();
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetExternalTokensQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetExternalTokens(request, provider, CancellationToken.None);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task DeleteAccount_ShouldReturnNoContent_WhenSuccessful()
        {
            // Arrange
            _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteAccountCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.DeleteAccount(CancellationToken.None);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }
    }
}
