using GoodBadHabitsTracker.Core.Interfaces;
using MediatR;
using LanguageExt.Common;
using GoodBadHabitsTracker.Application.Exceptions;

namespace GoodBadHabitsTracker.Application.Commands.Habits.Delete
{
    internal sealed class DeleteHabitCommandHandler(IHabitsRepository habitsRepository) : IRequestHandler<DeleteHabitCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(DeleteHabitCommand command, CancellationToken cancellationToken)
        {
            var habitId = command.Id;
            var habitToDelete = await habitsRepository.FindAsync(habitId, cancellationToken);
            if (habitToDelete == null)
                return new Result<bool>(new AppException(System.Net.HttpStatusCode.NotFound, "Habit Not Found"));

            await habitsRepository.DeleteAsync(habitToDelete, cancellationToken);
            return new Result<bool>(true);
        }
    }
}
