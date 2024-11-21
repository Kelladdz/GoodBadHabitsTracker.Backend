using Azure.Core;
using GoodBadHabitsTracker.Application.Commands.Auth.Login;
using GoodBadHabitsTracker.Application.DTOs.Auth.Response;
using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Core.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.Commands.Auth.RefreshToken
{
    internal sealed class RefreshTokenCommandHandler
        (UserManager<User> userManager,
        IAccessTokenHandler accessTokenHandler,
        IRefreshTokenHandler refreshTokenHandler) : IRequestHandler<RefreshTokenCommand, LoginResponse>
    {
        public async Task<LoginResponse> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
        {
            var accessToken = command.Request.AccessToken;
            var refreshToken = command.Request.RefreshToken;

            var principal = accessTokenHandler.GetPrincipalFromExpiredToken(command.Request.AccessToken);
            var userId = principal.Claims.FirstOrDefault(claim => claim.Type == "sub")?.Value
                ?? throw new AppException(System.Net.HttpStatusCode.Unauthorized, "Invalid access token");

            var user = await userManager.FindByIdAsync(userId)
                ?? throw new AppException(System.Net.HttpStatusCode.Unauthorized, "Can't find user with this id.");

            if (user.RefreshToken != refreshToken ||
                    user.RefreshTokenExpirationDate <= DateTime.UtcNow) throw new UnauthorizedAccessException("Refresh Token is invalid.");

            var userRoles = await userManager.GetRolesAsync(user);
            if (userRoles.Count == 0)
            {
                var result = await userManager.AddToRoleAsync(user, "User");
                if (!result.Succeeded) throw new InvalidOperationException("User role cannot be added.");
            }
            var userSession = new UserSession(user.Id, user.UserName, user.Email, userRoles);

            var newAccessToken = accessTokenHandler.GenerateAccessToken(userSession, out string userFingerprint)
                ?? throw new InvalidOperationException("New access token cannot be null.");

            var newRefreshToken = refreshTokenHandler.GenerateRefreshToken()
                ?? throw new InvalidOperationException("New refresh token cannot be null.");

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpirationDate = DateTime.UtcNow.AddDays(7);
            var userUpdateResult = await userManager.UpdateAsync(user);
            if (!userUpdateResult.Succeeded) throw new InvalidOperationException("User update failed.");

            return new LoginResponse(newAccessToken, newRefreshToken, userFingerprint);
        }
    }
}
