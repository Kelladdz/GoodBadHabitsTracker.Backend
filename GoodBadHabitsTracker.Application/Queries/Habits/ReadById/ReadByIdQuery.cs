using GoodBadHabitsTracker.Application.Abstractions.Messaging;
using GoodBadHabitsTracker.Application.DTOs.Response;

namespace GoodBadHabitsTracker.Application.Queries.Habits.ReadById
{
    public sealed record ReadByIdQuery(Guid Id) : IQuery<HabitResponse?>;
}
