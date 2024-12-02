using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Application.DTOs.Response;
using GoodBadHabitsTracker.Application.Exceptions;
using MediatR;

namespace GoodBadHabitsTracker.Application.Queries.Groups.ReadAll
{
    internal sealed class ReadAllQueryHandler(IGroupsRepository groupsRepository, IUserAccessor userAccessor) : IRequestHandler<ReadAllQuery, IEnumerable<GroupResponse>>
    {
        public async Task<IEnumerable<GroupResponse>> Handle(ReadAllQuery query, CancellationToken cancellationToken)
        {
            var user = await userAccessor.GetCurrentUser()
                ?? throw new AppException(System.Net.HttpStatusCode.Unauthorized, "User not found");

            var userId = user.Id;
            var groups = await groupsRepository.ReadAllAsync(userId, cancellationToken);
            if (groups is null)
                return [];

            var response = new List<GroupResponse>();
            foreach (var group in groups)
            {
                response.Add(new GroupResponse(group));
            }

            return response;
        }
    }
}
