using GoodBadHabitsTracker.Application.Abstractions.Messaging;
using GoodBadHabitsTracker.Application.DTOs.Request;
using GoodBadHabitsTracker.Application.DTOs.Response;
using LanguageExt.Common;

namespace GoodBadHabitsTracker.Application.Commands.Habits.Create
{
    public sealed record CreateHabitCommand(HabitRequest Request) : ICommand<Result<CreateHabitResponse>>;
}
