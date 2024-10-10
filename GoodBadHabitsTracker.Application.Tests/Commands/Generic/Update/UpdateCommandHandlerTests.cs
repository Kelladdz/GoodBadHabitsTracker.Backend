using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.JsonPatch;
using GoodBadHabitsTracker.Application.Commands.Generic.Update;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.TestMisc;

namespace GoodBadHabitsTracker.Application.Tests.Commands.Generic.Update
{
    public class UpdateCommandHandlerTests
    {
        private readonly Mock<IGenericRepository<Habit>> _habitsRepositoryMock;
        private readonly Mock<IGenericRepository<Group>> _groupsRepositoryMock;
        private readonly UpdateCommandHandler<Habit> _habitsHandler;
        private readonly UpdateCommandHandler<Group> _groupsHandler;
        private readonly DataGenerator _dataGenerator;

        public UpdateCommandHandlerTests()
        {
            _habitsRepositoryMock = new();
            _groupsRepositoryMock = new();
            _habitsHandler = new(_habitsRepositoryMock.Object);
            _groupsHandler = new(_groupsRepositoryMock.Object);
            _dataGenerator = new();
        }

        [Fact]
        public async Task Habit_Handle_EntityNotUpdated_ReturnsFalse()
        {
            //ARRANGE
            var document = _dataGenerator.SeedHabitJsonPatchDocument();
            var id = Guid.NewGuid();
            var command = new UpdateCommand<Habit>(id, document);

            _habitsRepositoryMock.Setup(r => r.UpdateAsync(It.Is<JsonPatchDocument<Habit>>(x => x == document), It.Is<Guid>(x => x == id), default))
                                  .ReturnsAsync(false);


            //ACT
            var result = await _habitsHandler.Handle(command, default);

            //ASSERT
            result.Should().BeFalse();

            _habitsRepositoryMock.Verify(r => r.UpdateAsync(It.Is<JsonPatchDocument<Habit>>(x => x == document), It.Is<Guid>(x => x == id), default), Times.Once);
        }

        [Fact]
        public async Task Habit_Handle_EntityUpdated_ReturnsTrue()
        {
            //ARRANGE
            var document = _dataGenerator.SeedHabitJsonPatchDocument();
            var id = Guid.NewGuid();
            var command = new UpdateCommand<Habit>(id, document);

            _habitsRepositoryMock.Setup(r => r.UpdateAsync(It.Is<JsonPatchDocument<Habit>>(x => x == document), It.Is<Guid>(x => x == id), default))
                                  .ReturnsAsync(true);

            //ACT
            var result = await _habitsHandler.Handle(command, default);

            //ASSERT
            result.Should().BeTrue();

            _habitsRepositoryMock.Verify(r => r.UpdateAsync(It.Is<JsonPatchDocument<Habit>>(x => x == document), It.Is<Guid>(x => x == id), default), Times.Once);
        }

        [Fact]
        public async Task Group_Handle_EntityNotUpdated_ReturnsFalse()
        {
            //ARRANGE
            var document = _dataGenerator.SeedGroupJsonPatchDocument();
            var id = Guid.NewGuid();
            var command = new UpdateCommand<Group>(id, document);

            _groupsRepositoryMock.Setup(r => r.UpdateAsync(It.Is<JsonPatchDocument<Group>>(x => x == document), It.Is<Guid>(x => x == id), default))
                                  .ReturnsAsync(false);

            //ACT
            var result = await _groupsHandler.Handle(command, default);

            //ASSERT
            result.Should().BeFalse();

            _groupsRepositoryMock.Verify(r => r.UpdateAsync(It.Is<JsonPatchDocument<Group>>(x => x == document), It.Is<Guid>(x => x == id), default), Times.Once);
        }

        [Fact]
        public async Task Group_Handle_EntityUpdated_ReturnsTrue()
        {
            //ARRANGE
            var document = _dataGenerator.SeedGroupJsonPatchDocument();
            var id = Guid.NewGuid();
            var command = new UpdateCommand<Group>(id, document);

            _groupsRepositoryMock.Setup(r => r.UpdateAsync(It.Is<JsonPatchDocument<Group>>(x => x == document), It.Is<Guid>(x => x == id), default))
                                  .ReturnsAsync(true);

            //ACT
            var result = await _groupsHandler.Handle(command, default);

            //ASSERT
            result.Should().BeTrue();

            _groupsRepositoryMock.Verify(r => r.UpdateAsync(It.Is<JsonPatchDocument<Group>>(x => x == document), It.Is<Guid>(x => x == id), default), Times.Once);
        }
    }
}
