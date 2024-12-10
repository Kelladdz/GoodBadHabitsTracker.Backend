using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Core.Interfaces;
using LanguageExt.Common;
using MediatR;
using System.Net;

namespace GoodBadHabitsTracker.Application.Commands.Groups.Update
{
    internal sealed class UpdateGroupCommandHandler(IGroupsRepository groupsRepository) : IRequestHandler<UpdateGroupCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(UpdateGroupCommand command, CancellationToken cancellationToken)
        {
            var groupId = command.Id;
            var document = command.Request;

            var groupToUpdate = await groupsRepository.FindAsync(groupId, cancellationToken);
            if (groupToUpdate == null)
                return new Result<bool>(new AppException(HttpStatusCode.NotFound, "Group Not Found"));
            
            await groupsRepository.UpdateAsync(document, groupToUpdate, cancellationToken);
            return new Result<bool>(true);
        }
    }
}
