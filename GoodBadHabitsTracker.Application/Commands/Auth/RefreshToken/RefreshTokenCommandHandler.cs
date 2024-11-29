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
        ITokenHandler tokenHandler) : IRequestHandler<RefreshTokenCommand, LoginResponse>
    {
        public async Task<LoginResponse> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
        {
            var accessToken = command.Request.AccessToken;
            var refreshToken = command.Request.RefreshToken;

            var accessTokenPrincipal = tokenHandler.ValidateAndGetPrincipalFromToken(accessToken)
                ?? throw new AppException(System.Net.HttpStatusCode.Unauthorized, "Invalid access token");
            var refreshTokenPrincipal = tokenHandler.ValidateAndGetPrincipalFromToken(refreshToken)
                ?? throw new AppException(System.Net.HttpStatusCode.Unauthorized, "Invalid refresh token");

            var accessTokenPrincipalUserId = accessTokenPrincipal.Claims.FirstOrDefault(claim => claim.Type == "sub")?.Value
                ?? throw new AppException(System.Net.HttpStatusCode.Unauthorized, "Invalid access token");
            var user = await userManager.FindByIdAsync(accessTokenPrincipalUserId)
                ?? throw new AppException(System.Net.HttpStatusCode.Unauthorized, "Invalid access token");

            var refreshTokenPrincipalUserId = refreshTokenPrincipal.Claims.FirstOrDefault(claim => claim.Type == "sub")?.Value
                ?? throw new AppException(System.Net.HttpStatusCode.Unauthorized, "Invalid refresh token");
            var refreshTokenExpiry = refreshTokenPrincipal.Claims.FirstOrDefault(claim => claim.Type == "sub")?.Value;

            if (accessTokenPrincipalUserId is null || refreshTokenPrincipal is null 
                || accessTokenPrincipalUserId != refreshTokenPrincipalUserId) 
                throw new UnauthorizedAccessException("Refresh Token is invalid");

            var userRoles = await userManager.GetRolesAsync(user);
            if (userRoles.Count == 0)
            {
                var result = await userManager.AddToRoleAsync(user, "User");
                if (!result.Succeeded) throw new InvalidOperationException("User role cannot be added.");
            }
            var userSession = new UserSession(user.Id, user.UserName, user.Email, userRoles);

            var newAccessToken = tokenHandler.GenerateAccessToken(userSession, out string userFingerprint)
                ?? throw new InvalidOperationException("New access token cannot be null.");

            var newRefreshToken = tokenHandler.GenerateRefreshToken(userSession)
                ?? throw new InvalidOperationException("New refresh token cannot be null.");

            return new LoginResponse(newAccessToken, newRefreshToken, userFingerprint);
        }
    }
}
