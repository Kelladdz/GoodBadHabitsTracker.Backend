using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Core.Interfaces;
using MediatR;

namespace GoodBadHabitsTracker.Application.Commands.Habits.DeleteAll
{
    internal sealed class DeleteAllCommandHandler(
        IHabitsRepository habitsRepository,
        IUserAccessor userAccessor) : IRequestHandler<DeleteAllCommand, bool>
    {
        public async Task<bool> Handle(DeleteAllCommand command, CancellationToken cancellationToken)
        {
            var user = await userAccessor.GetCurrentUser()
                ?? throw new AppException(System.Net.HttpStatusCode.Unauthorized, "User not found");
            var userId = user.Id;

            return await habitsRepository.DeleteAllProgressAsync(userId, cancellationToken);
        }
    }
}
