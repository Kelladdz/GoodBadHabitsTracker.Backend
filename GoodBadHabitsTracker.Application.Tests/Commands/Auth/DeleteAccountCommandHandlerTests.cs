using GoodBadHabitsTracker.Application.Commands.Auth.DeleteAccount;
using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Core.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Moq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Bogus;
using GoodBadHabitsTracker.Application.Commands.Auth.ConfirmEmail;
using GoodBadHabitsTracker.Application.DTOs.Request;
using GoodBadHabitsTracker.TestMisc;

namespace GoodBadHabitsTracker.Application.Tests.Commands.Auth
{
    public class DeleteAccountCommandHandlerTests
    {
        private readonly Mock<IUserStore<User>> _userStoreMock;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<IUserAccessor> _userAccessorMock;
        private readonly DeleteAccountCommandHandler _handler;

        public DeleteAccountCommandHandlerTests()
        {
            _userStoreMock = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(
                _userStoreMock.Object, null, null, null, null, null, null, null, null);
            _userAccessorMock = new Mock<IUserAccessor>();
            _handler = new DeleteAccountCommandHandler(_userManagerMock.Object, _userAccessorMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnTrue_WhenUserDeletedSuccessfully()
        {
            //ARRANGE
            var email = new Faker<string>().CustomInstantiator(f => f.Internet.Email()).Generate();
            var user = new User { Email = email, Id = Guid.NewGuid() };
            var command = new DeleteAccountCommand();

            _userAccessorMock.Setup(ua => ua.GetCurrentUser())
                .ReturnsAsync(user);
            _userManagerMock.Setup(um => um.DeleteAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            //ACT
            var result = await _handler.Handle(command, default);

            //ASSERT
            result.Succeeded.Should().BeTrue();

            _userAccessorMock.Verify(ua => ua.GetCurrentUser(), Times.Once);
            _userManagerMock.Verify(um => um.DeleteAsync(user), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowAppException_WhenUserNotFound()
        {
            //ARRANGE
            var command = new DeleteAccountCommand();

            _userAccessorMock.Setup(ua => ua.GetCurrentUser())
                .ReturnsAsync((User)null);

            //ACT
            var result = await _handler.Handle(command, CancellationToken.None);

            //ASSERT
            result.Succeeded.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.Code == "CurrentUserNotFound" && e.Description == "Cannot find current user");

            _userAccessorMock.Verify(ua => ua.GetCurrentUser(), Times.Once);
            _userManagerMock.Verify(um => um.DeleteAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnFalse_WhenUserDeletionFails()
        {
            //ARRANGE
            var email = new Faker<string>().CustomInstantiator(f => f.Internet.Email()).Generate();
            var user = new User { Email = email, Id = Guid.NewGuid() };
            var command = new DeleteAccountCommand();

            _userAccessorMock.Setup(ua => ua.GetCurrentUser())
                .ReturnsAsync(user);
            _userManagerMock.Setup(um => um.DeleteAsync(user))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Deletion failed" }));

            //ACT
            var result = await _handler.Handle(command, default);

            //ASSERT
            _userAccessorMock.Verify(ua => ua.GetCurrentUser(), Times.Once);
            _userManagerMock.Verify(um => um.DeleteAsync(user), Times.Once);
        }
    }
}