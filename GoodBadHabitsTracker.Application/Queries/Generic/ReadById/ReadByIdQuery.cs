using GoodBadHabitsTracker.Application.Abstractions.Messaging;
using GoodBadHabitsTracker.Application.DTOs.Response;

namespace GoodBadHabitsTracker.Application.Queries.Generic.ReadById
{
    public sealed record ReadByIdQuery<TEntity>(Guid Id) : ICommand<GenericResponse<TEntity>> where TEntity : class;
}
