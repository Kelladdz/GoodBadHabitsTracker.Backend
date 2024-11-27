using GoodBadHabitsTracker.Application.DTOs.Auth.Request;
using GoodBadHabitsTracker.Application.DTOs.Auth.Response;
using GoodBadHabitsTracker.Application.Abstractions.Messaging;

namespace GoodBadHabitsTracker.Application.Commands.Auth.ExternalLogin
{
    public record ExternalLoginCommand(ExternalLoginRequest Request) : ICommand<LoginResponse>;
}
