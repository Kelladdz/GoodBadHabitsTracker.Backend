using FluentAssertions;
using AutoMapper;
using GoodBadHabitsTracker.TestMisc;
using Moq;
using GoodBadHabitsTracker.Application.Commands.Generic.Insert;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.Core.Interfaces;

namespace GoodBadHabitsTracker.Application.Tests.Commands.Generic.Insert
{
    public class InsertCommandHandlerTests
    {
        /*private readonly Mock<IGenericRepository<Habit>> _habitsRepositoryMock;
        private readonly Mock<IGenericRepository<Group>> _groupsRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly InsertCommandHandler<Habit, HabitRequest> _habitsHandler;
        private readonly InsertCommandHandler<Group, GroupRequest> _groupsHandler;
        private readonly DataGenerator _dataGenerator;

        public InsertCommandHandlerTests()
        {
            _habitsRepositoryMock = new();
            _groupsRepositoryMock = new();
            _mapperMock = new();
            _habitsHandler = new(_habitsRepositoryMock.Object, _mapperMock.Object);
            _groupsHandler = new(_groupsRepositoryMock.Object, _mapperMock.Object);
            _dataGenerator = new();
        }

        [Fact]
        public async Task Habit_Handle_EntityIsInserted_ReturnsGenericResponse()
        {
            // Arrange
            var request = new InsertCommand<Habit, HabitRequest>(new HabitRequest());
            var habit = _dataGenerator.SeedHabit();

            _mapperMock.Setup(m => m.Map<Habit>(It.IsAny<HabitRequest>())).Returns(habit);
            _habitsRepositoryMock.Setup(r => r.InsertAsync(It.IsAny<Habit>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(habit);

            // Act
            var result = await _habitsHandler.Handle(request, default);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<GenericResponse<Habit>>();

            _mapperMock.Verify(m => m.Map<Habit>(It.IsAny<HabitRequest>()), Times.Once);
            _habitsRepositoryMock.Verify(r => r.InsertAsync(It.IsAny<Habit>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Habit_Handle_EntityIsNotInserted_ReturnsNull()
        {
            // Arrange
            var request = new InsertCommand<Habit, HabitRequest>(new HabitRequest());
            var habit = _dataGenerator.SeedHabit();

            _mapperMock.Setup(m => m.Map<Habit>(It.IsAny<HabitRequest>())).Returns(habit);
            _habitsRepositoryMock.Setup(r => r.InsertAsync(It.IsAny<Habit>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                  .ReturnsAsync((Habit)null);

            // Act
            var result = await _habitsHandler.Handle(request, default);

            // Assert
            result.Should().BeNull();

            _mapperMock.Verify(m => m.Map<Habit>(It.IsAny<HabitRequest>()), Times.Once);
            _habitsRepositoryMock.Verify(r => r.InsertAsync(It.IsAny<Habit>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Group_Handle_EntityIsInserted_ReturnsGenericResponse()
        {
            // Arrange
            var command = new InsertCommand<Group, GroupRequest>(new GroupRequest() { Name = _dataGenerator.SeedRandomString(10) });
            var group = _dataGenerator.SeedGroup();
            group.Name = command.Request.Name;

            _mapperMock.Setup(m => m.Map<Group>(It.IsAny<GroupRequest>())).Returns(group);
            _groupsRepositoryMock.Setup(r => r.InsertAsync(It.IsAny<Group>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(group);

            // Act
            var result = await _groupsHandler.Handle(command, default);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<GenericResponse<Group>>();

            _mapperMock.Verify(m => m.Map<Group>(It.IsAny<GroupRequest>()), Times.Once);
            _groupsRepositoryMock.Verify(r => r.InsertAsync(It.IsAny<Group>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Group_Handle_EntityIsNotInserted_ReturnsNull()
        {
            // Arrange
            var command = new InsertCommand<Group, GroupRequest>(new GroupRequest() { Name = _dataGenerator.SeedRandomString(10)});
            var group = _dataGenerator.SeedGroup();
            group.Name = command.Request.Name;

            _mapperMock.Setup(m => m.Map<Group>(It.IsAny<GroupRequest>())).Returns(group);
            _groupsRepositoryMock.Setup(r => r.InsertAsync(It.IsAny<Group>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                  .ReturnsAsync((Group)null);

            // Act
            var result = await _groupsHandler.Handle(command, default);

            // Assert
            result.Should().BeNull();

            _mapperMock.Verify(m => m.Map<Group>(It.IsAny<GroupRequest>()), Times.Once);
            _groupsRepositoryMock.Verify(r => r.InsertAsync(It.IsAny<Group>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        }*/
    }
}
