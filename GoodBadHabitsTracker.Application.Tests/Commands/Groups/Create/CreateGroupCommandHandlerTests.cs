using FluentAssertions;
using GoodBadHabitsTracker.Application.Commands.Groups.Create;
using GoodBadHabitsTracker.Application.DTOs.Request;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Core.Models;
using Moq;
using Bogus;
using GoodBadHabitsTracker.TestMisc;
using GoodBadHabitsTracker.Application.Exceptions;
using System.Net;
using LanguageExt.Common;
using GoodBadHabitsTracker.Application.DTOs.Response;

namespace GoodBadHabitsTracker.Application.Tests.Commands.Groups.Create;

public class CreateGroupCommandHandlerTests
{
    private readonly Mock<IGroupsRepository> _groupsRepositoryMock;
    private readonly Mock<IUserAccessor> _userAccessorMock;
    private readonly CreateGroupCommandHandler _handler;

    public CreateGroupCommandHandlerTests()
    {
        _groupsRepositoryMock = new Mock<IGroupsRepository>();
        _userAccessorMock = new Mock<IUserAccessor>();
        _handler = new CreateGroupCommandHandler(_groupsRepositoryMock.Object, _userAccessorMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenGroupIsCreated()
    {
        //ARRANGE
        var name = new Faker<string>().CustomInstantiator(f => f.Commerce.ProductName());
        var userId = Guid.NewGuid();
        var request = new GroupRequest(name);
        var command = new CreateGroupCommand(request);
        var groupToInsert = new Group
        {
            UserId = userId,
            Name = name
        };
        var expectedResult = new Result<GroupResponse>(new GroupResponse(groupToInsert));

        _userAccessorMock.Setup(x => x.GetCurrentUser()).ReturnsAsync(new User { Id = userId });
        _groupsRepositoryMock.Setup(x => x.InsertAsync(groupToInsert, default)).Returns(Task.CompletedTask);

        //ACT
        var result = await _handler.Handle(command, default);

        //ASSERT
        result.IsSuccess.Should().BeTrue();

        _userAccessorMock.Verify(x => x.GetCurrentUser(), Times.Once);
        _groupsRepositoryMock.Verify(x => x.InsertAsync(It.IsAny<Group>(), default), Times.Once);

    }
    [Fact]
    public async Task Handle_ShouldReturnFail_WhenUserIsNull()
    {
        //ARRANGE
        var name = new Faker<string>().CustomInstantiator(f => f.Commerce.ProductName());
        var request = new GroupRequest(name);
        var command = new CreateGroupCommand(request);
        var expectedResult = new Result<GroupResponse>(new AppException(HttpStatusCode.BadRequest, "User Not Found"));

        _userAccessorMock.Setup(x => x.GetCurrentUser())!.ReturnsAsync((User?)null);

        //ACT
        var result = await _handler.Handle(command, CancellationToken.None);

        //ASSERT
        result.IsFaulted.Should().BeTrue();
        result.Should().BeEquivalentTo(expectedResult);

        _userAccessorMock.Verify(x => x.GetCurrentUser(), Times.Once);
    }
}
