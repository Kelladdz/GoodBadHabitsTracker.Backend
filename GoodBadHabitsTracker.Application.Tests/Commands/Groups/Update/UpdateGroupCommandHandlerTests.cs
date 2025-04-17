using Bogus;
using FluentAssertions;
using FluentValidation;
using GoodBadHabitsTracker.Application.Commands.Groups.Update;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.TestMisc;
using Moq;

namespace GoodBadHabitsTracker.Application.Tests.Commands.Groups.Update
{
    public class UpdateGroupCommandHandlerTests
    {
        private readonly Mock<IHabitsDbContext> _dbContextMock;
        private readonly Mock<IUserAccessor> _userAccessorMock;
        private readonly IValidator<Group> _validator;
        private readonly UpdateGroupCommandHandler _handler;

        public UpdateGroupCommandHandlerTests()
        {
            _dbContextMock = new Mock<IHabitsDbContext>();
            _userAccessorMock = new Mock<IUserAccessor>();
            _validator = new AppliedGroupValidator();
            _handler = new UpdateGroupCommandHandler(_dbContextMock.Object, _userAccessorMock.Object, _validator);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenGroupWasUpdated()
        {
            //ARRANGE
            var userId = Guid.NewGuid();
            var name = new Faker<string>().CustomInstantiator(f => f.Commerce.ProductName());
            var document = DataGenerator.SeedGroupJsonPatchDocument();
            var groupToUpdate = new Group { Name = name, UserId = userId};
            var groupId = groupToUpdate.Id;
            var command = new UpdateGroupCommand(groupId, document);

            _userAccessorMock.Setup(x => x.GetCurrentUser()).ReturnsAsync(new User { Id = userId });
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