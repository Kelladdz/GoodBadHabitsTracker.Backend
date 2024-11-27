﻿namespace GoodBadHabitsTracker.Application.DTOs.Auth.Request
{
    public sealed class LoginRequest
    {
        public string Email { get; init; } = default!;
        public string Password { get; init; } = default!;
    }
}
