using Bogus;
using FluentAssertions;
using GoodBadHabitsTracker.Application.Commands.Groups.Create;
using GoodBadHabitsTracker.Application.DTOs.Request;
using GoodBadHabitsTracker.Application.DTOs.Response;
using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Application.Queries.Groups.ReadAll;
using GoodBadHabitsTracker.Application.Queries.Groups.ReadById;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.TestMisc;
using GoodBadHabitsTracker.WebApi.Controllers;
using LanguageExt.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.JsonPatch;
using GoodBadHabitsTracker.Application.Commands.Groups.Update;
using GoodBadHabitsTracker.Application.Commands.Groups.Delete;

namespace GoodBadHabitsTracker.WebApi.Tests.Controllers
{
    public class GroupsControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly GroupsController _controller;

        public GroupsControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new GroupsController(_mediatorMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        [Fact]
        public async Task GetById_ShouldReturnOk_WhenGroupExists()
        {
            // Arrange
            var group = DataGenerator.SeedGroup();
            var response = new GroupResponse(group);
            _mediatorMock.Setup(m => m.Send(It.IsAny<ReadGroupByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetById(group.Id, CancellationToken.None);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenGroupDoesNotExist()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            _mediatorMock.Setup(m => m.Send(It.IsAny<ReadGroupByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Result<GroupResponse>(new Exception("Read failed")));

            // Act
            var result = await _controller.GetById(groupId, CancellationToken.None);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetAll_ShouldReturnOk_WithListOfGroups()
        {
            // Arrange
            var response = new List<GroupResponse> { new (DataGenerator.SeedGroup())};
            _mediatorMock.Setup(m => m.Send(It.IsAny<ReadAllGroupsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetAll(CancellationToken.None);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Post_ShouldReturnCreatedAtAction_WhenSuccessful()
        {
            // Arrange
            var request = new GroupRequest(DataGenerator.SeedRandomString(10));
            var group = new Group { Name = request.Name };
            var response = new GroupResponse(group);
            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);
            
            // Act
            var result = await _controller.Post(request, CancellationToken.None);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
        }

        [Fact]
        public async Task Post_ShouldReturnBadRequest_WhenFailed()
        {
            // Arrange
            var request = new GroupRequest(DataGenerator.SeedRandomString(10));
            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Result<GroupResponse>(new Exception("Update failed")));

            // Act
            var result = await _controller.Post(request, CancellationToken.None);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Patch_ShouldReturnNoContent_WhenSuccessful()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var patchDoc = new JsonPatchDocument();
            _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Result<bool>(true));

            // Act
            var result = await _controller.Patch(groupId, patchDoc, CancellationToken.None);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task Patch_ShouldReturnBadRequest_WhenFailed()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            var patchDoc = new JsonPatchDocument();
            _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Result<bool>(new Exception("Update failed")));

            // Act
            var result = await _controller.Patch(groupId, patchDoc, CancellationToken.None);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Delete_ShouldReturnNoContent_WhenSuccessful()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Result<bool>(true));

            // Act
            var result = await _controller.Delete(groupId, CancellationToken.None);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task Delete_ShouldReturnBadRequest_WhenFailed()
        {
            // Arrange
            var groupId = Guid.NewGuid();
            _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Result<bool>(new Exception("Deletion failed")));

            // Act
            var result = await _controller.Delete(groupId, CancellationToken.None);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}
