using GoodBadHabitsTracker.Application.DTOs.Generic.Response;
using GoodBadHabitsTracker.Application.Abstractions.Messaging;

namespace GoodBadHabitsTracker.Application.Queries.Generic.Search
{
    public sealed record SearchQuery<TEntity>(string? Term, DateOnly Date) : ICommand<IEnumerable<GenericResponse<TEntity>>>
        where TEntity : class;

}
