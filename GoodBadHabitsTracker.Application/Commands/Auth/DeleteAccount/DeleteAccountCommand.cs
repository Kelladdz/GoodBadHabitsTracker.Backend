using GoodBadHabitsTracker.Application.Abstractions.Messaging;
using Microsoft.AspNetCore.Identity;
namespace GoodBadHabitsTracker.Application.Commands.Auth.DeleteAccount
{
    public sealed record DeleteAccountCommand() : ICommand<IdentityResult>;
}
