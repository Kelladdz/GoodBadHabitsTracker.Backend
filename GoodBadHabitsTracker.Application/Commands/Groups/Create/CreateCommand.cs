using GoodBadHabitsTracker.Application.Abstractions.Messaging;
using GoodBadHabitsTracker.Application.DTOs.Response;
using GoodBadHabitsTracker.Application.DTOs.Request;

namespace GoodBadHabitsTracker.Application.Commands.Groups.Create
{
    public sealed record CreateCommand(GroupRequest Request) : ICommand<GroupResponse?>;
}
