using GoodBadHabitsTracker.Application.Abstractions.Messaging;
using GoodBadHabitsTracker.Application.DTOs.Response;
using LanguageExt.Common;

namespace GoodBadHabitsTracker.Application.Queries.Groups.ReadById
{
    public sealed record ReadGroupByIdQuery(Guid Id) : IQuery<Result<GroupResponse>>;
}
