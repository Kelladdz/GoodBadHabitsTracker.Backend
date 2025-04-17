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
        private readonly Mock<IHabitsDbContext> _dbContextMock;
        private readonly Mock<IUserAccessor> _userAccessorMock;
        private readonly DeleteAllProgressCommandHandler _handler;

        public DeleteAllProgressCommandHandlerTests()
        {
            _dbContextMock = new Mock<IHabitsDbContext>();
            _userAccessorMock = new Mock<IUserAccessor>();
            _handler = new DeleteAllProgressCommandHandler(_dbContextMock.Object, _userAccessorMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenDeleteIsSuccessful()
        {
            //ARRANGE
            var user = DataGenerator.SeedUser();
            var userId = user.Id;
            var command = new DeleteAllProgressCommand();

            _userAccessorMock.Setup(x => x.GetCurrentUser()).ReturnsAsync(user);
            _dbContextMock
                .Setup(x => x.DeleteAllDayResultsAsync(userId))
                .Returns(Task.CompletedTask);

            //ACT
            var result = await _handler.Handle(command, default);

            //ASSERT
            result.IsSuccess.Should().BeTrue();

            _userAccessorMock.Verify(x => x.GetCurrentUser(), Times.Once);
            _dbContextMock.Verify(x => x.DeleteAllDayResultsAsync(userId), Times.Once);
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