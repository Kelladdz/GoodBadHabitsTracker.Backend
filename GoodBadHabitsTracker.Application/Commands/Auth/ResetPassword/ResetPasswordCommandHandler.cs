using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Core.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace GoodBadHabitsTracker.Application.Commands.Auth.ResetPassword
{
    internal sealed class ResetPasswordCommandHandler(
        UserManager<User> userManager) : IRequestHandler<ResetPasswordCommand, IdentityResult>
    {
        public async Task<IdentityResult> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
        {
                var email = command.Request.Email;
                var password = command.Request.Password;
                var token = command.Request.Token;

                var user = await userManager.FindByEmailAsync(email);
                if (user == null)
                    return IdentityResult.Failed(new IdentityError { Code = "UserNotFoundByEmail", Description = "There is no user with this email" });

                return await userManager.ResetPasswordAsync(user, token, password); 
        }
    }
}
