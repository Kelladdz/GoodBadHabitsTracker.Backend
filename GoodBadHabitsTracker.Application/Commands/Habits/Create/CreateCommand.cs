using GoodBadHabitsTracker.Application.Abstractions.Messaging;
using GoodBadHabitsTracker.Application.DTOs.Request;
using GoodBadHabitsTracker.Application.DTOs.Response;

namespace GoodBadHabitsTracker.Application.Commands.Habits.Create
{
    public sealed record CreateCommand(HabitRequest Request) : ICommand<HabitResponse?>;
}
