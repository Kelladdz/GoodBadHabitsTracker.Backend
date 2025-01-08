using GoodBadHabitsTracker.Core.Interfaces;
using MediatR;
using LanguageExt.Common;
using GoodBadHabitsTracker.Application.Exceptions;
using System.Net;

namespace GoodBadHabitsTracker.Application.Commands.Habits.Delete
{
    internal sealed class DeleteHabitCommandHandler(
        IHabitsDbContext dbContext,
        IUserAccessor userAccessor) : IRequestHandler<DeleteHabitCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(DeleteHabitCommand command, CancellationToken cancellationToken)
        {
            var user = await userAccessor.GetCurrentUser();
            if (user == null)
                return new Result<bool>(new AppException(HttpStatusCode.BadRequest, "User Not Found"));

            var habitId = command.Id;
            var userId = user.Id;

            dbContext.BeginTransaction();

            try
            {
                var habitToDelete = await dbContext.ReadHabitByIdAsync(habitId);
                if (habitToDelete == null)
                {
                    await dbContext.CommitAsync();
                    return new Result<bool>(new AppException(HttpStatusCode.NotFound, "Habit Not Found"));
                }

                dbContext.DeleteHabit(habitToDelete);

                await dbContext.CommitAsync();
                return new Result<bool>(true);
            }
            catch (Exception ex)
            {
                await dbContext.RollbackAsync();
                return new Result<bool>(new AppException(HttpStatusCode.BadRequest, ex.Message));
            }
        }
    }
}