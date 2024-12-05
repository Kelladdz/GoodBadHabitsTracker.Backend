using GoodBadHabitsTracker.Application.Abstractions.Messaging;
namespace GoodBadHabitsTracker.Application.Commands.Auth.DeleteAccount
{
    public sealed record DeleteAccountCommand() : ICommand<bool>;
}
