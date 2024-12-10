using GoodBadHabitsTracker.Application.Abstractions.Messaging;
using GoodBadHabitsTracker.Application.DTOs.Response;
using LanguageExt.Common;

namespace GoodBadHabitsTracker.Application.Queries.Habits.ReadById
{
    public sealed record ReadHabitByIdQuery(Guid Id) : IQuery<Result<HabitResponse>>;
}
