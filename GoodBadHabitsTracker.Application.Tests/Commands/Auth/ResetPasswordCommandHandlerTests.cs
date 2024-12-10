using Bogus;
using FluentAssertions;
using GoodBadHabitsTracker.Application.Commands.Auth.DeleteAccount;
using GoodBadHabitsTracker.Application.Commands.Auth.ResetPassword;
using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.TestMisc;
using Microsoft.AspNetCore.Identity;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.Tests.Commands.Auth
{
    public class ResetPasswordCommandHandlerTests
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly ResetPasswordCommandHandler _handler;

        public ResetPasswordCommandHandlerTests()
        {
            _userManagerMock = new Mock<UserManager<User>>(
                Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);
            _handler = new ResetPasswordCommandHandler(_userManagerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenUserExistsAndPasswordReset()
        {
            //ARRANGE
            var request = DataGenerator.SeedResetPasswordRequest();
            var command = new ResetPasswordCommand(request);
            var user = DataGenerator.SeedUser();
            _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.ResetPasswordAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

            //ACT
            var result = await _handler.Handle(command, CancellationToken.None);

            //ASSERT
            result.Succeeded.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ShouldReturnFailed_WhenUserDoesNotExist()
        {
            //ARRANGE
            var request = DataGenerator.SeedResetPasswordRequest();
            var command = new ResetPasswordCommand(request);

            _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((User)null);

            //ACT
            var result = await _handler.Handle(command, CancellationToken.None);

            //ASSERT
            result.Succeeded.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.Code == "UserNotFoundByEmail" && e.Description == "There is no user with this email");
        }

        [Fact]
        public async Task Handle_ShouldReturnFailed_WhenPasswordResetFails()
        {
            //ARRANGE
            var request = DataGenerator.SeedResetPasswordRequest();
            var command = new ResetPasswordCommand(request);
            var user = DataGenerator.SeedUser();
            _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.ResetPasswordAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "ResetFailed", Description = "Password reset failed" }));

            //ACT
            var result = await _handler.Handle(command, CancellationToken.None);

            //ASSERTZ
            result.Succeeded.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.Code == "ResetFailed" && e.Description == "Password reset failed");
        }
    }
}
