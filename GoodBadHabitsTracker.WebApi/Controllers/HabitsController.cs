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
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Application.Commands.Habits.DeleteAll;
using GoodBadHabitsTracker.Application.Commands.Habits.DeleteAllProgress;
using GoodBadHabitsTracker.Application.DTOs.Response;
using LanguageExt.Common;
using FluentResults;

namespace GoodBadHabitsTracker.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class HabitsController(IMediator mediator, IEmailSender emailSender) : ControllerBase
    {
        [OutputCache]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new ReadHabitByIdQuery(id), cancellationToken);

            return result.Match<IActionResult>(
                res => Ok(res),
                _ => NotFound());
        }

        [OutputCache]
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new ReadAllHabitsQuery(), cancellationToken);

            return result.Match<IActionResult>(
                res => Ok(res),
                _ => NotFound());
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
                _ => NotFound());
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] HabitRequest request, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new CreateHabitCommand(request), cancellationToken);

            return result.Match<IActionResult>(
                res =>
                {
                    emailSender.SendMessageAfterNewHabitCreateAsync(res.User, res.Habit);
                    return CreatedAtAction(nameof(GetById), new { id = res.Habit.Id! }, res);
                },
                error => BadRequest(error));
        }

        [HttpPatch("{id:guid}")]
        public async Task<IActionResult> Patch(
            [FromRoute] Guid id, 
            [FromBody] JsonPatchDocument request,
            CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new UpdateHabitCommand(id, request), cancellationToken);

            return result.Match<IActionResult>(
                _ => NoContent(),
                error => BadRequest(error));
        }
        [HttpPatch("reset")]
        public async Task<IActionResult> DeleteAllProgress(CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new DeleteAllProgressCommand(), cancellationToken);

            return result.Match<IActionResult>(
                _ => NoContent(),
                error => BadRequest(error));
        }
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new DeleteHabitCommand(id), cancellationToken);

            return result.Match<IActionResult>(
                _ => NoContent(),
                error => BadRequest(error));
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteAll(CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new DeleteAllHabitsCommand(), cancellationToken);

            return result.Match<IActionResult>(
                _ => NoContent(),
                error => BadRequest(error));
        }
    }
}
