﻿using GoodBadHabitsTracker.Application.Abstractions.Messaging;
using GoodBadHabitsTracker.Application.DTOs.Request;
using Microsoft.AspNetCore.Identity;

namespace GoodBadHabitsTracker.Application.Commands.Auth.ResetPassword
{
    public sealed record ResetPasswordCommand(ResetPasswordRequest Request) : ICommand<IdentityResult>;
}
