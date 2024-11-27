using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.DTOs.Auth.Response
{
    public record GetExternalTokensResponse
    {
        public string? AccessToken { get; set; }
        public int ExpiresIn { get; set; }
        public string? Scope { get; set; }
        public string? IdToken { get; set; }
        public string? RefreshToken { get; set; }
        public string? TokenType { get; set; }
        public string? Provider { get; set; }
    }
}
