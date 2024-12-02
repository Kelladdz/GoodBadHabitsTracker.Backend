using GoodBadHabitsTracker.Core.Interfaces;
using MediatR;

namespace GoodBadHabitsTracker.Application.Commands.Groups.Delete
{
    internal sealed class DeleteCommandHandler(IGroupsRepository groupsRepository) : IRequestHandler<DeleteCommand, bool>
    {
        public async Task<bool> Handle(DeleteCommand command, CancellationToken cancellationToken)
        {
            var habitId = command.Id;

            return await groupsRepository.DeleteAsync(habitId, cancellationToken);
        }
    }
}
