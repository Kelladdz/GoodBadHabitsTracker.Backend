﻿using MediatR;
using Microsoft.AspNetCore.Identity;
using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Application.DTOs.Response;

namespace GoodBadHabitsTracker.Application.Commands.Auth.Login
{
    internal sealed class LoginCommandHandler
        (UserManager<User> userManager,
        IJwtTokenHandler tokenHandler) : IRequestHandler<LoginCommand, LoginResponse>
    {
        public async Task<LoginResponse> Handle(LoginCommand command, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByEmailAsync(command.Request.Email) 
                ?? throw new AppException(System.Net.HttpStatusCode.Unauthorized, "Invalid email or password");

            var checkPasswordResult = await userManager.CheckPasswordAsync(user, command.Request.Password);
            if (!checkPasswordResult) throw new AppException(System.Net.HttpStatusCode.Unauthorized, "Invalid email or password");

            var userRoles = await userManager.GetRolesAsync(user);
            if (userRoles.Count == 0)
            {
                var addToRoleResult = await userManager.AddToRoleAsync(user, "User");
                if (!addToRoleResult.Succeeded)
                    throw new AppException(System.Net.HttpStatusCode.BadRequest, "Failed to add user to role: " + string.Join(", ", addToRoleResult.Errors.Select(e => e.Description)));
            }

            var userSession = new UserSession(user.Id, user.UserName!, user.Email, userRoles);

            var accessToken = tokenHandler.GenerateAccessToken(userSession, out string userFingerprint)
                ?? throw new AppException(System.Net.HttpStatusCode.Unauthorized, "Something goes wrong. Try again.");

            var refreshToken = tokenHandler.GenerateRefreshToken(userSession)
                ?? throw new AppException(System.Net.HttpStatusCode.Unauthorized, "Something goes wrong. Try again.");

            return new LoginResponse(accessToken, refreshToken, userFingerprint);
        }
    }
}
