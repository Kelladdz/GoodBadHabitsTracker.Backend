using Bogus;
using FluentAssertions;
using GoodBadHabitsTracker.Application.Commands.Groups.Delete;
using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Core.Models;
using LanguageExt.Common;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.Tests.Commands.Groups.Delete
{
    public class DeleteGroupCommandHandlerTests
    {
        private readonly Mock<IGroupsRepository> _groupsRepositoryMock;
        private readonly DeleteGroupCommandHandler _handler;

        public DeleteGroupCommandHandlerTests()
        {
            _groupsRepositoryMock = new Mock<IGroupsRepository>();
            _handler = new DeleteGroupCommandHandler(_groupsRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenDeleteIsSuccessful()
        {
            //ARRANGE
            var name = new Faker<string>().CustomInstantiator(f => f.Commerce.ProductName());
            var groupToDelete = new Group { Name = name };
            var groupId = groupToDelete.Id;
            var command = new DeleteGroupCommand(groupId);

            _groupsRepositoryMock
                .Setup(x => x.FindAsync(groupId, default))
                .ReturnsAsync(groupToDelete);
            _groupsRepositoryMock
                .Setup(x => x.DeleteAsync(groupToDelete, default)); 

            //ACT
            var result = await _handler.Handle(command, default);

            //ASSERT
            result.IsSuccess.Should().BeTrue();

            _groupsRepositoryMock.Verify(x => x.FindAsync(groupId, default), Times.Once);
            _groupsRepositoryMock.Verify(x => x.DeleteAsync(groupToDelete, default), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenGroupDoesntExist()
        {
            //ARRANGE
            var groupId = Guid.NewGuid();
            var command = new DeleteGroupCommand(groupId);

            _groupsRepositoryMock.Setup(x => x.FindAsync(groupId, default)).ReturnsAsync((Group?)null);

            //ACT
            var result = await _handler.Handle(command, default);

            //ASSERT
            result.IsFaulted.Should().BeTrue();
            result.Should().BeEquivalentTo(new Result<bool>(new AppException(HttpStatusCode.NotFound, "Group Not Found")));

            _groupsRepositoryMock.Verify(x => x.FindAsync(groupId, default), Times.Once);
        }
    }
}
