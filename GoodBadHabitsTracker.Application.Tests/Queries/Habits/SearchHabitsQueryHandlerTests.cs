using Xunit;
using FluentAssertions;
using Moq;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Application.DTOs.Response;
using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Application.Queries.Habits.Search;
using LanguageExt.Common;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.TestMisc;

namespace GoodBadHabitsTracker.Application.Tests.Queries.Habits
{
    public class SearchHabitsQueryHandlerTests
    {
        private readonly Mock<IHabitsRepository> _habitsRepositoryMock;
        private readonly Mock<IUserAccessor> _userAccessorMock;
        private readonly SearchHabitsQueryHandler _handler;

        public SearchHabitsQueryHandlerTests()
        {
            _habitsRepositoryMock = new Mock<IHabitsRepository>();
            _userAccessorMock = new Mock<IUserAccessor>();
            _handler = new SearchHabitsQueryHandler(_habitsRepositoryMock.Object, _userAccessorMock.Object);
        }

        [Fact]
        public async Task Handle_UserIsNull_ReturnsUnauthorized()
        {
            // Arrange
            _userAccessorMock.Setup(x => x.GetCurrentUser()).ReturnsAsync((User?)null);

            var term = "test";
            var date = DateOnly.FromDateTime(DateTime.Now);
            var query = new SearchHabitsQuery(term, date);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsFaulted.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_HabitsFound_ReturnsHabitResponses()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid() };
            var term = "test";
            var date = DateOnly.FromDateTime(DateTime.Now);
            var query = new SearchHabitsQuery(term, date);
            var habits = DataGenerator.SeedHabitsCollection(10);

            _userAccessorMock.Setup(x => x.GetCurrentUser()).ReturnsAsync(user);
            _habitsRepositoryMock.Setup(x => x.SearchAsync(It.IsAny<string>(), It.IsAny<DateOnly>(), user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(habits);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.IfSucc(habitResponses =>
            {
                habitResponses.Should().HaveCount(10);
            });
        }

        [Fact]
        public async Task Handle_NoHabitsFound_ReturnsEmptyList()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid() };
            var term = "test";
            var date = DateOnly.FromDateTime(DateTime.Now);
            var query = new SearchHabitsQuery(term, date);

            _userAccessorMock.Setup(x => x.GetCurrentUser()).ReturnsAsync(user);
            _habitsRepositoryMock.Setup(x => x.SearchAsync(It.IsAny<string>(), It.IsAny<DateOnly>(), user.Id, It.IsAny<CancellationToken>())).ReturnsAsync([]);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.IfSucc(habitResponses => habitResponses.Should().BeEmpty());
        }
    }
}


