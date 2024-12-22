using GoodBadHabitsTracker.Application.Commands.Comments.Create;
using GoodBadHabitsTracker.Application.Queries.Comments.ReadById;
using GoodBadHabitsTracker.Application.Commands.DayResults.Update;
using GoodBadHabitsTracker.Application.DTOs.Request;
using GoodBadHabitsTracker.Application.DTOs.Response;
using GoodBadHabitsTracker.Core.Models;
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
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid habitId, [FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new ReadCommentByIdQuery(id), cancellationToken);
            return result.Match<IActionResult>(
                res => Ok(res),
                _ => NotFound());
        }
        [HttpPost]
        public async Task<IActionResult> Post([FromRoute] Guid habitId, [FromBody] CreateCommentRequest request, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new CreateCommentCommand(habitId, request), cancellationToken);
            return result.Match<IActionResult>(
                res => CreatedAtAction(nameof(GetById), new { habitId = habitId, id = res.Comment.Id }, res.Comment),
            error => BadRequest(error));
        }
    }
}