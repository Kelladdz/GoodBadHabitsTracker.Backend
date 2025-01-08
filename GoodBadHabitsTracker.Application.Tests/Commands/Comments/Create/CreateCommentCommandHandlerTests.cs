using AutoMapper;
using Bogus;
using GoodBadHabitsTracker.Application.Commands.Comments.Create;
using GoodBadHabitsTracker.Application.Commands.Groups.Create;
using GoodBadHabitsTracker.Application.DTOs.Request;
using GoodBadHabitsTracker.Application.DTOs.Response;
using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.Infrastructure.Persistance;
using GoodBadHabitsTracker.TestMisc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using LanguageExt.Common;
using System.Threading.Tasks;
using FluentAssertions;

namespace GoodBadHabitsTracker.Application.Tests.Commands.Comments.Create
{
    public class CreateCommentCommandHandlerTests
    {
        private readonly Mock<IHabitsDbContext> _dbContextMock;
        private readonly Mock<IUserAccessor> _userAccessorMock;
        private readonly Mock<ILogger> _loggerMock;
        private readonly CreateCommentCommandHandler _handler;

        public CreateCommentCommandHandlerTests()
        {
            _dbContextMock = new Mock<IHabitsDbContext>();
            _userAccessorMock = new Mock<IUserAccessor>();
            _loggerMock = new Mock<ILogger>();
            _handler = new CreateCommentCommandHandler(
                _dbContextMock.Object, _userAccessorMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenCommentIsCreated()
        {
            //ARRANGE
            var habitId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var request = DataGenerator.SeedCreateCommentRequest();
            var command = new CreateCommentCommand(habitId, request);
            var commentToInsert = new Comment
            {
                Body = request.Body,
                Date = request.Date,
                HabitId = habitId
            };

            var expectedResult = new Result<CommentResponse>(new CommentResponse(commentToInsert));

            _userAccessorMock.Setup(x => x.GetCurrentUser()).ReturnsAsync(new User { Id = userId });
            _dbContextMock.Setup(x => x.InsertCommentAsync(commentToInsert)).Returns(Task.CompletedTask);

            //ACT
            var result = await _handler.Handle(command, default);

            //ASSERT
            result.IsSuccess.Should().BeTrue();

        }
        [Fact]
        public async Task Handle_ShouldReturnFail_WhenUserIsNull()
        {
            //ARRANGE
            var request = DataGenerator.SeedCreateCommentRequest();
            var userId = Guid.NewGuid();
            var habitId = Guid.NewGuid();
            var command = new CreateCommentCommand(habitId, request);
            var expectedResult = new Result<CommentResponse>(new AppException(HttpStatusCode.Unauthorized, "User Not Found"));

            _userAccessorMock.Setup(x => x.GetCurrentUser())!.ReturnsAsync((User?)null);

            //ACT
            var result = await _handler.Handle(command, CancellationToken.None);

            //ASSERT
            result.IsFaulted.Should().BeTrue();
            result.Should().BeEquivalentTo(expectedResult);

            _userAccessorMock.Verify(x => x.GetCurrentUser(), Times.Once);
        }
    }
}
