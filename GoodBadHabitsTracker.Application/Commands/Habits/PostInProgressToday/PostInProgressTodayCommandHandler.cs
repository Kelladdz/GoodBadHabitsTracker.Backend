using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Core.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.Commands.Habits.PostInProgressToday
{
    internal sealed class PostInProgressTodayCommandHandler(
        IHabitsRepository habitsRepository,
        IUserAccessor userAccessor) : IRequestHandler<PostInProgressTodayCommand, bool>
    {
        public async Task<bool> Handle(PostInProgressTodayCommand command, CancellationToken cancellationToken)
        {
            var user = await userAccessor.GetCurrentUser()
                ?? throw new AppException(System.Net.HttpStatusCode.Unauthorized, "User not found");
            var userId = user.Id;

            return await habitsRepository.PostInProgressTodayAsync(userId, cancellationToken);
        }
    }
}
