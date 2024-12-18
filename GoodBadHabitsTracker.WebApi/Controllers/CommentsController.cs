using GoodBadHabitsTracker.Application.Commands.Comments.Create;
using GoodBadHabitsTracker.Application.Commands.DayResults.Update;
using GoodBadHabitsTracker.Application.DTOs.Request;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoodBadHabitsTracker.WebApi.Controllers
{
    [ApiController]
    [Route("api/habits/{habitId:guid}/[controller]")]
    [Authorize(Policy = "GBHTPolicy")]
    public class CommentsController(IMediator mediator, ILogger<CommentsController> logger) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Post(CreateCommentRequest request, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new CreateCommentCommand(request), cancellationToken);

            return result.Match<IActionResult>(
                _ => NoContent(),
                error => BadRequest(error));
        }
    }
}
