using GoodBadHabitsTracker.Application.Commands.Groups.Create;
using GoodBadHabitsTracker.Application.DTOs.Request;
using GoodBadHabitsTracker.Application.Queries.Groups.ReadById;
using GoodBadHabitsTracker.Application.Queries.Groups.ReadAll;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.JsonPatch;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.Application.Commands.Groups.Update;
using GoodBadHabitsTracker.Application.Commands.Groups.Delete;

namespace GoodBadHabitsTracker.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "GBHTPolicy")]
    public class GroupsController(IMediator mediator) : ControllerBase
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

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] GroupRequest request, CancellationToken cancellationToken)
        {
            var response = await mediator.Send(new CreateCommand(request), cancellationToken);
            return response is not null ? CreatedAtAction(nameof(GetById), new { id = response!.Group.Id! }, response) : BadRequest();
        }

        [HttpPatch("{id:guid}")]
        public async Task<IActionResult> Patch([FromRoute] Guid id, 
            [FromBody] JsonPatchDocument<Group> request, 
            CancellationToken cancellationToken)
        {
            var response = await mediator.Send(new UpdateCommand(id, request), cancellationToken);
            return response ? NoContent() : BadRequest();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var response = await mediator.Send(new DeleteCommand(id), cancellationToken);
            return response ? NoContent() : BadRequest();
        }
    }
}
