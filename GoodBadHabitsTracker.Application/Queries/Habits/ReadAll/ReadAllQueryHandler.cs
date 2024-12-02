using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Application.Exceptions;
using MediatR;
using GoodBadHabitsTracker.Application.DTOs.Response;

namespace GoodBadHabitsTracker.Application.Queries.Habits.ReadAll
{
    internal sealed class ReadAllQueryHandler(IHabitsRepository habitsRepository, IUserAccessor userAccessor) : IRequestHandler<ReadAllQuery, IEnumerable<HabitResponse>>
    {
        public async Task<IEnumerable<HabitResponse>> Handle(ReadAllQuery query, CancellationToken cancellationToken)
        {
            var user = await userAccessor.GetCurrentUser()
                ?? throw new AppException(System.Net.HttpStatusCode.Unauthorized, "User not found");

            var userId = user.Id;
            var habits = await habitsRepository.ReadAllAsync(userId, cancellationToken);
            if (habits is null)
                return [];

            var response = new List<HabitResponse>();
            foreach (var habit in habits)
            {
                response.Add(new HabitResponse(habit));
            }

            return response;
        }
    }
}
