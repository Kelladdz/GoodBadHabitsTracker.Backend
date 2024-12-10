using GoodBadHabitsTracker.Application.Abstractions.Messaging;
using GoodBadHabitsTracker.Core.Models;
using Microsoft.AspNetCore.JsonPatch;
using LanguageExt.Common;

namespace GoodBadHabitsTracker.Application.Commands.Groups.Update
{
    public sealed record UpdateGroupCommand(Guid Id, JsonPatchDocument Request) : ICommand<Result<bool>>;
}
