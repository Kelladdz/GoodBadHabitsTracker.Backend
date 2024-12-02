using GoodBadHabitsTracker.Application.Abstractions.Messaging;
using GoodBadHabitsTracker.Application.DTOs.Response;

namespace GoodBadHabitsTracker.Application.Queries.Groups.ReadById
{
    public sealed record ReadByIdQuery(Guid Id) : IQuery<GroupResponse?>;
}
