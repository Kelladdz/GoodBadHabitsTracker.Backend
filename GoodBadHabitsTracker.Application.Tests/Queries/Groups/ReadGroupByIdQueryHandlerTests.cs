using FluentAssertions;
using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Application.Queries.Groups.ReadById;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Core.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.Tests.Queries.Groups
{
    public class ReadGroupByIdQueryHandlerTests
    {
        private readonly Mock<IGroupsRepository> _groupsRepositoryMock;
        private readonly ReadGroupByIdQueryHandler _handler;

        public ReadGroupByIdQueryHandlerTests()
        {
            _groupsRepositoryMock = new Mock<IGroupsRepository>();
            _handler = new ReadGroupByIdQueryHandler(_groupsRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_GroupNotFound_ReturnsNotFound()
        {
            // Arrange
            var query = new ReadGroupByIdQuery(Guid.NewGuid());
            _groupsRepositoryMock.Setup(x => x.ReadByIdAsync(query.Id, It.IsAny<CancellationToken>())).ReturnsAsync((Group)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsFaulted.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_GroupFound_ReturnsGroupResponse()
        {
            // Arrange
            var group = new Group { Name = "Test Group" };
            var query = new ReadGroupByIdQuery(group.Id);

            _groupsRepositoryMock.Setup(x => x.ReadByIdAsync(query.Id, It.IsAny<CancellationToken>())).ReturnsAsync(group);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.IfSucc(response =>
            {
                response.Group.Id.Should().Be(group.Id);
                response.Group.Name.Should().Be(group.Name);
            });
        }
    }
}
