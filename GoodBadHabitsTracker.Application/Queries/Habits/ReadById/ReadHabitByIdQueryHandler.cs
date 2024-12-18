using GoodBadHabitsTracker.Application.DTOs.Response;
using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Infrastructure.Persistance;
using LanguageExt.Common;
using MediatR;
using System.Net;

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
                var habit = await dbContext.ReadHabitByIdAsync(userId, groupId);
                if (habit is null)
                {
                    await dbContext.CommitAsync();
                    return new Result<HabitResponse>(new AppException(HttpStatusCode.NotFound, "Habit Not Found"));
                }

                await dbContext.CommitAsync();
                return new Result<HabitResponse>(new HabitResponse(habit));
            }
            catch (Exception ex)
            {
                await dbContext.RollbackAsync();
                return new Result<HabitResponse>(new AppException(HttpStatusCode.BadRequest, ex.Message));
            }
        }
    }
}
