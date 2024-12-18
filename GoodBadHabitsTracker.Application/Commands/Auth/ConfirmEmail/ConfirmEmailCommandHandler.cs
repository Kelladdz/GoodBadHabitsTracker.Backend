using GoodBadHabitsTracker.Application.Commands.Auth.ForgetPassword;
using GoodBadHabitsTracker.Application.DTOs.Response;
using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Core.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.Commands.Auth.ConfirmEmail
{
    internal sealed class ConfirmEmailCommandHandler(
        UserManager<User> userManager) : IRequestHandler<ConfirmEmailCommand, IdentityResult>
    {
        public async Task<IdentityResult> Handle(ConfirmEmailCommand command, CancellationToken cancellationToken)
        {
                var userId = command.Request.UserId.ToString();
                var token = command.Request.Token;

                var user = await userManager.FindByIdAsync(userId);
                if (user is null)
                    return IdentityResult.Failed(new IdentityError { Code = "UserNotFound", Description = $"User with id {userId} not found." });

                else return await userManager.ConfirmEmailAsync(user, token);
        }
    }
}
