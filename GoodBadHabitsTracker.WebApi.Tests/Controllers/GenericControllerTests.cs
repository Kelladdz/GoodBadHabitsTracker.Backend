using MediatR;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;
using GoodBadHabitsTracker.TestMisc;
using GoodBadHabitsTracker.WebApi.Controllers;
using GoodBadHabitsTracker.Application.Queries.Generic.ReadById;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.Application.DTOs.Habit.Request;
using GoodBadHabitsTracker.Application.DTOs.Generic.Response;
using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Application.Queries.Generic.Search;
using GoodBadHabitsTracker.Application.Commands.Generic.Insert;
using GoodBadHabitsTracker.Core.Enums;
using GoodBadHabitsTracker.Application.Commands.Generic.Update;

namespace GoodBadHabitsTracker.WebApi.Tests.Controllers
{
    public class GenericControllerTests
    { 
        private readonly DataGenerator _dataGenerator;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly GenericController<Habit, HabitRequest, GenericResponse<Habit>> _habitsController;

        public GenericControllerTests()
        {
            _dataGenerator = new DataGenerator();
            _mediatorMock = new Mock<IMediator>();
            _habitsController = new GenericController<Habit, HabitRequest, GenericResponse<Habit>>(_mediatorMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        [Fact]
        public async Task Habit_GetById_ResponseIsNotNull_ReturnsOkWithHabitResponse()
        {
            //ARRANGE
            var response = _dataGenerator.SeedHabitResponse();

            _mediatorMock.Setup(x => x.Send(It.IsAny<ReadByIdQuery<Habit>>(), default))
                .ReturnsAsync(response);

            //ACT
            var result = await _habitsController.GetById(It.IsAny<Guid>()) as OkObjectResult;
            var value = result!.Value;

            //ASSERT
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
            value.Should().BeOfType<GenericResponse<Habit>>();

            _mediatorMock.Verify(x => x.Send(It.IsAny<ReadByIdQuery<Habit>>(), default), Times.Once);
        }

        [Fact]
        public async Task Habit_GetById_ResponseIsNull_ReturnsNotFound()
        {
            //ARRANGE
            var id = Guid.NewGuid();
            var response = null as GenericResponse<Habit>;

            _mediatorMock.Setup(x => x.Send(It.Is<ReadByIdQuery<Habit>>(a => a.Id == id), default))
                .ReturnsAsync(response);

            //ACT
            var result = await _habitsController.GetById(id) as NotFoundResult;

            //ASSERT
            result?.StatusCode.Should().Be(StatusCodes.Status404NotFound);

            _mediatorMock.Verify(x => x.Send(It.Is<ReadByIdQuery<Habit>>(a => a.Id == id), default), Times.Once);
        }

        [Fact]
        public async Task Habit_Search_ValidRequest_ResponseIsNotNull_ReturnsOkWithHabitsResponseCollection()
        {
            //ARRANGE
            var term = _dataGenerator.SeedRandomString(16);
            var date = DateOnly.FromDateTime(DateTime.Now);
            var response = _dataGenerator.SeedHabitResponseCollection();

            _mediatorMock.Setup(x => x.Send(It.Is<SearchQuery<Habit>>(a => a.Term == term && a.Date == date), default)).ReturnsAsync(response);

            //ACT
            var result = await _habitsController.Search(term, date) as OkObjectResult;
            var value = result!.Value;

            // ASSERT
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
            value.Should().BeEquivalentTo(response);

            _mediatorMock.Verify(x => x.Send(It.Is<SearchQuery<Habit>>(a => a.Term == term && a.Date == date), default), Times.Once);
        }

        [Fact]
        public async Task Habit_Search_ValidRequest_ResponseIsNull_ReturnsNotFoundWithEmptyArray()
        {
            //ARRANGE
            var term = _dataGenerator.SeedRandomString(16);
            var date = DateOnly.FromDateTime(DateTime.Now);
            var response = new List<GenericResponse<Habit>>();

            _mediatorMock.Setup(x => x.Send(It.Is<SearchQuery<Habit>>(a => a.Term == term && a.Date == date), default)).ReturnsAsync(response);

            //ACT
            var result = await _habitsController.Search(term, date) as NotFoundObjectResult;
            var value = result!.Value;

            // ASSERT
            result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            value.Should().BeEquivalentTo(response);
        }

        [Fact]
        public async Task Habit_Search_InvalidDate_ThrowsExceptionAndReturnsBadRequest()
        {
            //ARRANGE
            var term = null as string;
            var date = DateOnly.FromDateTime(DateTime.Now).AddDays(8);
            var exception = new ValidationException(new List<ValidationError> { new ValidationError("Date", "The date must be within the next 7 days.") });

            _mediatorMock.Setup(x => x.Send(It.Is<SearchQuery<Habit>>(a => a.Term == term && a.Date == date), default)).ThrowsAsync(exception);

            //ACT
            Func<Task> action = async () => await _habitsController.Search(term, date);

            // ASSERT
            await action.Should().ThrowAsync<ValidationException>().Where(x => x.Errors.FirstOrDefault()!.ErrorMessage == "The date must be within the next 7 days.");
       
            _mediatorMock.Verify(x => x.Send(It.Is<SearchQuery<Habit>>(a => a.Term == term && a.Date == date), default), Times.Once);
        }

        [Fact]
        public async Task Habit_Post_ValidRequest_ReturnsCreatedActionWithActionNameRouteValuesAndHabitResponse()
        {
            //ARRANGE
            var request = _dataGenerator.SeedHabitRequest();
            var response = _dataGenerator.SeedHabitResponse();

            _mediatorMock.Setup(x => x.Send(It.Is<InsertCommand<Habit, HabitRequest>>(a => a.Request == request), default))
                .ReturnsAsync(response);

            //ACT
            var result = await _habitsController.Post(request) as CreatedAtActionResult;
            var actionName = result!.ActionName;
            var routeValues = result!.RouteValues;
            var value = result.Value;

            _mediatorMock.Verify(x => x.Send(It.Is<InsertCommand<Habit, HabitRequest>>(a => a.Request == request), default), Times.Once);

            //ASSERT
            result.StatusCode.Should().Be(StatusCodes.Status201Created);
            actionName.Should().Be("GetById");
            routeValues!.Keys.Should().HaveCount(1).And.Contain("id");
            routeValues["id"].Should().Be(response.Entity.Id);
            value.Should().BeAssignableTo<GenericResponse<Habit>?>();
        }

        [Fact]
        public async Task Habit_Post_ResponseIsNull_ReturnsBadRequest()
        {
            //ARRANGE
            var request = _dataGenerator.SeedHabitRequest();
            var response = (GenericResponse<Habit>?)null;

            _mediatorMock.Setup(x => x.Send(It.Is<InsertCommand<Habit, HabitRequest>>(a => a.Request == request), default))
                .ReturnsAsync(response);

            //ACT
            var result = await _habitsController.Post(request) as BadRequestResult;

            //ASSERT
            result!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

            _mediatorMock.Verify(x => x.Send(It.Is<InsertCommand<Habit, HabitRequest>>(a => a.Request == request), default), Times.Once);
        }

        [Fact]
        public async Task Habit_Post_InvalidRequest_ThrowsValidationException()
        {
            //ARRANGE
            var request = new HabitRequest
            {
                Name = _dataGenerator.SeedRandomString(16),
                HabitType = HabitTypes.Good,
                IconPath = "",
                StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-5)),
                ReminderTimes = new TimeOnly[1],
                IsTimeBased = false,
                Quantity = 5,
            };
            var exception = new ValidationException(new List<ValidationError> { new ValidationError("StartDate", "Start date must be today or later.") });

            _mediatorMock.Setup(x => x.Send(It.IsAny<InsertCommand<Habit, HabitRequest>>(), default)).ThrowsAsync(exception);

            //ACT
            var action = async () => await _habitsController.Post(request);

            //ASSERT
            await action.Should().ThrowAsync<ValidationException>()
                .Where(x=> x.Errors.FirstOrDefault()!.ErrorMessage == "Start date must be today or later.");

            _mediatorMock.Verify(x => x.Send(It.IsAny<InsertCommand<Habit, HabitRequest>>(), default), Times.Once);
        }

        [Fact]
        public async Task Habit_Patch_ValidRequest_ResponseIsTrue_ReturnsNoContent()
        {
            //ARRANGE
            var id = Guid.NewGuid();
            var document = _dataGenerator.SeedHabitJsonPatchDocument();

            _mediatorMock.Setup(x => x.Send(It.Is<UpdateCommand<Habit>>(a => a.Id == id && a.Request == document), default))
                .ReturnsAsync(true);

            //ACT
            var result = await _habitsController.Patch(id, document) as NoContentResult;

            //ASSERT
            result!.StatusCode.Should().Be(StatusCodes.Status204NoContent);

            _mediatorMock.Verify(x => x.Send(It.Is<UpdateCommand<Habit>>(a => a.Id == id && a.Request == document), default), Times.Once);
        }

        [Fact]
        public async Task Habit_Patch_ValidRequest_ResponseIsFalse_ReturnsBadRequest()
        {
            //ARRANGE
            var id = Guid.NewGuid();
            var document = _dataGenerator.SeedHabitJsonPatchDocument();

            _mediatorMock.Setup(x => x.Send(It.Is<UpdateCommand<Habit>>(a => a.Id == id && a.Request == document), default))
                .ReturnsAsync(false);

            //ACT
            var result = await _habitsController.Patch(id, document) as BadRequestResult;

            //ASSERT
            result!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

            _mediatorMock.Verify(x => x.Send(It.Is<UpdateCommand<Habit>>(a => a.Id == id && a.Request == document), default), Times.Once);
        }

        [Fact]
        public async Task Habit_Patch_JsonPatchDocumentIsNull_ThrowsValidationException()
        {
            //ARRANGE
            var id = Guid.NewGuid();
            var document = (JsonPatchDocument<Habit>?)null;
            var exception = new ValidationException(new List<ValidationError> { new ValidationError("Operation", "Operation is required") });

            _mediatorMock.Setup(x => x.Send(It.Is<UpdateCommand<Habit>>(a => a.Id == id && a.Request == document), default))
                .ThrowsAsync(exception);

            //ACT
            Func<Task> action = async () => await _habitsController.Patch(id, document);


            // ASSERT
            await action.Should().ThrowAsync<ValidationException>()
                .Where(x => x.Errors.FirstOrDefault()!.ErrorMessage == "Operation is required");
        }
    }
}