using GoodBadHabitsTracker.Application.Abstractions.Messaging;
using LanguageExt.Common;
namespace GoodBadHabitsTracker.Application.Commands.Habits.DeleteAllProgress
{
    public sealed record DeleteAllProgressCommand() : ICommand<Result<bool>>;
}
