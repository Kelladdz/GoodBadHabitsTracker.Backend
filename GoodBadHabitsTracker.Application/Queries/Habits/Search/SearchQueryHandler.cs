using GoodBadHabitsTracker.Core.Interfaces;
using MediatR;
using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Application.DTOs.Response;

namespace GoodBadHabitsTracker.Application.Queries.Habits.Search
{
    internal sealed class SearchQueryHandler(IHabitsRepository habitsRepository, IUserAccessor userAccessor) : IRequestHandler<SearchQuery, IEnumerable<HabitResponse>>
    {
        public async Task<IEnumerable<HabitResponse>> Handle(SearchQuery query, CancellationToken cancellationToken)
        {
            var user = await userAccessor.GetCurrentUser()
                ?? throw new AppException(System.Net.HttpStatusCode.Unauthorized, "User not found");
            var userId = user.Id;

            var term = query.Term;
            var date = query.Date;

            var habits = await habitsRepository.SearchAsync(term, date, userId, cancellationToken);
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
