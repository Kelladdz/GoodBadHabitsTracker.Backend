﻿using System.Net;
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
using LanguageExt.Common;
using GoodBadHabitsTracker.Application.DTOs.Response;
using GoodBadHabitsTracker.Application.Exceptions;

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
            var result = await mediator.Send(new ReadGroupByIdQuery(id), cancellationToken);

            return result.Match<IActionResult>(
                res => Ok(res),
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

        [OutputCache]
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new ReadAllGroupsQuery(), cancellationToken);

            return result.Match<IActionResult>(
                res => Ok(res),
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

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] GroupRequest request, CancellationToken cancellationToken)
        {
            Result<GroupResponse> result = await mediator.Send(new CreateGroupCommand(request), cancellationToken);

            return result.Match<IActionResult>(
                res => CreatedAtAction(nameof(GetById), new { id = res.Group.Id! }, res),
                error => BadRequest(error));
        }

        [HttpPatch("{id:guid}")]
        public async Task<IActionResult> Patch([FromRoute] Guid id,
            [FromBody] JsonPatchDocument request,
            CancellationToken cancellationToken)
        {
            Result<bool> result = await mediator.Send(new UpdateGroupCommand(id, request), cancellationToken);

            return result.Match<IActionResult>(
                res => NoContent(),
                error => BadRequest(error));
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            Result<bool> result = await mediator.Send(new DeleteGroupCommand(id), cancellationToken);

            return result.Match<IActionResult>(
                res => NoContent(),
                error => BadRequest(error));
        }
    }
}