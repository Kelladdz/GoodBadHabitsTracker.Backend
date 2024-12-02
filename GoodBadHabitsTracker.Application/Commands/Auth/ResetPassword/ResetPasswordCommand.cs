using GoodBadHabitsTracker.Application.Abstractions.Messaging;
using GoodBadHabitsTracker.Application.DTOs.Request;

namespace GoodBadHabitsTracker.Application.Commands.Auth.ResetPassword
{
    public sealed record ResetPasswordCommand(ResetPasswordRequest Request) : ICommand<bool>;
}
