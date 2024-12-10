using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Core.Interfaces;
using MediatR;
using LanguageExt.Common;
using System.Net;

namespace GoodBadHabitsTracker.Application.Commands.Habits.DeleteAllProgress
{
    internal sealed class DeleteAllProgressCommandHandler(
        IHabitsRepository habitsRepository,
        IUserAccessor userAccessor) : IRequestHandler<DeleteAllProgressCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(DeleteAllProgressCommand command, CancellationToken cancellationToken)
        {
            var user = await userAccessor.GetCurrentUser();
            if (user == null)
                return new Result<bool>(new AppException(HttpStatusCode.BadRequest, "User Not Found"));
            
            var userId = user.Id;


            await habitsRepository.DeleteAllProgressAsync(userId, cancellationToken);
            /*var postInProgressTodayTask = habitsRepository.PostInProgressTodayAsync(allHabits, cancellationToken);*/


            return new Result<bool>(true);


        }
    }
}
