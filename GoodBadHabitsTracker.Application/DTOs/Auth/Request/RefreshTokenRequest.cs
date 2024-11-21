using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.DTOs.Auth.Request
{
    public sealed class RefreshTokenRequest
    {
        public string AccessToken { get; init; } = default!;
        public string RefreshToken { get; init; } = default!;
    }
}
