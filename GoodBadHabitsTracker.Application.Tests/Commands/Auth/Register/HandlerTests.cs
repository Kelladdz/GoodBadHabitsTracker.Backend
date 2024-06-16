using FluentAssertions;
using GoodBadHabitsTracker.Application.Commands.Auth.Register;
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
        private readonly ServiceCollection _services;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<RoleManager<UserRole>> _roleManagerMock;
        private readonly DataGenerator _dataGenerator;

        public HandlerTests()
        {
            _services = new ServiceCollection();
            var userStoreMock = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
            var roleStoreMock = new Mock<IRoleStore<UserRole>>();
            _roleManagerMock = new Mock<RoleManager<UserRole>>(roleStoreMock.Object, null, null, null, null);
            _dataGenerator = new DataGenerator();
        }

        [Fact]
        public async Task Handle_WhenRequestIsValid_AndRoleIsNotExists_CreateRoleAddUserToRole_AndReturnSuccededResult()
        {
            //ARRANGE
            var request = _dataGenerator.SeedValidRegisterRequest();
            var command = new Command(request, default);
            var handler = new Handler(_userManagerMock.Object, _roleManagerMock.Object);

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
            _roleManagerMock.Verify(x => x.CreateAsync(It.IsAny<UserRole>()), Times.Once);
            _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Once);
            result.Should().BeAssignableTo<User>();
            result.Should().BeEquivalentTo(request, options => options.Including(x => x.Email).Including(x => x.UserName));
        }
    }
}
