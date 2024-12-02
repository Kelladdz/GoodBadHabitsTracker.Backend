using GoodBadHabitsTracker.Application.DTOs.Response;
using GoodBadHabitsTracker.Core.Interfaces;
using MediatR;

namespace GoodBadHabitsTracker.Application.Queries.Habits.ReadById
{
    internal sealed class ReadByIdQueryHandler(IHabitsRepository habitsRepository) : IRequestHandler<ReadByIdQuery, HabitResponse?>
    {
        public async Task<HabitResponse?> Handle(ReadByIdQuery query, CancellationToken cancellationToken)
        {
            var habitId = query.Id;

            var habit = await habitsRepository.ReadByIdAsync(habitId, cancellationToken);
            if (habit is null)
                return null;

            return new HabitResponse(habit);
        }
    }
}
