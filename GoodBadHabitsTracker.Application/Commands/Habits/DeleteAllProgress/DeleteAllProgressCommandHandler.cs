using GoodBadHabitsTracker.Application.Commands.Habits.DeleteAll;
using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Core.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.Commands.Habits.DeleteAllProgress
{
    internal sealed class DeleteAllProgressCommandHandler(
        IHabitsRepository habitsRepository,
        IUserAccessor userAccessor) : IRequestHandler<DeleteAllProgressCommand, bool>
    {
        public async Task<bool> Handle(DeleteAllProgressCommand command, CancellationToken cancellationToken)
        {
            var user = await userAccessor.GetCurrentUser()
                ?? throw new AppException(System.Net.HttpStatusCode.Unauthorized, "User not found");
            var userId = user.Id;

            return await habitsRepository.DeleteAllProgressAsync(userId, cancellationToken);


        }
    }
}
