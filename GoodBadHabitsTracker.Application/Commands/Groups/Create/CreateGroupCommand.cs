using GoodBadHabitsTracker.Application.Abstractions.Messaging;
using GoodBadHabitsTracker.Application.DTOs.Response;
using GoodBadHabitsTracker.Application.DTOs.Request;
using LanguageExt.Common;

namespace GoodBadHabitsTracker.Application.Commands.Groups.Create
{
    public sealed record CreateGroupCommand(GroupRequest Request) : ICommand<Result<GroupResponse>>;
}
