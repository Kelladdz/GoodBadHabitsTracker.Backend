using GoodBadHabitsTracker.Application.Abstractions.Messaging;
using GoodBadHabitsTracker.Application.DTOs.Request;
using GoodBadHabitsTracker.Application.DTOs.Response;


namespace GoodBadHabitsTracker.Application.Commands.Auth.Register
{
    public sealed record RegisterCommand(RegisterRequest Request) : ICommand<RegisterResponse>;
}
