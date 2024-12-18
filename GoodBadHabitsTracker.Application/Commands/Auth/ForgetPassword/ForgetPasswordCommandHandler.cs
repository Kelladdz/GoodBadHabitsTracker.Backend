using GoodBadHabitsTracker.Application.Commands.Auth.Register;
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

namespace GoodBadHabitsTracker.Application.Commands.Auth.ForgetPassword
{
    internal sealed class ForgetPasswordCommandHandler(
        UserManager<User> userManager) : IRequestHandler<ForgetPasswordCommand, ForgetPasswordResponse>
    {
        public async Task<ForgetPasswordResponse> Handle(ForgetPasswordCommand command, CancellationToken cancellationToken)
        {
            var email = command.Request.Email;

            var user = await userManager.FindByEmailAsync(email)
                ?? throw new ValidationException([new ValidationError("Email", "User with this email not found")]);

            var token = await userManager.GeneratePasswordResetTokenAsync(user)
                ?? throw new AppException(System.Net.HttpStatusCode.BadRequest, "Failed to generate password reset token");

            return new ForgetPasswordResponse(user, token);
        }
    }
}
