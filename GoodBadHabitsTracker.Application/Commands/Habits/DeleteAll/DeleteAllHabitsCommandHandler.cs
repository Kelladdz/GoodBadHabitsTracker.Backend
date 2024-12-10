using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Core.Interfaces;
using MediatR;
using LanguageExt.Common;

namespace GoodBadHabitsTracker.Application.Commands.Habits.DeleteAll
{
    internal sealed class DeleteAllHabitsCommandHandler(
        IHabitsRepository habitsRepository,
        IUserAccessor userAccessor) : IRequestHandler<DeleteAllHabitsCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(DeleteAllHabitsCommand command, CancellationToken cancellationToken)
        {
            var user = await userAccessor.GetCurrentUser();
            if (user == null)
                return new Result<bool>(new AppException(System.Net.HttpStatusCode.BadRequest, "User Not Found"));
            
            var userId = user.Id;

            var allHabits = await habitsRepository.ReadAllAsync(userId, cancellationToken);

            await habitsRepository.DeleteAllAsync(allHabits, cancellationToken);

            return new Result<bool>(true);
        }
    }
}
