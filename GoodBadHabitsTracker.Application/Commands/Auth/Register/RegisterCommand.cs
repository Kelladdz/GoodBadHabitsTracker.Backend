using GoodBadHabitsTracker.Application.DTOs.Auth.Request;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.Application.Abstractions.Messaging;
using GoodBadHabitsTracker.Application.DTOs.Auth.Response;


namespace GoodBadHabitsTracker.Application.Commands.Auth.Register
{
    public sealed record RegisterCommand(RegisterRequest Request) : ICommand<RegisterResponse>;
}
