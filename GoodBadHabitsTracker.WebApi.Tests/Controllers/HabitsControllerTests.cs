using FluentAssertions;
using GoodBadHabitsTracker.Application.Commands.Habits.Create;
using GoodBadHabitsTracker.Application.DTOs.Request;
using GoodBadHabitsTracker.Application.DTOs.Response;
using GoodBadHabitsTracker.Application.Queries.Habits.ReadAll;
using GoodBadHabitsTracker.Application.Queries.Habits.ReadById;
using GoodBadHabitsTracker.Application.Queries.Habits.Search;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.TestMisc;
using GoodBadHabitsTracker.WebApi.Controllers;
using LanguageExt.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Microsoft.AspNetCore.JsonPatch;
using GoodBadHabitsTracker.Application.Commands.Habits.Update;
using GoodBadHabitsTracker.Application.Commands.Habits.DeleteAllProgress;
using GoodBadHabitsTracker.Application.Commands.Habits.Delete;
using GoodBadHabitsTracker.Application.Commands.Habits.DeleteAll;
using GoodBadHabitsTracker.Core.Models;

namespace GoodBadHabitsTracker.WebApi.Tests.Controllers
{
    public class HabitsControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<IEmailSender> _emailSenderMock;
        private readonly HabitsController _controller;

        public HabitsControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _emailSenderMock = new Mock<IEmailSender>();
            _controller = new HabitsController(_mediatorMock.Object, _emailSenderMock.Object);
        }

        [Fact]
        public async Task GetById_ShouldReturnOk_WhenHabitExists()
        {
            // Arrange
            var habitId = Guid.NewGuid();
            var response = new HabitResponse(DataGenerator.SeedHabit());
            _mediatorMock.Setup(m => m.Send(It.IsAny<ReadHabitByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetById(habitId, CancellationToken.None);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenHabitDoesNotExist()
        {
            // Arrange
            var habitId = Guid.NewGuid();
            _mediatorMock.Setup(m => m.Send(It.IsAny<ReadHabitByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Result<HabitResponse>(new Exception("Habit not found")));

            // Act
            var result = await _controller.GetById(habitId, CancellationToken.None);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetAll_ShouldReturnOk_WithListOfHabits()
        {
            // Arrange
            var response = new List<HabitResponse> { new HabitResponse(DataGenerator.SeedHabit()) };
            _mediatorMock.Setup(m => m.Send(It.IsAny<ReadAllHabitsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetAll(CancellationToken.None);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Search_ShouldReturnOk_WithListOfHabits()
        {
            // Arrange
            var response = new List<HabitResponse> { new HabitResponse(DataGenerator.SeedHabit()) };
            _mediatorMock.Setup(m => m.Send(It.IsAny<SearchHabitsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Search("test", DateOnly.FromDateTime(DateTime.Now), CancellationToken.None);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Post_ShouldReturnCreatedAtAction_WhenSuccessful()
        {
            // Arrange
            var request = new HabitRequest();
            var habit = DataGenerator.SeedHabit();
            var user = DataGenerator.SeedUser();
            var response = new CreateHabitResponse(habit, user);

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateHabitCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);
            _emailSenderMock.Setup(es => es.SendMessageAfterNewHabitCreate(user, habit));

            // Act
            var result = await _controller.Post(request, CancellationToken.None);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
        }

        [Fact]
        public async Task Post_ShouldReturnBadRequest_WhenFailed()
        {
            // Arrange
            var request = new HabitRequest();
            Habit? habit = null;
            var user = DataGenerator.SeedUser();
            var response = new Result<CreateHabitResponse>(new Exception("Error"));
            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateHabitCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Post(request, CancellationToken.None);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Patch_ShouldReturnNoContent_WhenSuccessful()
        {
            // Arrange
            var habitId = Guid.NewGuid();
            var patchDoc = new JsonPatchDocument();
            _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateHabitCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Result<bool>(true));

            // Act
            var result = await _controller.Patch(habitId, patchDoc, CancellationToken.None);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task Patch_ShouldReturnBadRequest_WhenFailed()
        {
            // Arrange
            var habitId = Guid.NewGuid();
            var patchDoc = new JsonPatchDocument();
            _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateHabitCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Result<bool>(new Exception("Update failed")));

            // Act
            var result = await _controller.Patch(habitId, patchDoc, CancellationToken.None);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task DeleteAllProgress_ShouldReturnNoContent_WhenSuccessful()
        {
            // Arrange
            _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteAllProgressCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Result<bool>(true));

            // Act
            var result = await _controller.DeleteAllProgress(CancellationToken.None);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task DeleteAllProgress_ShouldReturnBadRequest_WhenFailed()
        {
            // Arrange
            _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteAllProgressCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Result<bool>(new Exception("Reset failed")));

            // Act
            var result = await _controller.DeleteAllProgress(CancellationToken.None);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Delete_ShouldReturnNoContent_WhenSuccessful()
        {
            // Arrange
            var habitId = Guid.NewGuid();
            _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteHabitCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Result<bool>(true));

            // Act
            var result = await _controller.Delete(habitId, CancellationToken.None);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task Delete_ShouldReturnBadRequest_WhenFailed()
        {
            // Arrange
            var habitId = Guid.NewGuid();
            _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteHabitCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Result<bool>(new Exception("Deletion failed")));

            // Act
            var result = await _controller.Delete(habitId, CancellationToken.None);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task DeleteAll_ShouldReturnNoContent_WhenSuccessful()
        {
            // Arrange
            _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteAllHabitsCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Result<bool>(true));

            // Act
            var result = await _controller.DeleteAll(CancellationToken.None);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task DeleteAll_ShouldReturnBadRequest_WhenFailed()
        {
            // Arrange
            _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteAllHabitsCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Result<bool>(new Exception("Deletion failed")));

            // Act
            var result = await _controller.DeleteAll(CancellationToken.None);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}
