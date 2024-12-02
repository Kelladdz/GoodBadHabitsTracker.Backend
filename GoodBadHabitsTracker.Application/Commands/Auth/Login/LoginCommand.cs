using GoodBadHabitsTracker.Application.Abstractions.Messaging;
using GoodBadHabitsTracker.Application.DTOs.Request;
using GoodBadHabitsTracker.Application.DTOs.Response;

namespace GoodBadHabitsTracker.Application.Commands.Auth.Login
{
    public sealed record LoginCommand(LoginRequest Request) : ICommand<LoginResponse>;
}
