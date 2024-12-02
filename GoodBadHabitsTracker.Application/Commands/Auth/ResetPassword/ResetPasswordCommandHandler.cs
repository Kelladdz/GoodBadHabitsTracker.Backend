using GoodBadHabitsTracker.Core.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace GoodBadHabitsTracker.Application.Commands.Auth.ResetPassword
{
    internal class ResetPasswordCommandHandler(
        UserManager<User> userManager) : IRequestHandler<ResetPasswordCommand, bool>
    {
        public async Task<bool> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
        {
            var email = command.Request.Email;
            var password = command.Request.Password;
            var token = command.Request.Token;
            token = token.Replace("%2B", "+").Replace("%2F", "/");

            var user = await userManager.FindByEmailAsync(email);
            if (user == null) return false;
            IdentityResult result = await userManager.ResetPasswordAsync(user, token, password);

            return result.Succeeded;
        }
    }
}
