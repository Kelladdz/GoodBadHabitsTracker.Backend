using MediatR;
using AutoMapper;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Application.DTOs.Response;
using GoodBadHabitsTracker.Application.Exceptions;

namespace GoodBadHabitsTracker.Application.Commands.Groups.Create
{
    internal sealed class CreateCommandHandler(IGroupsRepository groupRepository, IMapper mapper, IUserAccessor userAccessor) : IRequestHandler<CreateCommand, GroupResponse?>
    {
        public async Task<GroupResponse?> Handle(CreateCommand command, CancellationToken cancellationToken)
        {
            var user = await userAccessor.GetCurrentUser()
                ?? throw new AppException(System.Net.HttpStatusCode.Unauthorized, "User not found");
            var userId = user.Id;

            var request = command.Request;
            var groupToInsert = mapper.Map<Group>(request);

            var newGroup = await groupRepository.InsertAsync(groupToInsert, userId, cancellationToken);

            return newGroup is not null ? new GroupResponse(newGroup) : null;
        }
    }
}
