using GoodBadHabitsTracker.Application.Abstractions.Messaging;
using LanguageExt.Common;
using Microsoft.AspNetCore.JsonPatch;

namespace GoodBadHabitsTracker.Application.Commands.Habits.Update
{
    public sealed record UpdateHabitCommand(Guid Id, JsonPatchDocument Request) : ICommand<Result<bool>>;
}
