using MediatR;
using GoodBadHabitsTracker.Application.DTOs.Response;
using GoodBadHabitsTracker.Core.Interfaces;
using LanguageExt.Common;
using GoodBadHabitsTracker.Application.Exceptions;
using System.Net;

namespace GoodBadHabitsTracker.Application.Queries.Groups.ReadById
{
    internal sealed class ReadGroupByIdQueryHandler(IGroupsRepository groupsRepository) : IRequestHandler<ReadGroupByIdQuery, Result<GroupResponse>>
    {
        public async Task<Result<GroupResponse>> Handle(ReadGroupByIdQuery query, CancellationToken cancellationToken)
        {
            var groupId = query.Id;

            var group = await groupsRepository.ReadByIdAsync(groupId, cancellationToken);
            if (group is null)
                return new Result<GroupResponse>(new AppException(HttpStatusCode.NotFound, "Group not found"));


            return new Result<GroupResponse>(new GroupResponse(group));
        }
    }
}
