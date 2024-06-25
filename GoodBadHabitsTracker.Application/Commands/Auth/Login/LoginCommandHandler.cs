using GoodBadHabitsTracker.Application.DTOs.Auth.Response;
using GoodBadHabitsTracker.Core.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodBadHabitsTracker.Application.Exceptions;
using Azure.Core;
using System.Security.Authentication;
using Azure;
using Microsoft.AspNetCore.Http;
using GoodBadHabitsTracker.Core.Interfaces;

namespace GoodBadHabitsTracker.Application.Commands.Auth.Login
{
    public sealed class LoginCommandHandler
        (UserManager<User> userManager,
        IAccessTokenHandler accessTokenHandler,
        IRefreshTokenHandler refreshTokenHandler) : IRequestHandler<LoginCommand, LoginResponse>
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

            var accessToken = accessTokenHandler.GenerateAccessToken(userSession, out string userFingerprint);
            if (accessToken is null) throw new AppException(System.Net.HttpStatusCode.Unauthorized, "Something goes wrong. Try again.");

            var refreshToken = refreshTokenHandler.GenerateRefreshToken();
            if (refreshToken is null) throw new AppException(System.Net.HttpStatusCode.Unauthorized, "Something goes wrong. Try again.");

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpirationDate = DateTime.UtcNow.AddDays(7);

            var userUpdateResult = await userManager.UpdateAsync(user);
            if (!userUpdateResult.Succeeded) throw new AppException(System.Net.HttpStatusCode.Unauthorized, "Something goes wrong. Try again.");

            return new LoginResponse(accessToken, refreshToken, userFingerprint);
        }
    }
}
