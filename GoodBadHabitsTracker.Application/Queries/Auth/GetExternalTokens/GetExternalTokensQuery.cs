using GoodBadHabitsTracker.Application.Abstractions.Messaging;
using GoodBadHabitsTracker.Application.DTOs.Request;
using GoodBadHabitsTracker.Application.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.Queries.Auth.GetExternalTokens
{
    public sealed record GetExternalTokensQuery(GetExternalTokensRequest Request, string Provider) : ICommand<GetExternalTokensResponse>;
}
