using GoodBadHabitsTracker.Application.DTOs.Auth.Request;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.Application.Abstractions.Messaging;


namespace GoodBadHabitsTracker.Application.Commands.Auth.Register
{
    public sealed record RegisterCommand(RegisterRequest Request, CancellationToken CancellationToken) : ICommand<User>;
}
