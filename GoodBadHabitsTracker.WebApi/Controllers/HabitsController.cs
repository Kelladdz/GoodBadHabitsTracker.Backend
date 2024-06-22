using GoodBadHabitsTracker.Application.DTOs.Habit.Request;
using GoodBadHabitsTracker.Application.Commands.Habit.GoodHabit.Create;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;

namespace GoodBadHabitsTracker.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HabitsController(IMediator mediator) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] HabitRequest request, CancellationToken cancellationToken)
        {
            var response = request.IsGood ? await mediator.Send(new Command(request)) : null;
            return Created(new Uri($"/api/habits/{response.GoodHabit}", UriKind.Relative), response.GoodHabit); //TO CHANGE LATER
        }
    }
}
