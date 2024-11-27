using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.DTOs.Auth.Request
{
    public record GetExternalTokensRequest
    {
        public string? GrantType { get; init; }
        public string? Code { get; init; }
        public string? RedirectUri { get; init; }
        public string? ClientId { get; init; }
        public string? CodeVerifier { get; init; }
    }
}
