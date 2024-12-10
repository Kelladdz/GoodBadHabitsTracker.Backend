using GoodBadHabitsTracker.Application.Abstractions.Messaging;
using GoodBadHabitsTracker.Application.DTOs.Request;
using Microsoft.AspNetCore.Identity;

namespace GoodBadHabitsTracker.Application.Commands.Auth.ConfirmEmail
{
    public sealed record ConfirmEmailCommand(ConfirmEmailRequest Request) : ICommand<IdentityResult>;
}
