using GoodBadHabitsTracker.Application.Abstractions.Messaging;
using GoodBadHabitsTracker.Application.DTOs.Request;
using GoodBadHabitsTracker.Application.DTOs.Response;
using LanguageExt.Common;

namespace GoodBadHabitsTracker.Application.Queries.Auth.GetExternalTokens
{
    public sealed record GetExternalTokensQuery(GetExternalTokensRequest Request, string Provider) : IQuery<Result<GetExternalTokensResponse>>;
}
