using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Core.Interfaces;
using MediatR;
using LanguageExt.Common;
using GoodBadHabitsTracker.Infrastructure.Persistance;
using GoodBadHabitsTracker.Core.Models;
using System.Net;

namespace GoodBadHabitsTracker.Application.Commands.Habits.DeleteAll
{
    internal sealed class DeleteAllHabitsCommandHandler(
        IHabitsDbContext dbContext,
        IUserAccessor userAccessor) : IRequestHandler<DeleteAllHabitsCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(DeleteAllHabitsCommand command, CancellationToken cancellationToken)
        {
            var user = await userAccessor.GetCurrentUser();
            if (user == null)
                return new Result<bool>(new AppException(System.Net.HttpStatusCode.BadRequest, "User Not Found"));
            
            var userId = user.Id;

            dbContext.BeginTransaction();

            try
            {
                var allHabits = await dbContext.ReadAllHabitsAsync(userId);
                if (!allHabits!.Any())
                    return new Result<bool>(true);

                dbContext.DeleteRange(allHabits!);

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
