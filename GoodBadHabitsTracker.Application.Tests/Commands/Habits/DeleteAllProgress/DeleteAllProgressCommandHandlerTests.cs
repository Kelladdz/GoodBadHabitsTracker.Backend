using FluentAssertions;
using GoodBadHabitsTracker.Application.Commands.Habits.DeleteAll;
using GoodBadHabitsTracker.Application.Commands.Habits.DeleteAllProgress;
using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.TestMisc;
using LanguageExt.Common;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.Tests.Commands.Habits.DeleteAllProgress
{
    public class DeleteAllProgressCommandHandlerTests
    {
        private readonly Mock<IHabitsRepository> _habitsRepositoryMock;
        private readonly Mock<IUserAccessor> _userAccessorMock;
        private readonly DeleteAllProgressCommandHandler _handler;

        public DeleteAllProgressCommandHandlerTests()
        {
            _habitsRepositoryMock = new Mock<IHabitsRepository>();
            _userAccessorMock = new Mock<IUserAccessor>();
            _handler = new DeleteAllProgressCommandHandler(_habitsRepositoryMock.Object, _userAccessorMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenDeleteIsSuccessful()
        {
            //ARRANGE
            var user = DataGenerator.SeedUser();
            var userId = user.Id;
            var command = new DeleteAllProgressCommand();

            _userAccessorMock.Setup(x => x.GetCurrentUser()).ReturnsAsync(user);
            _habitsRepositoryMock
                .Setup(x => x.DeleteAllProgressAsync(userId, default))
                .Returns(Task.CompletedTask);

            //ACT
            var result = await _handler.Handle(command, default);

            //ASSERT
            result.IsSuccess.Should().BeTrue();

            _userAccessorMock.Verify(x => x.GetCurrentUser(), Times.Once);
            _habitsRepositoryMock.Verify(x => x.DeleteAllProgressAsync(userId, default), Times.Once);
        }
        [Fact]
        public async Task Handle_ShouldReturnFail_WhenUserDoesntExist()
        {
            //ARRANGE
            var command = new DeleteAllProgressCommand();

            _userAccessorMock.Setup(x => x.GetCurrentUser()).ReturnsAsync((User?)null);

            //ACT
            var result = await _handler.Handle(command, default);

            //ASSERT
            result.IsFaulted.Should().BeTrue();
            result.Should().BeEquivalentTo(new Result<bool>(new AppException(HttpStatusCode.NotFound, "User Not Found")));


            _userAccessorMock.Verify(x => x.GetCurrentUser(), Times.Once);
        }
    }
}
