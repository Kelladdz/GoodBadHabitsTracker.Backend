/*using Moq;
using FluentAssertions;
using GoodBadHabitsTracker.Application.Commands.Generic.Delete;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.TestMisc;

namespace GoodBadHabitsTracker.Application.Tests.Commands.Generic.Delete
{
    public class DeleteCommandHandlerTests
    {
        private readonly Mock<IGenericRepository<Habit>> _habitsRepositoryMock;
        private readonly Mock<IGenericRepository<Group>> _groupsRepositoryMock;
        private readonly DeleteCommandHandler<Habit> _habitsHandler;
        private readonly DeleteCommandHandler<Group> _groupsHandler;
        private readonly DataGenerator _dataGenerator;

        public DeleteCommandHandlerTests()
        {
            _habitsRepositoryMock = new();
            _groupsRepositoryMock = new();
            _habitsHandler = new(_habitsRepositoryMock.Object);
            _groupsHandler = new(_groupsRepositoryMock.Object);
            _dataGenerator = new();
        }

        [Fact]
        public async Task Habit_Handle_EntityNotDeleted_ReturnsFalse()
        {
            //ARRANGE
            var command = new DeleteCommand<Habit>(Guid.NewGuid());

            _habitsRepositoryMock.Setup(r => r.DeleteAsync(It.Is<Guid>(x => x == command.Id), default))
                                  .ReturnsAsync(false);

            //ACT
            var result = await _habitsHandler.Handle(command, default);

            //ASSERT
            result.Should().BeFalse();

            _habitsRepositoryMock.Verify(r => r.DeleteAsync(It.Is<Guid>(x => x == command.Id), default), Times.Once);
        }

        [Fact]
        public async Task Habit_Handle_EntityDeleted_ReturnsTrue()
        {
            //ARRANGE
            var command = new DeleteCommand<Habit>(Guid.NewGuid());

            _habitsRepositoryMock.Setup(r => r.DeleteAsync(It.Is<Guid>(x => x == command.Id), default))
                                  .ReturnsAsync(true);

            //ACT
            var result = await _habitsHandler.Handle(command, default);

            //ASSERT
            result.Should().BeTrue();

            _habitsRepositoryMock.Verify(r => r.DeleteAsync(It.Is<Guid>(x => x == command.Id), default), Times.Once);
        }

        [Fact]
        public async Task Group_Handle_EntityNotDeleted_ReturnsFalse()
        {
            //ARRANGE
            var command = new DeleteCommand<Group>(Guid.NewGuid());

            _groupsRepositoryMock.Setup(r => r.DeleteAsync(It.Is<Guid>(x => x == command.Id), default))
                                  .ReturnsAsync(false);

            //ACT
            var result = await _groupsHandler.Handle(command, default);

            //ASSERT
            result.Should().BeFalse();

            _groupsRepositoryMock.Verify(r => r.DeleteAsync(It.Is<Guid>(x => x == command.Id), default), Times.Once);
        }

        [Fact]
        public async Task Group_Handle_EntityDeleted_ReturnsTrue()
        {
            //ARRANGE
            var command = new DeleteCommand<Group>(Guid.NewGuid());

            _groupsRepositoryMock.Setup(r => r.DeleteAsync(It.Is<Guid>(x => x == command.Id), default))
                                  .ReturnsAsync(true);

            //ACT
            var result = await _groupsHandler.Handle(command, default);

            //ASSERT
            result.Should().BeTrue();

            _groupsRepositoryMock.Verify(r => r.DeleteAsync(It.Is<Guid>(x => x == command.Id), default), Times.Once);
        }
    }
}
*/