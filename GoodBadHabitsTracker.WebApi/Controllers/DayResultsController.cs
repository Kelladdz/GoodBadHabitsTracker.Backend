using System.Net;
using GoodBadHabitsTracker.Application.Commands.DayResults.Update;
using GoodBadHabitsTracker.Application.DTOs.Request;
using GoodBadHabitsTracker.Application.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoodBadHabitsTracker.WebApi.Controllers
{
    [ApiController]
    [Route("api/habits/{habitId:guid}/day-results")]
    [Authorize(Policy = "GBHTPolicy")]
    public class DayResultsController(IMediator mediator, ILogger<DayResultsController> logger) : ControllerBase
    {
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Put([FromRoute] Guid habitId, [FromRoute] Guid id, [FromBody] UpdateDayResultRequest request, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new UpdateDayResultCommand(id, request), cancellationToken);

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