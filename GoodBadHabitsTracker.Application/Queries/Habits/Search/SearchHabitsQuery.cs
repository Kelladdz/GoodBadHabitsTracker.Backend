using GoodBadHabitsTracker.Application.Abstractions.Messaging;
using GoodBadHabitsTracker.Application.DTOs.Response;
using LanguageExt.Common;

namespace GoodBadHabitsTracker.Application.Queries.Habits.Search
{
    public sealed record SearchHabitsQuery(string? Term, DateOnly Date) : IQuery<Result<IEnumerable<HabitResponse>>>;
}
