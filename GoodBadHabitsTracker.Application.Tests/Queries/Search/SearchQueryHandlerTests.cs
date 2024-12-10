/*using Moq;
using FluentAssertions;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.Application.Queries.Generic.Search;
using GoodBadHabitsTracker.TestMisc;
using GoodBadHabitsTracker.Application.DTOs.Generic.Response;


namespace GoodBadHabitsTracker.Application.Tests.Queries.Search
{
    public class SearchQueryHandlerTests
    {
        private readonly Mock<IGenericRepository<Habit>> _habitsRepositoryMock;
        private readonly Mock<IGenericRepository<Group>> _groupsRepositoryMock;
        private readonly SearchQueryHandler<Habit> _habitsHandler;
        private readonly SearchQueryHandler<Group> _groupsHandler;
        private readonly DataGenerator _dataGenerator;

        public SearchQueryHandlerTests()
        {
            _habitsRepositoryMock = new();
            _groupsRepositoryMock = new();
            _habitsHandler = new(_habitsRepositoryMock.Object);
            _groupsHandler = new(_groupsRepositoryMock.Object);
            _dataGenerator = new();
        }

        [Fact]
        public async Task Habit_Handle_HabitsFound_ReturnsGenericResponsesCollection()
        {
            //ARRANGE
            var term = _dataGenerator.SeedRandomString(10);
            var date = DateOnly.FromDateTime(DateTime.UtcNow);
            var command = new SearchQuery<Habit>(term, date);
            var habits = _dataGenerator.SeedHabitsCollection(5);

            _habitsRepositoryMock.Setup(r => r.SearchAsync(It.Is<string>(x => x == term), It.Is<DateOnly>(x => x == date), It.IsAny<Guid>(), default))
                                  .ReturnsAsync(habits);

            //ACT
            var result = await _habitsHandler.Handle(command, default) as IEnumerable<GenericResponse<Habit>>;

            //ASSERT
            result.Count().Should().Be(habits.Count());

            _habitsRepositoryMock.Verify(r => r.SearchAsync(It.Is<string>(x => x == term), It.Is<DateOnly>(x => x == date), It.IsAny<Guid>(), default), Times.Once);
        }

        [Fact]
        public async Task Habit_Handle_HabitsNotFound_ReturnsEmptyArray()
        {
            //ARRANGE
            var term = _dataGenerator.SeedRandomString(10);
            var date = DateOnly.FromDateTime(DateTime.UtcNow);
            var command = new SearchQuery<Habit>(term, date);

            _habitsRepositoryMock.Setup(r => r.SearchAsync(It.Is<string>(x => x == term), It.Is<DateOnly>(x => x == date), It.IsAny<Guid>(), default))
                                  .ReturnsAsync([]);

            //ACT
            var result = await _habitsHandler.Handle(command, default);

            //ASSERT
            result.Should().BeEmpty();

            _habitsRepositoryMock.Verify(r => r.SearchAsync(It.Is<string>(x => x == term), It.Is<DateOnly>(x => x == date), It.IsAny<Guid>(), default), Times.Once);
        }
    }
}
*/