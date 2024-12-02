using GoodBadHabitsTracker.Application.Abstractions.Messaging;
using GoodBadHabitsTracker.Application.DTOs.Response;

namespace GoodBadHabitsTracker.Application.Queries.Generic.GetAll
{
    public sealed record GetAllQuery<TEntity>() : ICommand<IEnumerable<GenericResponse<TEntity>>> where TEntity : class;
}
