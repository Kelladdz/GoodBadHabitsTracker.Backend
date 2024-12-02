using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.DTOs.Request
{
    public sealed class RefreshTokenRequest
    {
        public string RefreshToken { get; init; } = default!;
    }
}
