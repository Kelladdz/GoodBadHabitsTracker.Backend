using GoodBadHabitsTracker.Core.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.Commands.Habits.Update
{
    internal sealed class UpdateCommandHandler(IHabitsRepository habitsRepository) : IRequestHandler<UpdateCommand, bool>
    {
        public async Task<bool> Handle(UpdateCommand command, CancellationToken cancellationToken)
        {
            var habitId = command.Id;
            var document = command.Request;

            return await habitsRepository.UpdateAsync(document, habitId, cancellationToken);
        }
    }
}
