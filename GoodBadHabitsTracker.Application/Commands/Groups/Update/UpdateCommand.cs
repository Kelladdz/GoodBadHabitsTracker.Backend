using GoodBadHabitsTracker.Application.Abstractions.Messaging;
using GoodBadHabitsTracker.Core.Models;
using Microsoft.AspNetCore.JsonPatch;

namespace GoodBadHabitsTracker.Application.Commands.Groups.Update
{
    public sealed record UpdateCommand(Guid Id, JsonPatchDocument<Group> Request) : ICommand<bool>;
}
