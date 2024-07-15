using GoodBadHabitsTracker.Application.DTOs.Habit.Request;
using GoodBadHabitsTracker.Application.Commands.Habit.GoodHabit.Create;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using GoodBadHabitsTracker.Application.Commands.Habit.LimitHabit.Create;

using FluentValidation;
using GoodBadHabitsTracker.Application.Commands.Habit.QuitHabit.Create;
using System.Reflection.Metadata.Ecma335;

namespace GoodBadHabitsTracker.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HabitsController(IMediator mediator) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] HabitRequest request, CancellationToken cancellationToken)
        {
            if (request.IsGood)
            {
                var command = new CreateGoodHabitCommand(request);

                var response = await mediator.Send(command, cancellationToken);
                return Created(new Uri($"/api/habits/{response.GoodHabit.Id}", UriKind.Relative), response.GoodHabit); //TO CHANGE LATER
            }
            else if ((bool)!request.IsQuit!)
            {
                var command = new CreateLimitHabitCommand(request);

                var response = await mediator.Send(command, cancellationToken);
                return Created(new Uri($"/api/habits/{response.LimitHabit.Id}", UriKind.Relative), response.LimitHabit); //TO CHANGE LATER
            }
            else
            {
                var command = new CreateQuitHabitCommand(request);

                var response = await mediator.Send(command, cancellationToken);
                return Created(new Uri($"/api/habits/{response.QuitHabit.Id}", UriKind.Relative), response.QuitHabit); //TO CHANGE LATER
            }
        }
    }
}
