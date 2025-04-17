using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using GoodBadHabitsTracker.Application.Commands.DayResults.Update;
using GoodBadHabitsTracker.Application.DTOs.Request;
using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Core.Enums;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.Infrastructure.Persistance;
using GoodBadHabitsTracker.TestMisc;
using LanguageExt.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class UpdateDayResultCommandHandlerTests
{
    private readonly Mock<IHabitsDbContext> _dbContextMock;
    private readonly Mock<IUserAccessor> _userAccessorMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly IRequestHandler<UpdateDayResultCommand, Result<bool>> _handler;

    public UpdateDayResultCommandHandlerTests()
    {
        _dbContextMock = new Mock<IHabitsDbContext>();
        _userAccessorMock = new Mock<IUserAccessor>();
        _loggerMock = new Mock<ILogger>();
        _handler = new UpdateDayResultCommandHandler(_dbContextMock.Object, _userAccessorMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsUnauthorized()
    {
        // Arrange
        var request = DataGenerator.SeedUpdateDayResultRequest();
        var command = new UpdateDayResultCommand(Guid.NewGuid(), request);

        _userAccessorMock.Setup(x => x.GetCurrentUser()).ReturnsAsync((User)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFaulted.Should().BeTrue();
        result.IfFail(ex => ex.Should().BeOfType<AppException>().Which.Code.Should().Be(HttpStatusCode.Unauthorized));
        result.IfFail(ex => ex.Should().BeOfType<AppException>().Which.Errors.Should().Be("User Not Found"));
    }

    [Fact]
    public async Task Handle_DayResultNotFound_ReturnsNotFound()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid() };
        var request = DataGenerator.SeedUpdateDayResultRequest();
        var command = new UpdateDayResultCommand(Guid.NewGuid(), request);

        _userAccessorMock.Setup(x => x.GetCurrentUser()).ReturnsAsync(user);
        _dbContextMock.Setup(x => x.DayResults.FindAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((DayResult)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFaulted.Should().BeTrue();
        result.IfFail(ex => ex.Should().BeOfType<AppException>().Which.Code.Should().Be(HttpStatusCode.NotFound));
        result.IfFail(ex => ex.Should().BeOfType<AppException>().Which.Errors.Should().Be("Day result not found"));
    }

    [Fact]
    public async Task Handle_HabitNotFound_ReturnsNotFound()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid() };
        var request = DataGenerator.SeedUpdateDayResultRequest();
        var command = new UpdateDayResultCommand(Guid.NewGuid(), request);

        _userAccessorMock.Setup(x => x.GetCurrentUser()).ReturnsAsync(user);
        _dbContextMock.Setup(x => x.DayResults.FindAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(new DayResult { Date = DateOnly.FromDateTime(DateTime.UtcNow), Status = 0});
        _dbContextMock.Setup(x => x.Habits.FindAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Habit)null);


        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFaulted.Should().BeTrue();
        result.IfFail(ex => ex.Should().BeOfType<AppException>().Which.Code.Should().Be(HttpStatusCode.NotFound));
        result.IfFail(ex => ex.Should().BeOfType<AppException>().Which.Errors.Should().Be("Habit not found"));
    }
}