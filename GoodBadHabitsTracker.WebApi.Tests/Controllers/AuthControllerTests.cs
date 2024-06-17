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
using GoodBadHabitsTracker.Application.Commands.Auth.Register;
using GoodBadHabitsTracker.Application.Exceptions;
using System.Net;
using GoodBadHabitsTracker.Application.DTOs.Auth.Request;

namespace GoodBadHabitsTracker.WebApi.Tests.Controllers
{
   public class AuthControllerTests
   {
        private readonly DataGenerator _dataGenerator;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly AuthController _controller;
        public AuthControllerTests()
        {
            _dataGenerator = new DataGenerator();
            _mediatorMock = new Mock<IMediator>();
            _controller = new AuthController(_mediatorMock.Object);
            
        }
        [Fact]
        public async void Register_ValidRequest_ReturnsCreatedAtAction()
        {
            //ARRANGE
            var request = _dataGenerator.SeedValidRegisterRequest();

            _mediatorMock.Setup(x => x.Send(It.IsAny<Command>(), default)).ReturnsAsync(new User { Email = request.Email, UserName = request.UserName });

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
        public async void Register_InvalidRequest_ReturnsBadRequest()
        {
            //ARRANGE
            var request = _dataGenerator.SeedValidRegisterRequest();
            _controller.ModelState.AddModelError("Email", $"Email '{request.Email}' is already taken.");

            _mediatorMock.Setup(x => x.Send(It.IsAny<Command>(), default)).ThrowsAsync(new AppException(HttpStatusCode.BadRequest, $"Failed to create user: Email '{request.Email}' is already taken."));

            //ACT
            Func<Task> action = async () => await _controller.Register(request, default);

            //ASSERT
            _controller.ModelState.ErrorCount.Should().BeGreaterThan(0);
            action.Should().ThrowAsync<AppException>();
        }

        [Fact]
        public async void Register_NullRequest_ReturnsBadRequest()
        {
            //ARRANGE
            RegisterRequest request = null;

            _mediatorMock.Setup(x => x.Send(It.IsAny<Command>(), default)).ReturnsAsync((User)null);

            //ACT
            Func<Task> action = async () => await _controller.Register(request, default);

            //ASSERT
            action.Should().ThrowAsync<HttpRequestException>();
        }
    }
}
