using GoodBadHabitsTracker.Application.Abstractions.Messaging;

namespace GoodBadHabitsTracker.Application.Commands.Generic.Delete
{
    public sealed record DeleteCommand<TEntity>(Guid Id) : ICommand<bool> 
        where TEntity : class;
}
