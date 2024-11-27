using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.DTOs.Auth.Request
{
    public record ExternalLoginRequest
    {
        public string? AccessToken { get; init; }
        public int ExpiresIn { get; init; }
        public string? Scope { get; init; }
        public string? IdToken { get; init; }
        public string? RefreshToken { get; init; }
        public string? TokenType { get; init; }
        public string? Provider { get; init; }
    }
}
