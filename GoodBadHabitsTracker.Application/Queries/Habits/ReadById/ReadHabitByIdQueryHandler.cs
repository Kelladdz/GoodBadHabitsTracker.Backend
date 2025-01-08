using GoodBadHabitsTracker.Application.DTOs.Response;
using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Infrastructure.Persistance;
using LanguageExt.Common;
using MediatR;
using System.Net;
using GoodBadHabitsTracker.Application.Services;

namespace GoodBadHabitsTracker.Application.Queries.Habits.ReadById
{
    internal sealed class ReadHabitByIdQueryHandler(
        IHabitsDbContext dbContext,
        IUserAccessor userAccessor) : IRequestHandler<ReadHabitByIdQuery, Result<HabitResponse>>
    {
        public async Task<Result<HabitResponse>> Handle(ReadHabitByIdQuery query, CancellationToken cancellationToken)
        {
            var user = await userAccessor.GetCurrentUser();
            if (user is null)
                return new Result<HabitResponse>(new AppException(HttpStatusCode.Unauthorized, "User not found"));

            var userId = user.Id;
            var groupId = query.Id;

            dbContext.BeginTransaction();

            try
            {
                var habit = await dbContext.ReadHabitByIdAsync(groupId);
                if (habit is null)
                {
                    await dbContext.CommitAsync();
                    return new Result<HabitResponse>(new AppException(HttpStatusCode.NotFound, "Habit Not Found"));
                }

                await dbContext.CommitAsync();
                var stats = HabitStatistics.GetStats(habit);
                return new Result<HabitResponse>(new HabitResponse(habit, stats));
            }
            catch (Exception ex)
            {
                await dbContext.RollbackAsync();
                return new Result<HabitResponse>(new AppException(HttpStatusCode.BadRequest, ex.Message));
            }
        }
    }
}