using GoodBadHabitsTracker.Application.Abstractions.Messaging;
using LanguageExt.Common;

namespace GoodBadHabitsTracker.Application.Commands.Habits.Delete
{
    public sealed record DeleteHabitCommand(Guid Id) : ICommand<Result<bool>>;
}
