using FluentAssertions;
using GoodBadHabitsTracker.Application.Queries.Groups.ReadAll;
using GoodBadHabitsTracker.Application.Queries.Habits.ReadAll;
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
    public class ReadAllHabitsQueryHandlerTests
    {
        private readonly Mock<IHabitsRepository> _habitsRepositoryMock;
        private readonly Mock<IUserAccessor> _userAccessorMock;
        private readonly ReadAllHabitsQueryHandler _handler;

        public ReadAllHabitsQueryHandlerTests()
        {
            _habitsRepositoryMock = new Mock<IHabitsRepository>();
            _userAccessorMock = new Mock<IUserAccessor>();
            _handler = new ReadAllHabitsQueryHandler(_habitsRepositoryMock.Object, _userAccessorMock.Object);
        }

        [Fact]
        public async Task Handle_UserIsNull_ReturnsUnauthorized()
        {
            // Arrange
            var query = new ReadAllHabitsQuery();

            _userAccessorMock.Setup(x => x.GetCurrentUser()).ReturnsAsync((User?)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsFaulted.Should().BeTrue();/*
            result.IfFail(ex => ex.Should().BeOfType<AppException>()
                .Which.StatusCode.Should().Be(HttpStatusCode.Unauthorized));*/
        }

        [Fact]
        public async Task Handle_HabitsAreNull_ReturnsEmptyList()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid() };
            var query = new ReadAllHabitsQuery();

            _userAccessorMock.Setup(x => x.GetCurrentUser()).ReturnsAsync(user);
            _habitsRepositoryMock.Setup(x => x.ReadAllAsync(user.Id, It.IsAny<CancellationToken>()))!.ReturnsAsync((IEnumerable<Habit>?)null);



            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.IfSucc(groups => groups.Should().BeEmpty());
        }

        [Fact]
        public async Task Handle_HabitsExist_ReturnsHabitResponses()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid() };
            var query = new ReadAllHabitsQuery();
            var habits = DataGenerator.SeedHabitsCollection(10);

            _userAccessorMock.Setup(x => x.GetCurrentUser()).ReturnsAsync(user);
            _habitsRepositoryMock.Setup(x => x.ReadAllAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(habits);



            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.IfSucc(groupResponses =>
            {
                groupResponses.Should().HaveCount(10);
            });
        }
    }
}
