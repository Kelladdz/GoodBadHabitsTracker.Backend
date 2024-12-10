using GoodBadHabitsTracker.Application.DTOs.Response;
using GoodBadHabitsTracker.Application.Abstractions.Messaging;
using GoodBadHabitsTracker.Application.DTOs.Request;
using Microsoft.AspNetCore.Identity;

namespace GoodBadHabitsTracker.Application.Commands.Auth.ExternalLogin
{
    public record ExternalLoginCommand(ExternalLoginRequest Request) : ICommand<IdentityResult>;
}
