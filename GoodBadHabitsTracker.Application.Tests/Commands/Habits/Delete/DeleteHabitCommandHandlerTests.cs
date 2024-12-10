using Bogus;
using FluentAssertions;
using GoodBadHabitsTracker.Application.Commands.Generic.Delete;
using GoodBadHabitsTracker.Application.Commands.Groups.Delete;
using GoodBadHabitsTracker.Application.Commands.Habits.Delete;
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

namespace GoodBadHabitsTracker.Application.Tests.Commands.Habits.Delete
{
    public class DeleteHabitCommandHandlerTests
    {
        private readonly Mock<IHabitsRepository> _habitsRepositoryMock;
        private readonly DeleteHabitCommandHandler _handler;

        public DeleteHabitCommandHandlerTests()
        {
            _habitsRepositoryMock = new Mock<IHabitsRepository>();
            _handler = new DeleteHabitCommandHandler(_habitsRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenDeleteIsSuccessful()
        {
            //ARRANGE
            var habitToDelete = DataGenerator.SeedHabit();
            var habitId = habitToDelete.Id;
            var command = new DeleteHabitCommand(habitId);

            _habitsRepositoryMock
                .Setup(x => x.FindAsync(habitId, default))
                .ReturnsAsync(habitToDelete);
            _habitsRepositoryMock
                .Setup(x => x.DeleteAsync(habitToDelete, default))
                .Returns(Task.CompletedTask);

            //ACT
            var result = await _handler.Handle(command, default);

            //ASSERT
            result.IsSuccess.Should().BeTrue();

            _habitsRepositoryMock.Verify(x => x.FindAsync(habitId, default), Times.Once);
            _habitsRepositoryMock.Verify(x => x.DeleteAsync(habitToDelete, default), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenHabitDoesntExist()
        {
            //ARRANGE
            var habitId = Guid.NewGuid();
            var command = new DeleteHabitCommand(habitId);

            _habitsRepositoryMock
                .Setup(x => x.FindAsync(habitId, default))
                .ReturnsAsync((Habit?)null);

            //ACT
            var result = await _handler.Handle(command, default);

            //ASSERT
            result.IsFaulted.Should().BeTrue();
            result.Should().BeEquivalentTo(new Result<bool>(new AppException(HttpStatusCode.NotFound, "Habit Not Found")));

            _habitsRepositoryMock.Verify(x => x.FindAsync(habitId, default), Times.Once);
        }
    }
}
