using GoodBadHabitsTracker.Application.Abstractions.Messaging;

namespace GoodBadHabitsTracker.Application.Commands.Habits.Delete
{
    public sealed record DeleteCommand(Guid Id) : ICommand<bool>;
}
