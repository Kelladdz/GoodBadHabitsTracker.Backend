using GoodBadHabitsTracker.Application.DTOs.Auth.Request;
using GoodBadHabitsTracker.Application.DTOs.Auth.Response;
using GoodBadHabitsTracker.Application.Abstractions.Messaging;

namespace GoodBadHabitsTracker.Application.Commands.Auth.Login
{
    public sealed record LoginCommand(LoginRequest Request, CancellationToken CancellationToken) : ICommand<LoginResponse>;
}
