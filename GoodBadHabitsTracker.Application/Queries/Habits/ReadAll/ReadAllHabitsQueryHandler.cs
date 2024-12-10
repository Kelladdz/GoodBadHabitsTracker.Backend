using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Application.Exceptions;
using MediatR;
using GoodBadHabitsTracker.Application.DTOs.Response;
using LanguageExt.Common;
using System.Net;

namespace GoodBadHabitsTracker.Application.Queries.Habits.ReadAll
{
    internal sealed class ReadAllHabitsQueryHandler(IHabitsRepository habitsRepository, IUserAccessor userAccessor) : IRequestHandler<ReadAllHabitsQuery, Result<IEnumerable<HabitResponse>>>
    {
        public async Task<Result<IEnumerable<HabitResponse>>> Handle(ReadAllHabitsQuery query, CancellationToken cancellationToken)
        {
            var user = await userAccessor.GetCurrentUser();
            if (user is null)
                return new Result<IEnumerable<HabitResponse>>(new AppException(HttpStatusCode.Unauthorized, "User not found"));

            var userId = user.Id;
            var habits = await habitsRepository.ReadAllAsync(userId, cancellationToken);
            if (habits is null)
                return new Result<IEnumerable<HabitResponse>>([]);

            var response = new List<HabitResponse>();
            foreach (var habit in habits)
            {
                response.Add(new HabitResponse(habit));
            }

            return new Result<IEnumerable<HabitResponse>>(response);
        }
    }
}
