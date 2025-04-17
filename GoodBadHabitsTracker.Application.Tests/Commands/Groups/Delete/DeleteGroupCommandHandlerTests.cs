using Bogus;
using FluentAssertions;
using GoodBadHabitsTracker.Application.Commands.Groups.Delete;
using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Core.Models;
using LanguageExt.Common;
using Moq;
using System.Net;
using GoodBadHabitsTracker.Core.Interfaces;

namespace GoodBadHabitsTracker.Application.Tests.Commands.Groups.Delete
{
    public class DeleteGroupCommandHandlerTests
    {
        private readonly Mock<IHabitsDbContext> _dbContextMock;
        private readonly Mock<IUserAccessor> _userAccessorMock;
        private readonly DeleteGroupCommandHandler _handler;

        public DeleteGroupCommandHandlerTests()
        {
            _dbContextMock = new Mock<IHabitsDbContext>();
            _userAccessorMock = new Mock<IUserAccessor>();
            _handler = new DeleteGroupCommandHandler(_dbContextMock.Object, _userAccessorMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenDeleteIsSuccessful()
        {
            //ARRANGE
            var userId = Guid.NewGuid();
            var name = new Faker<string>().CustomInstantiator(f => f.Commerce.ProductName());
            var groupToDelete = new Group { Name = name, UserId = userId};
            var groupId = groupToDelete.Id;
            var command = new DeleteGroupCommand(groupId);

            _userAccessorMock.Setup(x => x.GetCurrentUser()).ReturnsAsync(new User { Id = userId });
            _dbContextMock
                .Setup(x => x.ReadGroupByIdAsync(groupId, userId))
                .ReturnsAsync(groupToDelete);
            _dbContextMock
                .Setup(x => x.DeleteGroup(groupToDelete));

            //ACT
            var result = await _handler.Handle(command, default);

            //ASSERT
            result.IsSuccess.Should().BeTrue();

            _dbContextMock.Verify(x => x.ReadGroupByIdAsync(groupId, default), Times.Once);
            _dbContextMock.Verify(x => x.DeleteGroup(groupToDelete), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenUserNotFound()
        {
            //ARRANGE
            var groupId = Guid.NewGuid();
            var command = new DeleteGroupCommand(groupId);

            _userAccessorMock.Setup(x => x.GetCurrentUser())!.ReturnsAsync((User?)null);

            //ACT
            var result = await _handler.Handle(command, default);

            //ASSERT
            result.IsFaulted.Should().BeTrue();
            result.Should().BeEquivalentTo(new Result<bool>(new AppException(HttpStatusCode.NotFound, "User Not Found")));

            _dbContextMock.Verify(x => x.ReadGroupByIdAsync(groupId, default), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenGroupDoesntExist()
        {
            //ARRANGE
            var groupId = Guid.NewGuid();
            var command = new DeleteGroupCommand(groupId);

            _dbContextMock.Setup(x => x.ReadGroupByIdAsync(groupId, default)).ReturnsAsync((Group?)null);

            //ACT
            var result = await _handler.Handle(command, default);

            //ASSERT
            result.IsFaulted.Should().BeTrue();
            result.Should().BeEquivalentTo(new Result<bool>(new AppException(HttpStatusCode.NotFound, "Group Not Found")));

            _dbContextMock.Verify(x => x.ReadGroupByIdAsync(groupId, default), Times.Once);
        }
    }
}