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
   }
}
