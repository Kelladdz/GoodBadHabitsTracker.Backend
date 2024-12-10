using AutoMapper;
using FluentAssertions;
using LanguageExt.Common;
using GoodBadHabitsTracker.Application.Commands.Habits.Create;
using GoodBadHabitsTracker.Application.DTOs.Request;
using GoodBadHabitsTracker.Application.DTOs.Response;
using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Application.Mappings;
using GoodBadHabitsTracker.Core.Enums;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.TestMisc;
using Moq;
using System.Net;

namespace GoodBadHabitsTracker.Application.Tests.Commands.Habits.Create
{
    public class CreateHabitCommandHandlerTests
    {
        private readonly Mock<IHabitsRepository> _habitsRepositoryMock;
        private readonly Mock<IUserAccessor> _userAccessorMock;
        private readonly IMapper _mapper;
        private readonly CreateHabitCommandHandler _handler;

        public CreateHabitCommandHandlerTests()
        {
            _habitsRepositoryMock = new Mock<IHabitsRepository>();
            _userAccessorMock = new Mock<IUserAccessor>();

            var myProfile = new HabitsMappingProfile();
            var configuration = new MapperConfiguration(cfg => cfg.AddProfile(myProfile));
            _mapper = new Mapper(configuration);
            _handler = new CreateHabitCommandHandler(_habitsRepositoryMock.Object, _mapper, _userAccessorMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenHabitIsCreated()
        {
            // ARRANGE
            var userId = Guid.NewGuid();
            var user = new User { Id = userId };
            var request = DataGenerator.SeedHabitRequest();
            var command = new CreateHabitCommand(request);
            var habitToInsert = _mapper.Map<Habit>(request);
            var expectedResponse = new Result<CreateHabitResponse>(new CreateHabitResponse(habitToInsert, user));

            _userAccessorMock.Setup(x => x.GetCurrentUser()).ReturnsAsync(user);
            _habitsRepositoryMock.Setup(x => x.InsertAsync(It.IsAny<Habit>(), default)).Returns(Task.CompletedTask);

            // ACT
            var result = await _handler.Handle(command, default);

            // ASSERT
            result.IsSuccess.Should().BeTrue();

            _userAccessorMock.Verify(x => x.GetCurrentUser(), Times.Once);
            _habitsRepositoryMock.Verify(x => x.InsertAsync(It.IsAny<Habit>(), default), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenUserIsNull()
        {
            // ARRANGE
            var request = DataGenerator.SeedHabitRequest();
            var command = new CreateHabitCommand(request);
            var expectedResult = new Result<CreateHabitResponse>(new AppException(HttpStatusCode.BadRequest, "User Not Found"));

            _userAccessorMock.Setup(x => x.GetCurrentUser()).ReturnsAsync((User?)null);

            // ACT
            var result = await _handler.Handle(command, default);

            // ASSERT
            result.IsFaulted.Should().BeTrue();
            result.Should().BeEquivalentTo(expectedResult);

            _userAccessorMock.Verify(x => x.GetCurrentUser(), Times.Once);
            _habitsRepositoryMock.Verify(x => x.InsertAsync(It.IsAny<Habit>(), default), Times.Never);
        }
    }
}
