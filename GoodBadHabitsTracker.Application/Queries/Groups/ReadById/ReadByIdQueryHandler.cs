using MediatR;
using GoodBadHabitsTracker.Application.DTOs.Response;
using GoodBadHabitsTracker.Core.Interfaces;

namespace GoodBadHabitsTracker.Application.Queries.Groups.ReadById
{
    internal sealed class ReadByIdQueryHandler(IGroupsRepository groupsRepository) : IRequestHandler<ReadByIdQuery, GroupResponse?>
    {
        public async Task<GroupResponse?> Handle(ReadByIdQuery query, CancellationToken cancellationToken)
        {
            var groupId = query.Id;

            var group = await groupsRepository.ReadByIdAsync(groupId, cancellationToken);
            if (group is null)
                return null;

            return new GroupResponse(group);
        }
    }
}
