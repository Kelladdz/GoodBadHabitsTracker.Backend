using Microsoft.AspNetCore.JsonPatch;
using GoodBadHabitsTracker.Application.Abstractions.Messaging;

namespace GoodBadHabitsTracker.Application.Commands.Generic.Update
{
    public sealed record UpdateCommand<TEntity>(Guid Id, JsonPatchDocument<TEntity> Request) : ICommand<bool>
        where TEntity : class;
}
