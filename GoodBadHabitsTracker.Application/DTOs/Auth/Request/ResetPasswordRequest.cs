using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.DTOs.Auth.Request
{
    public class ResetPasswordRequest
    {
        public string Password { get; init; } = default!;
        public string Token { get; init; } = default!;
        public string Email { get; init; } = default!;
    }
}
