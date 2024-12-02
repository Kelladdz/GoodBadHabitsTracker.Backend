using GoodBadHabitsTracker.Application.Abstractions.Messaging;
using GoodBadHabitsTracker.Application.DTOs.Response;

namespace GoodBadHabitsTracker.Application.Queries.Habits.Search
{
    public sealed record SearchQuery(string? Term, DateOnly Date) : IQuery<IEnumerable<HabitResponse>>;
}
