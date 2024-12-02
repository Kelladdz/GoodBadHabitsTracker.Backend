using GoodBadHabitsTracker.Application.Abstractions.Messaging;
using GoodBadHabitsTracker.Application.DTOs.Response;

namespace GoodBadHabitsTracker.Application.Queries.Groups.ReadAll
{
    public sealed record ReadAllQuery() : IQuery<IEnumerable<GroupResponse>>;
}
