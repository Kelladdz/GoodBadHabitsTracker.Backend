using FluentAssertions;
using GoodBadHabitsTracker.Application.Commands.Generic.Update;
using GoodBadHabitsTracker.Application.Commands.Groups.Delete;
using GoodBadHabitsTracker.Application.Commands.Habits.Update;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.TestMisc;
using Microsoft.AspNetCore.JsonPatch;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.Tests.Commands.Habits.UpdateHabit
{
    public class UpdateHabitCommandHandlerTests
    {
        private readonly Mock<IHabitsRepository> _habitsRepositoryMock;
        private readonly UpdateHabitCommandHandler _handler;

        public UpdateHabitCommandHandlerTests()
        {
            _habitsRepositoryMock = new Mock<IHabitsRepository>();
            _handler = new UpdateHabitCommandHandler(_habitsRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenHabitUpdated()
        {
            //ARRANGE
            var document = DataGenerator.SeedHabitJsonPatchDocument();
            var habitToUpdate = DataGenerator.SeedHabit();
            var habitId = habitToUpdate.Id;
            var command = new UpdateHabitCommand(habitId, document);

            _habitsRepositoryMock.Setup(r => r.FindAsync(habitId, default))
                                  .ReturnsAsync(habitToUpdate);
            _habitsRepositoryMock.Setup(r => r.UpdateAsync(document, habitToUpdate, default))
                                  .ReturnsAsync(true);

            //ACT
            var result = await _handler.Handle(command, default);

            //ASSERT
            result.IsSuccess.Should().BeTrue();

            _habitsRepositoryMock.Verify(r => r.UpdateAsync(document, habitToUpdate, default), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenHabitIsNull()
        {
            //ARRANGE
            var document = DataGenerator.SeedHabitJsonPatchDocument();
            var habitId = Guid.NewGuid();
            var command = new UpdateHabitCommand(habitId, document);

            _habitsRepositoryMock.Setup(r => r.FindAsync(habitId, default))
                                  .ReturnsAsync((Habit?)null);


            //ACT
            var result = await _handler.Handle(command, default);

            //ASSERT
            result.IsFaulted.Should().BeTrue();

            _habitsRepositoryMock.Verify(r => r.FindAsync(habitId, default), Times.Once);
        }

        
    }
}
