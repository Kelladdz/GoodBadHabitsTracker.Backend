using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Application.Exceptions;
using MediatR;
using GoodBadHabitsTracker.Application.DTOs.Response;
using LanguageExt.Common;
using System.Net;
using GoodBadHabitsTracker.Application.Services;
using GoodBadHabitsTracker.Infrastructure.Persistance;

namespace GoodBadHabitsTracker.Application.Queries.Habits.ReadAll
{
    internal sealed class ReadAllHabitsQueryHandler(
        IHabitsDbContext dbContext,
        IUserAccessor userAccessor) : IRequestHandler<ReadAllHabitsQuery, Result<IEnumerable<HabitResponse>>>
    {
        public async Task<Result<IEnumerable<HabitResponse>>> Handle(ReadAllHabitsQuery query, CancellationToken cancellationToken)
        {
            var user = await userAccessor.GetCurrentUser();
            if (user is null)
                return new Result<IEnumerable<HabitResponse>>(new AppException(HttpStatusCode.Unauthorized, "User not found"));

            var userId = user.Id;

            dbContext.BeginTransaction();

            try
            {
                var allHabits = await dbContext.ReadAllHabitsAsync(userId);

                if (!allHabits.Any())
                {
                    await dbContext.CommitAsync();
                    return new Result<IEnumerable<HabitResponse>>([]);
                }

                var response = new List<HabitResponse>();
                foreach (var habit in allHabits)
                {
                    var stats = HabitStatistics.GetStats(habit);
                    response.Add(new HabitResponse(habit, stats));
                }

                await dbContext.CommitAsync();
                return new Result<IEnumerable<HabitResponse>>(response);
            }
            catch (Exception ex)
            {
                await dbContext.RollbackAsync();
                return new Result<IEnumerable<HabitResponse>>(new AppException(HttpStatusCode.BadRequest, ex.Message));
            }
        }
    }
}