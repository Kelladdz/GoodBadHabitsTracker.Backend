using GoodBadHabitsTracker.Application.Abstractions.Messaging;
using GoodBadHabitsTracker.Application.DTOs.Response;
using LanguageExt.Common;

namespace GoodBadHabitsTracker.Application.Queries.Habits.ReadAll
{
    public sealed record ReadAllHabitsQuery() : IQuery<Result<IEnumerable<HabitResponse>>>;
}
