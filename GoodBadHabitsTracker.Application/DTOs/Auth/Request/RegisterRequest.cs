﻿namespace GoodBadHabitsTracker.Application.DTOs.Auth.Request
{
    public sealed class RegisterRequest
    {
        public string Email { get; init; }
        public string UserName { get; init; }
        public string Password { get; init; }
        public string ConfirmPassword { get; init; }
    }
}
