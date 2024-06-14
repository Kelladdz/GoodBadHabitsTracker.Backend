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

namespace GoodBadHabitsTracker.WebApi.Tests.Controllers
{
   public class AuthControllerTests
   {
       private readonly Mock<UserManager<User>> _userManagerMock;
       private readonly Mock<RoleManager<UserRole>> _roleManagerMock;
       private readonly Mock<IMediator> _mediatorMock;
       private readonly AuthController _controller;
       private readonly DataGenerator _dataGenerator;
       public AuthControllerTests()
       {
           var userStoreMock = new Mock<IUserStore<User>>();
           _userManagerMock = new Mock<UserManager<User>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
           var roleStoreMock = new Mock<IRoleStore<UserRole>>();
           _roleManagerMock = new Mock<RoleManager<UserRole>>(roleStoreMock.Object, null, null, null, null);
           _mediatorMock = new Mock<IMediator>();
           _controller = new AuthController(_userManagerMock.Object, _roleManagerMock.Object);
            _dataGenerator = new DataGenerator();
       }

       [Fact]
       public async Task Register_WhenRequestIsValid_AndRoleIsNotExists_CreateRole_AndReturnsCreatedAtAction()
       {
            //ARRANGE
            var request = _dataGenerator.SeedValidRegisterRequest();

            _userManagerMock.Setup(x => x.CreateAsync(It.Is<User>(user => user.Id.GetType() == typeof(Guid)), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            _roleManagerMock.Setup(x => x.RoleExistsAsync(It.IsAny<string>())).
                ReturnsAsync(false);
            _roleManagerMock.Setup(x => x.CreateAsync(It.IsAny<UserRole>()))
            .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            //ACT
            var result = await _controller.Register(request) as CreatedAtActionResult;
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
