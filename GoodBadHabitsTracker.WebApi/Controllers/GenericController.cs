using MediatR;
using Microsoft.AspNetCore.Mvc;
using GoodBadHabitsTracker.WebApi.Conventions;
using GoodBadHabitsTracker.Application.Queries.Generic.ReadById;
using Microsoft.AspNetCore.OutputCaching;
using GoodBadHabitsTracker.Application.Queries.Generic.Search;
using GoodBadHabitsTracker.Application.Commands.Generic.Insert;
using Microsoft.AspNetCore.JsonPatch;
using GoodBadHabitsTracker.Application.Commands.Generic.Update;
using GoodBadHabitsTracker.Application.Commands.Generic.Delete;
using GoodBadHabitsTracker.Application.Queries.Generic.GetAll;
using Microsoft.AspNetCore.Authorization;


namespace GoodBadHabitsTracker.WebApi.Controllers
{
    [ApiController]
    [GenericControllerConventions]
    [Route("api/[controller]")]
    [Authorize(Policy = "GBHTPolicy")]

    public class GenericController<TEntity, TRequest, TResponse> : ControllerBase
        where TEntity : class
        where TRequest : class
        where TResponse : class
    {
        protected IMediator _mediator;
        protected CancellationToken _cancellationToken;
        public GenericController(IMediator mediator)
        {
            _mediator = mediator;
            _cancellationToken = CancellationToken.None;
        }
        [OutputCache]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var response = await _mediator.Send(new ReadByIdQuery<TEntity>(id), _cancellationToken);
            return response is null ? NotFound() : Ok(response);
        }

        [OutputCache]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var response = await _mediator.Send(new GetAllQuery<TEntity>(), _cancellationToken);
            return Ok(response);
        }

        /*[HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string? term, [FromQuery] DateOnly date)
        {
            var response = await _mediator.Send(new SearchQuery<TEntity>(term, date), _cancellationToken);
            return response is not null ? Ok(response) : NotFound();
        }*/

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TRequest request)
        {
            var response = await _mediator.Send(new InsertCommand<TEntity, TRequest>(request), _cancellationToken);
            return response is null ? BadRequest() : CreatedAtAction(nameof(GetById), new { id = (Guid)response.Entity.GetType().GetProperty("Id")!.GetValue(response.Entity)! }, response);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> Patch([FromRoute] Guid id, [FromBody] JsonPatchDocument<TEntity> request)
        {
            var response = await _mediator.Send(new UpdateCommand<TEntity>(id, request), _cancellationToken);
            return response ? NoContent() : BadRequest();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var response = await _mediator.Send(new DeleteCommand<TEntity>(id), _cancellationToken);
            return response ? NoContent() : BadRequest();
        }
    }
}
