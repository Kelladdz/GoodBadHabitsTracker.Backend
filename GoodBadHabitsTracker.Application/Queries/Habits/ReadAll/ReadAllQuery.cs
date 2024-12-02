using GoodBadHabitsTracker.Application.Abstractions.Messaging;
using GoodBadHabitsTracker.Application.DTOs.Response;

namespace GoodBadHabitsTracker.Application.Queries.Habits.ReadAll
{
    public sealed record ReadAllQuery() : IQuery<IEnumerable<HabitResponse>>;
}
