using System.Net;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using GoodBadHabitsTracker.Application.Commands.Habits.Create;
using GoodBadHabitsTracker.Application.Queries.Habits.Search;
using GoodBadHabitsTracker.Application.Queries.Habits.ReadAll;
using GoodBadHabitsTracker.Application.Queries.Habits.ReadById;
using GoodBadHabitsTracker.Application.Commands.Habits.Update;
using GoodBadHabitsTracker.Application.Commands.Habits.Delete;
using GoodBadHabitsTracker.Application.DTOs.Request;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Application.Commands.Habits.DeleteAll;
using GoodBadHabitsTracker.Application.Commands.Habits.DeleteAllProgress;
using GoodBadHabitsTracker.Application.Exceptions;
using LanguageExt.Common;

namespace GoodBadHabitsTracker.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "GBHTPolicy")]
    public class HabitsController(IMediator mediator, IEmailSender emailSender, ILogger<HabitsController> logger) : ControllerBase
    {
        [OutputCache]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new ReadHabitByIdQuery(id), cancellationToken);

            return result.Match<IActionResult>(
                res => Ok(res),
                error =>
                {
                    var code = (error as AppException)!.Code;
                    switch (code)
                    {
                        case HttpStatusCode.Unauthorized:
                            return Unauthorized(error);
                        case HttpStatusCode.BadRequest:
                            return BadRequest(error);
                        case HttpStatusCode.NotFound:
                            return NotFound(error);
                        default:
                            return Problem();
                    }
                });
        }

        [OutputCache]
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new ReadAllHabitsQuery(), cancellationToken);

            return result.Match<IActionResult>(
                res => Ok(res),
                error =>
                {
                    var code = (error as AppException)!.Code;
                    switch (code)
                    {
                        case HttpStatusCode.Unauthorized:
                            return Unauthorized(error);
                        case HttpStatusCode.BadRequest:
                            return BadRequest(error);
                        case HttpStatusCode.NotFound:
                            return NotFound(error);
                        default:
                            return Problem();
                    }
                });
        }

        [OutputCache]
        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] string? term,
            [FromQuery] DateOnly date,
            CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new SearchHabitsQuery(term, date), cancellationToken);

            return result.Match<IActionResult>(
                res => Ok(res),
                error =>
                {
                    var code = (error as AppException)!.Code;
                    switch (code)
                    {
                        case HttpStatusCode.Unauthorized:
                            return Unauthorized(error);
                        case HttpStatusCode.BadRequest:
                            return BadRequest(error);
                        case HttpStatusCode.NotFound:
                            return NotFound(error);
                        default:
                            return Problem();
                    }
                });
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] HabitRequest request, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new CreateHabitCommand(request), cancellationToken);

            return result.Match<IActionResult>(
                res =>
                {
                    var user = res.User;
                    var habit = res.Habit;
                    var habitId = habit.Id;

                    emailSender.SendMessageAfterNewHabitCreateAsync(user, habit);

                    return CreatedAtAction(nameof(GetById), new { id = habitId }, habit);
                },
                error =>
                {
                    var code = (error as AppException)!.Code;
                    switch (code)
                    {
                        case HttpStatusCode.Unauthorized:
                            return Unauthorized(error);
                        case HttpStatusCode.BadRequest:
                            return BadRequest(error);
                        case HttpStatusCode.NotFound:
                            return NotFound(error);
                        default:
                            return Problem();
                    }
                });
        }

        [HttpPatch("{id:guid}")]
        public async Task<IActionResult> Patch(
            [FromRoute] Guid id,
            [FromBody] JsonPatchDocument request,
            CancellationToken cancellationToken)
        {
            logger.LogDebug("Update habit with id: {id}...", id);
            var result = await mediator.Send(new UpdateHabitCommand(id, request), cancellationToken);

            return result.Match<IActionResult>(
                _ =>
                {
                    logger.LogDebug("Habit updated.");
                    return NoContent();
                },
                error =>
                {
                    logger.LogDebug("Habit wasn't updated. {error}", error);
                    return BadRequest(error);
                });
        }
        [HttpPatch("reset")]
        public async Task<IActionResult> DeleteAllProgress(CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new DeleteAllProgressCommand(), cancellationToken);

            return result.Match<IActionResult>(
                _ => NoContent(),
                error =>
                {
                    var code = (error as AppException)!.Code;
                    switch (code)
                    {
                        case HttpStatusCode.Unauthorized:
                            return Unauthorized(error);
                        case HttpStatusCode.BadRequest:
                            return BadRequest(error);
                        case HttpStatusCode.NotFound:
                            return NotFound(error);
                        default:
                            return Problem();
                    }
                });
        }
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new DeleteHabitCommand(id), cancellationToken);

            return result.Match<IActionResult>(
                _ => NoContent(),
                error =>
                {
                    var code = (error as AppException)!.Code;
                    switch (code)
                    {
                        case HttpStatusCode.Unauthorized:
                            return Unauthorized(error);
                        case HttpStatusCode.BadRequest:
                            return BadRequest(error);
                        case HttpStatusCode.NotFound:
                            return NotFound(error);
                        default:
                            return Problem();
                    }
                });
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteAll(CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new DeleteAllHabitsCommand(), cancellationToken);

            return result.Match<IActionResult>(
                _ => NoContent(),
                error =>
                {
                    var code = (error as AppException)!.Code;
                    switch (code)
                    {
                        case HttpStatusCode.Unauthorized:
                            return Unauthorized(error);
                        case HttpStatusCode.BadRequest:
                            return BadRequest(error);
                        case HttpStatusCode.NotFound:
                            return NotFound(error);
                        default:
                            return Problem();
                    }
                });
        }
    }
}