using GoodBadHabitsTracker.Application.Abstractions.Messaging;
using GoodBadHabitsTracker.Application.DTOs.Response;
using LanguageExt.Common;

namespace GoodBadHabitsTracker.Application.Queries.Groups.ReadAll
{
    public sealed record ReadAllGroupsQuery() : IQuery<Result<IEnumerable<GroupResponse>>>;
}
