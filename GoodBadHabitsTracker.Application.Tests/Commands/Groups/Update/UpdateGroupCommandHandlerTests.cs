using Bogus;
using Bogus.DataSets;
using FluentAssertions;
using GoodBadHabitsTracker.Application.Commands.Generic.Update;
using GoodBadHabitsTracker.Application.Commands.Groups.Delete;
using GoodBadHabitsTracker.Application.Commands.Groups.Update;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.TestMisc;
using Microsoft.AspNetCore.JsonPatch;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.Tests.Commands.Groups.Update
{
    public class UpdateGroupCommandHandlerTests
    {
        private readonly Mock<IGroupsRepository> _groupsRepositoryMock;
        private readonly UpdateGroupCommandHandler _handler;

        public UpdateGroupCommandHandlerTests()
        {
            _groupsRepositoryMock = new Mock<IGroupsRepository>();
            _handler = new UpdateGroupCommandHandler(_groupsRepositoryMock.Object);
        }
    
        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenGroupWasUpdated()
        {
            //ARRANGE
            var name = new Faker<string>().CustomInstantiator(f => f.Commerce.ProductName());
            var document = DataGenerator.SeedGroupJsonPatchDocument();
            var groupToUpdate = new Group { Name = name };
            var groupId = groupToUpdate.Id;
            var command = new UpdateGroupCommand(groupId, document);

            _groupsRepositoryMock.Setup(r => r.FindAsync(groupId, It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(groupToUpdate);
            _groupsRepositoryMock.Setup(r => r.UpdateAsync(document, groupToUpdate, It.IsAny<CancellationToken>()));

            //ACT
            var result = await _handler.Handle(command, CancellationToken.None);

            //ASSERT
            result.IsSuccess.Should().BeTrue();

            _groupsRepositoryMock.Verify(r => r.FindAsync(groupId, It.IsAny<CancellationToken>()), Times.Once);
            _groupsRepositoryMock.Verify(r => r.UpdateAsync(document, groupToUpdate, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenGroupIsNull()
        {
            //ARRANGE
            var name = new Faker<string>().CustomInstantiator(f => f.Commerce.ProductName());
            var document = DataGenerator.SeedGroupJsonPatchDocument();
            var groupToUpdate = new Group { Name = name };
            var groupId = groupToUpdate.Id;
            var command = new UpdateGroupCommand(groupId, document);

            _groupsRepositoryMock.Setup(r => r.FindAsync(groupId, default))
                                  .ReturnsAsync((Group?)null);

            //ACT
            var result = await _handler.Handle(command, default);

            //ASSERT
            result.IsFaulted.Should().BeTrue();

            _groupsRepositoryMock.Verify(r => r.FindAsync(groupId, default), Times.Once);
        }
    }
}
