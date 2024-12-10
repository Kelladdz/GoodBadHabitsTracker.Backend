using Bogus;
using FluentAssertions;
using GoodBadHabitsTracker.Application.Commands.Auth.ConfirmEmail;
using GoodBadHabitsTracker.Application.Commands.Auth.ExternalLogin;
using GoodBadHabitsTracker.Application.Commands.Auth.ForgetPassword;
using GoodBadHabitsTracker.Application.DTOs.Request;
using GoodBadHabitsTracker.Application.DTOs.Response;
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
using static System.Collections.Specialized.BitVector32;

namespace GoodBadHabitsTracker.Application.Tests.Commands.Auth
{
    public class ForgetPasswordCommandHandlerTests
    {
        private readonly Mock<IUserStore<User>> _userStoreMock;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly ForgetPasswordCommandHandler _handler;

        public ForgetPasswordCommandHandlerTests()
        {
            _userStoreMock = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(_userStoreMock.Object, null, null, null, null, null, null, null, null);
            _handler = new ForgetPasswordCommandHandler(_userManagerMock.Object);
        }
        [Fact]
        public async Task Handle_ShouldReturnForgetPasswordResponse_WhenUserExists()
        {
            //ARRANGE
            var email = new Faker<string>().CustomInstantiator(f => f.Internet.Email()).Generate();
            var request = new ForgetPasswordRequest(email);
            var command = new ForgetPasswordCommand(request);
            var user = DataGenerator.SeedUser();
            var token = DataGenerator.SeedRandomString(100);

            _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(user)).ReturnsAsync(token);

            //ACT
            var result = await _handler.Handle(command, CancellationToken.None);

            //ASSERT
            result.Should().NotBeNull();
            result.User.Should().Be(user);
            result.Token.Should().Be(token.Replace("+", "%2B").Replace("/", "%2F"));
        }

        [Fact]
        public async Task Handle_ShouldThrowAppException_WhenUserDoesNotExist()
        {
            //ARRANGE
            var email = new Faker<string>().CustomInstantiator(f => f.Internet.Email()).Generate();
            var request = new ForgetPasswordRequest(email);
            var command = new ForgetPasswordCommand(request);

            _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((User)null); 

            //ACT
            Func<Task> action = async () => await _handler.Handle(command, CancellationToken.None);

            //ASSERT
            await action.Should().ThrowAsync<AppException>().Where(ex => ex.Code == HttpStatusCode.BadRequest && ex.Errors!.Equals("User with this email does not exist"));
        }
        [Fact]
        public async Task Handle_ShouldThrowAppException_WhenTokenGenerationFails()
        {
            //ARRANGE
            var email = new Faker<string>().CustomInstantiator(f => f.Internet.Email()).Generate();
            var request = new ForgetPasswordRequest(email);
            var command = new ForgetPasswordCommand(request);
            var user = DataGenerator.SeedUser();

            _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(user)).ReturnsAsync((string)null);

            // Act
            Func<Task> action = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await action.Should().ThrowAsync<AppException>().Where(ex => ex.Code == HttpStatusCode.BadRequest && ex.Errors!.Equals("Failed to generate password reset token"));
        }
    }
}
