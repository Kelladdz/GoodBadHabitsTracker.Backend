using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Core.Interfaces;
using MediatR;
using LanguageExt.Common;

namespace GoodBadHabitsTracker.Application.Commands.Groups.Delete
{
    internal sealed class DeleteGroupCommandHandler(IGroupsRepository groupsRepository) : IRequestHandler<DeleteGroupCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(DeleteGroupCommand command, CancellationToken cancellationToken)
        {
            var groupId = command.Id;
            var groupToDelete = await groupsRepository.FindAsync(groupId, cancellationToken);
            if (groupToDelete == null)
                return new Result<bool>(new AppException(System.Net.HttpStatusCode.NotFound, "Group Not Found"));
            
            await groupsRepository.DeleteAsync(groupToDelete, cancellationToken);
            return new Result<bool>(true);
        }
    }
}
