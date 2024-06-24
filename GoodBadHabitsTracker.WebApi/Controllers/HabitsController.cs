using GoodBadHabitsTracker.Application.DTOs.Habit.Request;
using GoodBadHabitsTracker.Application.Commands.Habit.GoodHabit.Create;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using GoodBadHabitsTracker.Application.Commands.Habit.LimitHabit.Create;

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
                var response = await mediator.Send(new Application.Commands.Habit.GoodHabit.Create.Command(request), cancellationToken);
                return Created(new Uri($"/api/habits/{response.GoodHabit}", UriKind.Relative), response.GoodHabit); //TO CHANGE LATER
            }
            else if ((bool)!request.IsQuit!)
            {
                var response = await mediator.Send(new Application.Commands.Habit.LimitHabit.Create.Command(request), cancellationToken);
                return Created(new Uri($"/api/habits/{response.LimitHabit}", UriKind.Relative), response.LimitHabit); //TO CHANGE LATER
            }
            else
            {
                var response = await mediator.Send(new Application.Commands.Habit.QuitHabit.Create.Command(request), cancellationToken);
                return Created(new Uri($"/api/habits/{response.QuitHabit}", UriKind.Relative), response.QuitHabit); //TO CHANGE LATER
            }
        }
    }
}
