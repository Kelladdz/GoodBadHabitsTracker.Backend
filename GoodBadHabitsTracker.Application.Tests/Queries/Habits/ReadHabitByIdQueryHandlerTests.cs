using FluentAssertions;
using GoodBadHabitsTracker.Application.Queries.Groups.ReadById;
using GoodBadHabitsTracker.Application.Queries.Habits.ReadById;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.TestMisc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.Tests.Queries.Habits
{
    public class ReadHabitByIdQueryHandlerTests
    {
        private readonly Mock<IHabitsRepository> _habitsRepositoryMock;
        private readonly ReadHabitByIdQueryHandler _handler;

        public ReadHabitByIdQueryHandlerTests()
        {
            _habitsRepositoryMock = new Mock<IHabitsRepository>();
            _handler = new ReadHabitByIdQueryHandler(_habitsRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_HabitNotFound_ReturnsNotFound()
        {
            // Arrange
            var query = new ReadHabitByIdQuery(Guid.NewGuid());
            _habitsRepositoryMock.Setup(x => x.ReadByIdAsync(query.Id, It.IsAny<CancellationToken>())).ReturnsAsync((Habit?)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsFaulted.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_HabitFound_ReturnsGroupResponse()
        {
            // Arrange
            var habit = DataGenerator.SeedHabit();
            var habitId = habit.Id;
            var query = new ReadHabitByIdQuery(habitId);

            _habitsRepositoryMock.Setup(x => x.ReadByIdAsync(query.Id, It.IsAny<CancellationToken>())).ReturnsAsync(habit);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.IfSucc(response =>
            {
                response.Habit.Id.Should().Be(habitId);
                response.Habit.Name.Should().Be(habit.Name);
            });
        }
    }
}

