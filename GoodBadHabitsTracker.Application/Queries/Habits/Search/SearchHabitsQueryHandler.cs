using GoodBadHabitsTracker.Core.Interfaces;
using MediatR;
using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Application.DTOs.Response;
using LanguageExt.Common;
using System.Net;

namespace GoodBadHabitsTracker.Application.Queries.Habits.Search
{
    internal sealed class SearchHabitsQueryHandler(IHabitsRepository habitsRepository, IUserAccessor userAccessor) : IRequestHandler<SearchHabitsQuery, Result<IEnumerable<HabitResponse>>>
    {
        public async Task<Result<IEnumerable<HabitResponse>>> Handle(SearchHabitsQuery query, CancellationToken cancellationToken)
        {
            var user = await userAccessor.GetCurrentUser();
            if (user is null)
                return new Result<IEnumerable<HabitResponse>>(new AppException(HttpStatusCode.Unauthorized, "User not found"));

            var userId = user.Id;

            var term = query.Term;
            var date = query.Date;

            var habits = await habitsRepository.SearchAsync(term, date, userId, cancellationToken);

            var response = new List<HabitResponse>();
            foreach (var habit in habits)
            {
                response.Add(new HabitResponse(habit));
            }

            return new Result<IEnumerable<HabitResponse>>(response);
        }
    }
}
