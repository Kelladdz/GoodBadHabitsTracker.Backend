using Moq;
using FluentAssertions;
using GoodBadHabitsTracker.Application.DTOs.Generic.Response;
using GoodBadHabitsTracker.Application.Queries.Generic.ReadById;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.TestMisc;

namespace GoodBadHabitsTracker.Application.Tests.Queries.ReadById
{
    public class ReadByIdHandlerTests
    {
        private readonly Mock<IGenericRepository<Habit>> _habitsRepositoryMock;
        private readonly Mock<IGenericRepository<Group>> _groupsRepositoryMock;
        private readonly ReadByIdQueryHandler<Habit> _habitsHandler;
        private readonly ReadByIdQueryHandler<Group> _groupsHandler;
        private readonly DataGenerator _dataGenerator;

        public ReadByIdHandlerTests()
        {
            _habitsRepositoryMock = new();
            _groupsRepositoryMock = new();
            _habitsHandler = new(_habitsRepositoryMock.Object);
            _groupsHandler = new(_groupsRepositoryMock.Object);
            _dataGenerator = new();
        }

        [Fact]
        public async Task Habit_Handle_HabitFound_ReturnsGenericResponse()
        {
            // Arrange
            var id = Guid.NewGuid();
            var command = new ReadByIdQuery<Habit>(id);
            var habit = _dataGenerator.SeedHabit();

            _habitsRepositoryMock.Setup(r => r.ReadByIdAsync(It.Is<Guid>(x => x == id), default))
                 .ReturnsAsync(habit);

            // Act
            var result = await _habitsHandler.Handle(command, default);

            // Assert
            result.Should().Be(new GenericResponse<Habit>(habit));

            _habitsRepositoryMock.Verify(r => r.ReadByIdAsync(It.Is<Guid>(x => x == id), default), Times.Once);
        }

        [Fact]
        public async Task Habit_Handle_HabitNotFound_ReturnsNull()
        {
            // Arrange
            var id = Guid.NewGuid();
            var command = new ReadByIdQuery<Habit>(id);
            var habit = (Habit)null;

            _habitsRepositoryMock.Setup(r => r.ReadByIdAsync(It.Is<Guid>(x => x == id), default))!
                 .ReturnsAsync(habit);

            // Act
            var result = await _habitsHandler.Handle(command, default);

            // Assert
            result.Should().BeNull();

            _habitsRepositoryMock.Verify(r => r.ReadByIdAsync(It.Is<Guid>(x => x == id), default), Times.Once);
        }
    }
}
