using GoodBadHabitsTracker.Core.Interfaces;
using MediatR;

namespace GoodBadHabitsTracker.Application.Commands.Habits.Delete
{
    internal sealed class DeleteCommandHandler(IHabitsRepository habitsRepository) : IRequestHandler<DeleteCommand, bool>
    {
        public async Task<bool> Handle(DeleteCommand command, CancellationToken cancellationToken)
        {
            var habitId = command.Id;

            return await habitsRepository.DeleteAsync(habitId, cancellationToken);
        }
    }
}
