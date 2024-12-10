using FluentAssertions;
using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Application.Queries.Groups.ReadAll;
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
    public class ReadAllGroupsQueryHandlerTests
    {
        private readonly Mock<IGroupsRepository> _groupsRepositoryMock;
        private readonly Mock<IUserAccessor> _userAccessorMock;
        private readonly ReadAllGroupsQueryHandler _handler;

        public ReadAllGroupsQueryHandlerTests()
        {
            _groupsRepositoryMock = new Mock<IGroupsRepository>();
            _userAccessorMock = new Mock<IUserAccessor>();
            _handler = new ReadAllGroupsQueryHandler(_groupsRepositoryMock.Object, _userAccessorMock.Object);
        }

        [Fact]
        public async Task Handle_UserIsNull_ReturnsUnauthorized()
        {
            // Arrange
            var query = new ReadAllGroupsQuery();

            _userAccessorMock.Setup(x => x.GetCurrentUser()).ReturnsAsync((User?)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsFaulted.Should().BeTrue();/*
            result.IfFail(ex => ex.Should().BeOfType<AppException>()
                .Which.StatusCode.Should().Be(HttpStatusCode.Unauthorized));*/
        }

        [Fact]
        public async Task Handle_GroupsAreNull_ReturnsEmptyList()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid() };
            var query = new ReadAllGroupsQuery();

            _userAccessorMock.Setup(x => x.GetCurrentUser()).ReturnsAsync(user);
            _groupsRepositoryMock.Setup(x => x.ReadAllAsync(user.Id, It.IsAny<CancellationToken>()))!.ReturnsAsync((IEnumerable<Group>?)null);

            

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.IfSucc(groups => groups.Should().BeEmpty());
        }

        [Fact]
        public async Task Handle_GroupsExist_ReturnsGroupResponses()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid() };
            var query = new ReadAllGroupsQuery();
            var groups = new List<Group>
            {
                new Group { Name = "Group1" },
                new Group { Name = "Group2" }
            };

            _userAccessorMock.Setup(x => x.GetCurrentUser()).ReturnsAsync(user);
            _groupsRepositoryMock.Setup(x => x.ReadAllAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(groups);

            

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.IfSucc(groupResponses =>
            {
                groupResponses.Should().HaveCount(2);
                groupResponses.Should().ContainSingle(gr => gr.Group.Name == "Group1");
                groupResponses.Should().ContainSingle(gr => gr.Group.Name == "Group2");
            });
        }
    }
}
