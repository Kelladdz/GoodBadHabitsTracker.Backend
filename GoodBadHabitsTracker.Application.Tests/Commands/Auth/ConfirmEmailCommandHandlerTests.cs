using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.TestMisc;
using GoodBadHabitsTracker.Application.Commands.Auth.ConfirmEmail;
using GoodBadHabitsTracker.Application.DTOs.Request;
using Microsoft.AspNetCore.Identity;
using Moq;
using MediatR;
using Azure.Core;
using FluentAssertions;
using GoodBadHabitsTracker.Application.DTOs.Response;
using GoodBadHabitsTracker.Application.Exceptions;
using System.Reflection.Metadata;
using Bogus;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Runtime.InteropServices;


namespace GoodBadHabitsTracker.Application.Tests.Commands.Auth
{
    public class ConfirmEmailCommandHandlerTests
    {
        private readonly Mock<IUserStore<User>> _userStoreMock;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly ConfirmEmailCommandHandler _handler;

        public ConfirmEmailCommandHandlerTests()
        {
            _userStoreMock = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(_userStoreMock.Object, null, null, null, null, null, null, null, null);
            _handler = new ConfirmEmailCommandHandler(_userManagerMock.Object);
        }
    

        [Fact]
        public async Task Handle_ShouldReturnSuccessResult_WhenEmailConfirmed()
        {
            //ARRANGE
            var email = new Faker<string>().CustomInstantiator(f => f.Internet.Email()).Generate();
            var user = new User { Email = email, Id = Guid.NewGuid() };
            var token = DataGenerator.SeedRandomString(32);
            var command = new ConfirmEmailCommand(new ConfirmEmailRequest(user.Id, token));

            _userManagerMock.Setup(um => um.FindByIdAsync(user.Id.ToString()))
                .ReturnsAsync(user);
            _userManagerMock.Setup(um => um.ConfirmEmailAsync(user, token))
                .ReturnsAsync(IdentityResult.Success);

            //ACT
            var result = await _handler.Handle(command, CancellationToken.None);

            //ASSERT
            result.Succeeded.Should().BeTrue();


            _userManagerMock.Verify(um => um.FindByIdAsync(user.Id.ToString()), Times.Once);
            _userManagerMock.Verify(um => um.ConfirmEmailAsync(user, token), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailedResult_WhenUserNotFound()
        {
            //ARRANGE
            var email = new Faker<string>().CustomInstantiator(f => f.Internet.Email()).Generate();
            var user = new User { Email = email, Id = Guid.NewGuid() };
            var command = new ConfirmEmailCommand(new ConfirmEmailRequest(user.Id, DataGenerator.SeedRandomString(32)));

            _userManagerMock.Setup(um => um.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);

            //ACT
            var result = await _handler.Handle(command, CancellationToken.None);

            //ASSERT
            result.Succeeded.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.Code == "UserNotFound" && e.Description == $"User with id {command.Request.UserId} not found.");

            _userManagerMock.Verify(x => x.FindByIdAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailedResult_WhenEmailConfirmationFails()
        {
            //ARRANGE
            var email = new Faker<string>().CustomInstantiator(f => f.Internet.Email()).Generate();
            var user = new User { Email = email, Id = Guid.NewGuid() };
            var token = DataGenerator.SeedRandomString(32);
            var command = new ConfirmEmailCommand(new ConfirmEmailRequest(user.Id, token));

            _userManagerMock.Setup(um => um.FindByIdAsync(user.Id.ToString()))
                .ReturnsAsync(user);
            _userManagerMock.Setup(um => um.ConfirmEmailAsync(user, token))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Failed to confirm email" }));

            //ACT
            var result = await _handler.Handle(command, CancellationToken.None);

            //ASSERT
            result.Succeeded.Should().BeFalse();

            _userManagerMock.Verify(um => um.FindByIdAsync(user.Id.ToString()), Times.Once);
            _userManagerMock.Verify(um => um.ConfirmEmailAsync(user, token), Times.Once);
        }
    }
}