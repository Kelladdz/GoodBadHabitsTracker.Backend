using FluentAssertions;
using GoodBadHabitsTracker.Application.Commands.Auth.Register;
using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.TestMisc;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.Tests.Commands.Auth.Register
{
    public class HandlerTests
    {
        private readonly Mock<IUserStore<User>> _userStoreMock;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<IRoleStore<UserRole>> _roleStoreMock;
        private readonly Mock<RoleManager<UserRole>> _roleManagerMock;
        private readonly DataGenerator _dataGenerator;

        public HandlerTests()
        {
            _userStoreMock = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(_userStoreMock.Object, null, null, null, null, null, null, null, null);
            _roleStoreMock = new Mock<IRoleStore<UserRole>>();
            _roleManagerMock = new Mock<RoleManager<UserRole>>(_roleStoreMock.Object, null, null, null, null);
            _dataGenerator = new DataGenerator();
        }

        [Fact]
        public async Task Handle_WhenRequestIsValid_AndRoleExists_ReturnSuccededResult()
        {
            //ARRANGE
            var request = _dataGenerator.SeedValidRegisterRequest();
            var command = new RegisterCommand(request, default);
            var handler = new RegisterCommandHandler(_userManagerMock.Object, _roleManagerMock.Object);

            _userManagerMock.Setup(x => x.CreateAsync(It.Is<User>(user => user.Id.GetType() == typeof(Guid)), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            _roleManagerMock.Setup(x => x.RoleExistsAsync(It.IsAny<string>())).
                ReturnsAsync(true);
            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            //ACT
            var result = await handler.Handle(command, default);

            //ASSERT
            result.Should().BeAssignableTo<User>();
            result.Should().BeEquivalentTo(request, options => options.Including(x => x.Email).Including(x => x.UserName));

            _userManagerMock.Verify(x => x.CreateAsync(It.Is<User>(user => user.Id.GetType() == typeof(Guid)), It.IsAny<string>()), Times.Once);
            _roleManagerMock.Verify(x => x.RoleExistsAsync(It.IsAny<string>()), Times.Once);
            _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenRequestIsValid_AndCantCreateUser_ThrowsAppException_AndBadRequestCode()
        {
            //ARRANGE
            var request = _dataGenerator.SeedValidRegisterRequest();
            var command = new RegisterCommand(request, default);
            var handler = new RegisterCommandHandler(_userManagerMock.Object, _roleManagerMock.Object);

            _userManagerMock.Setup(x => x.CreateAsync(It.Is<User>(user => user.Id.GetType() == typeof(Guid)), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed());

            //ACT
            Func<Task> action = async () => await handler.Handle(command, default);


            //ASSERT
            await action.Should().ThrowAsync<AppException>()
                .Where(ex => ex.Code == System.Net.HttpStatusCode.BadRequest)
                .Where(ex => ex.Errors.ToString().StartsWith("Failed to create user: "));

            _userManagerMock.Verify(x => x.CreateAsync(It.Is<User>(user => user.Id.GetType() == typeof(Guid)), It.IsAny<string>()));
        }

        [Fact]
        public async Task Handle_WhenRequestIsValid_AndCantCreateRole_ThrowsAppException_AndBadRequestCode()
        {
            //ARRANGE
            var request = _dataGenerator.SeedValidRegisterRequest();
            var command = new RegisterCommand(request, default);
            var handler = new RegisterCommandHandler(_userManagerMock.Object, _roleManagerMock.Object);

            _userManagerMock.Setup(x => x.CreateAsync(It.Is<User>(user => user.Id.GetType() == typeof(Guid)), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            _roleManagerMock.Setup(x => x.RoleExistsAsync(It.IsAny<string>())).
                ReturnsAsync(false);
            _roleManagerMock.Setup(x => x.CreateAsync(It.IsAny<UserRole>()))
            .ReturnsAsync(IdentityResult.Failed());

            //ACT
            Func<Task> action = async () => await handler.Handle(command, default);

            //ASSERT
            await action.Should().ThrowAsync<AppException>()
                .Where(ex => ex.Code == System.Net.HttpStatusCode.BadRequest)
                .Where(ex => ex.Errors.ToString().StartsWith("Failed to create role: "));

            _userManagerMock.Verify(x => x.CreateAsync(It.Is<User>(user => user.Id.GetType() == typeof(Guid)), It.IsAny<string>()), Times.Once);
            _roleManagerMock.Verify(x => x.RoleExistsAsync(It.IsAny<string>()), Times.Once);
            _roleManagerMock.Verify(x => x.CreateAsync(It.IsAny<UserRole>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenRequestIsValid_AndRoleIsNotExists_CreateRoleAddUserToRole_AndReturnSuccededResult()
        {
            //ARRANGE
            var request = _dataGenerator.SeedValidRegisterRequest();
            var command = new RegisterCommand(request, default);
            var handler = new RegisterCommandHandler(_userManagerMock.Object, _roleManagerMock.Object);

            _userManagerMock.Setup(x => x.CreateAsync(It.Is<User>(user => user.Id.GetType() == typeof(Guid)), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            _roleManagerMock.Setup(x => x.RoleExistsAsync(It.IsAny<string>())).
                ReturnsAsync(false);
            _roleManagerMock.Setup(x => x.CreateAsync(It.IsAny<UserRole>()))
            .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            //ACT
            var result = await handler.Handle(command, default);


            //ASSERT
            result.Should().BeAssignableTo<User>();
            result.Should().BeEquivalentTo(request, options => options.Including(x => x.Email).Including(x => x.UserName));

            _userManagerMock.Verify(x => x.CreateAsync(It.Is<User>(user => user.Id.GetType() == typeof(Guid)), It.IsAny<string>()));
            _roleManagerMock.Verify(x => x.RoleExistsAsync(It.IsAny<string>()), Times.Once);
            _roleManagerMock.Verify(x => x.CreateAsync(It.IsAny<UserRole>()), Times.Once);
            _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenRequestIsValid_AndCantAddUserToRole_ThrowsAppException_AndBadRequestCode()
        {
            //ARRANGE
            var request = _dataGenerator.SeedValidRegisterRequest();
            var command = new RegisterCommand(request, default);
            var handler = new RegisterCommandHandler(_userManagerMock.Object, _roleManagerMock.Object);

            _userManagerMock.Setup(x => x.CreateAsync(It.Is<User>(user => user.Id.GetType() == typeof(Guid)), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            _roleManagerMock.Setup(x => x.RoleExistsAsync(It.IsAny<string>())).
                ReturnsAsync(false);
            _roleManagerMock.Setup(x => x.CreateAsync(It.IsAny<UserRole>()))
            .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed());

            //ACT
            Func<Task> action = async () => await handler.Handle(command, default);

            //ASSERT
            await action.Should().ThrowAsync<AppException>()
                .Where(ex => ex.Code == System.Net.HttpStatusCode.BadRequest)
                .Where(ex => ex.Errors.ToString().StartsWith("Failed to add user to role: "));

            _userManagerMock.Verify(x => x.CreateAsync(It.Is<User>(user => user.Id.GetType() == typeof(Guid)), It.IsAny<string>()), Times.Once);
            _roleManagerMock.Verify(x => x.RoleExistsAsync(It.IsAny<string>()), Times.Once);
            _roleManagerMock.Verify(x => x.CreateAsync(It.IsAny<UserRole>()), Times.Once);
        }
    }
}
