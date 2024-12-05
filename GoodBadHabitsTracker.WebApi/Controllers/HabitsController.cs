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
using GoodBadHabitsTracker.Application.Commands.Habits.PostInProgressToday;

namespace GoodBadHabitsTracker.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "GBHTPolicy")]
    public class HabitsController(IMediator mediator, IEmailSender emailSender) : ControllerBase
    {
        [OutputCache]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var response = await mediator.Send(new ReadByIdQuery(id), cancellationToken);
            return response is not null ? Ok(response) : NotFound();
        }

        [OutputCache]
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var response = await mediator.Send(new ReadAllQuery(), cancellationToken);
            return Ok(response);
        }

        [OutputCache]
        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] string? term, 
            [FromQuery] DateOnly date, 
            CancellationToken cancellationToken)
        {
            var response = await mediator.Send(new SearchQuery(term, date), cancellationToken);
            return response is not null ? Ok(response) : NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] HabitRequest request, CancellationToken cancellationToken)
        {
            var response = await mediator.Send(new CreateCommand(request), cancellationToken);
            if (response is not null)
            {
                await emailSender.SendMessageAfterNewHabitCreateAsync(response.User, response.Habit);
                return CreatedAtAction(nameof(GetById), new { id = response!.Habit.Id! }, response);
            }
            else return BadRequest();
        }

        [HttpPatch("{id:guid}")]
        public async Task<IActionResult> Patch(
            [FromRoute] Guid id, 
            [FromBody] JsonPatchDocument<Habit> request,
            CancellationToken cancellationToken)
        {
            var response = await mediator.Send(new UpdateCommand(id, request), cancellationToken);
            return response ? NoContent() : BadRequest();
        }
        [HttpPatch("reset")]
        public async Task<IActionResult> DeleteAllProgress(CancellationToken cancellationToken)
        {
            var response = await mediator.Send(new DeleteAllProgressCommand(), cancellationToken);
            if (response)
            {
                return await mediator.Send(new PostInProgressTodayCommand(), cancellationToken)
                    ? NoContent() : BadRequest();
            }
            else
            {
                return BadRequest();
            }
        }
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var response = await mediator.Send(new DeleteCommand(id), cancellationToken);
            return response ? NoContent() : BadRequest();
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteAll(CancellationToken cancellationToken)
        {
            var response = await mediator.Send(new DeleteAllCommand(), cancellationToken);
            return response ? NoContent() : BadRequest();
        }
    }
}
