using GoodBadHabitsTracker.Core.Interfaces;
using MediatR;

namespace GoodBadHabitsTracker.Application.Commands.Groups.Update
{
    internal sealed class UpdateCommandHandler(IGroupsRepository groupsRepository) : IRequestHandler<UpdateCommand, bool>
    {
        public async Task<bool> Handle(UpdateCommand command, CancellationToken cancellationToken)
        {
            var habitId = command.Id;
            var document = command.Request;

            return await groupsRepository.UpdateAsync(document, habitId, cancellationToken);
        }
    }
}
