﻿namespace GoodBadHabitsTracker.Application.DTOs.Auth.Request
{
    public sealed class LoginRequest
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
    }
}
