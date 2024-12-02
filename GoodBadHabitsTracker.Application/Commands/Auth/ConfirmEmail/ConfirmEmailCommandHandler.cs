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
        UserManager<User> userManager) : IRequestHandler<ConfirmEmailCommand, bool>
    {
        public async Task<bool> Handle(ConfirmEmailCommand command, CancellationToken cancellationToken)
        {
            var userId = command.Request.UserId.ToString();
            var token = command.Request.Token;
            token = token.Replace("%2B", "+").Replace("%2F", "/");
            var user = await userManager.FindByIdAsync(userId)
                ?? throw new AppException(System.Net.HttpStatusCode.BadRequest, "User with this id does not exist");

            var result = await userManager.ConfirmEmailAsync(user, token)
                ?? throw new AppException(System.Net.HttpStatusCode.BadRequest, "Failed to confirm email");

            return result.Succeeded;
        }
    }
}
