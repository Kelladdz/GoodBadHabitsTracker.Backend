using GoodBadHabitsTracker.Application.DTOs.Response;
using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Core.Interfaces;
using LanguageExt.Common;
using MediatR;
using System.Net;

namespace GoodBadHabitsTracker.Application.Queries.Habits.ReadById
{
    internal sealed class ReadHabitByIdQueryHandler(IHabitsRepository habitsRepository) : IRequestHandler<ReadHabitByIdQuery, Result<HabitResponse>>
    {
        public async Task<Result<HabitResponse>> Handle(ReadHabitByIdQuery query, CancellationToken cancellationToken)
        {
            var habitId = query.Id;

            var habit = await habitsRepository.ReadByIdAsync(habitId, cancellationToken);
            if (habit is null)
                return new Result<HabitResponse>(new AppException(HttpStatusCode.NotFound, "Habit not found"));

            return new Result<HabitResponse>(new HabitResponse(habit));
        }
    }
}
