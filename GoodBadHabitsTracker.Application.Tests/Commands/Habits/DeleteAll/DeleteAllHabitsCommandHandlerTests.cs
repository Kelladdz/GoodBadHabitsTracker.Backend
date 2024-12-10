using FluentAssertions;
using LanguageExt.Common;
using GoodBadHabitsTracker.Application.Commands.Habits.Delete;
using GoodBadHabitsTracker.Application.Commands.Habits.DeleteAll;
using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.TestMisc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static BenchmarkDotNet.Attributes.MarkdownExporterAttribute;

namespace GoodBadHabitsTracker.Application.Tests.Commands.Habits.DeleteAll
{
    public class DeleteAllHabitsCommandHandlerTests
    {
        private readonly Mock<IHabitsRepository> _habitsRepositoryMock;
        private readonly Mock<IUserAccessor> _userAccessorMock;
        private readonly DeleteAllHabitsCommandHandler _handler;

        public DeleteAllHabitsCommandHandlerTests()
        {
            _habitsRepositoryMock = new Mock<IHabitsRepository>();
            _userAccessorMock = new Mock<IUserAccessor>();
            _handler = new DeleteAllHabitsCommandHandler(_habitsRepositoryMock.Object, _userAccessorMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenDeleteIsSuccessful()
        {
            //ARRANGE
            var user = DataGenerator.SeedUser();
            var userId = user.Id;
            var command = new DeleteAllHabitsCommand();

            _userAccessorMock.Setup(x => x.GetCurrentUser()).ReturnsAsync(user);
            _habitsRepositoryMock
                .Setup(x => x.ReadAllAsync(userId, default))
                .ReturnsAsync(It.IsAny<IEnumerable<Habit>>());
            _habitsRepositoryMock
                .Setup(x => x.DeleteAllAsync(It.IsAny<IEnumerable<Habit>>(), default))
                .Returns(Task.CompletedTask);

            //ACT
            var result = await _handler.Handle(command, default);

            //ASSERT
            result.IsSuccess.Should().BeTrue();

            _userAccessorMock.Verify(x => x.GetCurrentUser(), Times.Once);
            _habitsRepositoryMock.Verify(x => x.ReadAllAsync(userId, default), Times.Once);
            _habitsRepositoryMock.Verify(x => x.DeleteAllAsync(It.IsAny<IEnumerable<Habit>>(), default), Times.Once);
        }
        [Fact]
        public async Task Handle_ShouldReturnFail_WhenUserDoesntExist()
        {
            //ARRANGE
            var command = new DeleteAllHabitsCommand();

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
