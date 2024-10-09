using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using GoodBadHabitsTracker.Core.Models;
using MediatR;
using GoodBadHabitsTracker.WebApi.Controllers;
using GoodBadHabitsTracker.TestMisc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Win32;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

using GoodBadHabitsTracker.Application.Exceptions;
using System.Net;
using GoodBadHabitsTracker.Application.DTOs.Auth.Request;
using GoodBadHabitsTracker.Application.DTOs.Auth.Response;
using System.Net.Http;
using GoodBadHabitsTracker.Infrastructure.Configurations;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using FluentAssertions.Common;
using System.Security.Authentication;
using GoodBadHabitsTracker.Application.Commands.Auth.Login;
using Xunit.Abstractions;

namespace GoodBadHabitsTracker.WebApi.Tests.Controllers
{
    using Register = Application.Commands.Auth.Register;
    using Login = Application.Commands.Auth.Login;
    public class AuthControllerTests
   {
        private readonly DataGenerator _dataGenerator;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly AuthController _controller;
        private readonly ITestOutputHelper _outputHelper;
        public AuthControllerTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _dataGenerator = new DataGenerator();
            _mediatorMock = new Mock<IMediator>();
            _controller = new AuthController(_mediatorMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }
        [Fact]
        public async Task Register_ValidRequest_ReturnsCreatedAtAction()
        {
            //ARRANGE
            var request = _dataGenerator.SeedValidRegisterRequest();

            _mediatorMock.Setup(x => x.Send(It.IsAny<Register.RegisterCommand>(), default)).ReturnsAsync(new User { Email = request.Email, UserName = request.UserName });

            //ACT
            var result = await _controller.Register(request, default) as CreatedAtActionResult;
            var actionName = result.ActionName;
            var routeValues = result.RouteValues;
            var value = result.Value;

            //ASSERT
            result.StatusCode.Should().Be(StatusCodes.Status201Created);
            actionName.Should().Be("Register");
            routeValues["id"].Should().BeAssignableTo<Guid>();
            value.Should().BeAssignableTo<User>();
        }

        [Fact]
        public async Task Register_InvalidRequest_ReturnsBadRequest()
        {
            //ARRANGE
            var request = _dataGenerator.SeedValidRegisterRequest();
            _controller.ModelState.AddModelError("Email", $"Email '{request.Email}' is already taken.");

            _mediatorMock.Setup(x => x.Send(It.IsAny<Register.RegisterCommand>(), default)).ThrowsAsync(new AppException(HttpStatusCode.BadRequest, $"Failed to create user: Email '{request.Email}' is already taken."));

            //ACT
            Func<Task> action = async () => await _controller.Register(request, default);

            //ASSERT
            _controller.ModelState.ErrorCount.Should().BeGreaterThan(0);
            action.Should().ThrowAsync<AppException>().Where(ex => ex.Code == HttpStatusCode.BadRequest).WithMessage($"Email '{request.Email}' is already taken.");
        }

        [Fact]
        public async Task Register_NullRequest_ReturnsBadRequest()
        {
            //ARRANGE
            RegisterRequest request = null;

            _mediatorMock.Setup(x => x.Send(It.IsAny<Register.RegisterCommand>(), default)).ReturnsAsync((User)null);

            //ACT
            Func<Task> action = async () => await _controller.Register(request, default);

            //ASSERT
            action.Should().ThrowAsync<HttpRequestException>().Where(ex => ex.StatusCode == HttpStatusCode.BadRequest);
        }
        [Fact]
        public async Task Login_CorrectCredentials_ReturnsOkWithAccessTokenRefreshToken()
        {
            //ARRANGE
            var request = _dataGenerator.SeedLoginRequest();
            var accessToken = _dataGenerator.SeedAccessToken(request.Email);
            var refreshToken = _dataGenerator.SeedRandomString(32);
            var userFingerprint = _dataGenerator.SeedRandomString(32);

            _mediatorMock.Setup(x => x.Send(It.IsAny<LoginCommand>(), default)).ReturnsAsync(new LoginResponse(accessToken, refreshToken, userFingerprint));


            //ACT
            var result = await _controller.Login(request, default) as OkObjectResult;

            //ASSERT
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
            result.Value.Should().BeEquivalentTo(new { accessToken, refreshToken });
            var properties = result.Value.GetType().GetProperties();
            properties.All(p => p.PropertyType == typeof(string)).Should().BeTrue();
            _controller.ControllerContext.HttpContext.Response.Headers.SetCookie.Should().BeEquivalentTo($"__Secure-Fgp={userFingerprint}; max-age=900; path=/; secure; samesite=strict; httponly");
        }
        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            //ARRANGE
            var request = _dataGenerator.SeedLoginRequest();

            _mediatorMock.Setup(x => x.Send(It.IsAny<LoginCommand>(), default)).ThrowsAsync(new AppException(HttpStatusCode.Unauthorized, "Invalid email or password"));

            //ACT
            Func<Task> action = async () => await _controller.Login(request, default);

            //ASSERT
            action.Should().ThrowAsync<AppException>().Where(ex => ex.Code == HttpStatusCode.Unauthorized).WithMessage("Invalid email or password");
        }
        [Fact]
        public async Task Login_NullRequest_ReturnsBadRequest()
        {
            //ARRANGE
            LoginRequest request = null;

            _mediatorMock.Setup(x => x.Send(It.IsAny<LoginCommand>(), default)).ReturnsAsync((LoginResponse)null);

            //ACT
            Func<Task> action = async () => await _controller.Login(request, default);

            //ASSERT
            action.Should().ThrowAsync<HttpRequestException>().Where(ex => ex.StatusCode == HttpStatusCode.BadRequest);
        }
    }
}
