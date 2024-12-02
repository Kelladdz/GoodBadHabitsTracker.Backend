using GoodBadHabitsTracker.Application.Abstractions.Messaging;
using GoodBadHabitsTracker.Application.DTOs.Response;

namespace GoodBadHabitsTracker.Application.Queries.Generic.Search
{
    public sealed record SearchQuery<TEntity>(string? Term, DateOnly Date) : ICommand<IEnumerable<GenericResponse<TEntity>>>
        where TEntity : class;

}
