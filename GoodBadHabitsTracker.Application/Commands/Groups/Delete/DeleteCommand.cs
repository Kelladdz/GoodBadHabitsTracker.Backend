using GoodBadHabitsTracker.Application.Abstractions.Messaging;

namespace GoodBadHabitsTracker.Application.Commands.Groups.Delete
{
    public sealed record DeleteCommand(Guid Id) : ICommand<bool>;
}
