using MediatR;
using GoodBadHabitsTracker.Application.DTOs.Auth.Response;
using Azure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.Application.DTOs.Auth.Request;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Text.Json;
using AutoMapper;

namespace GoodBadHabitsTracker.Application.Commands.Auth.ExternalLogin
{
    internal sealed class ExternalLoginCommandHandler(
        IMapper mapper,
        IIdTokenHandler idTokenHandler,
        ITokenHandler tokenHandler,
        SignInManager<User> signInManager,
        UserManager<User> userManager) : IRequestHandler<ExternalLoginCommand, bool>
    {
        public async Task<bool> Handle(ExternalLoginCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request
                ?? throw new HttpRequestException("Request cannot be null.");
            if (request.Provider is null || (request.Provider != "Google" && request.Provider != "Facebook")) throw new HttpRequestException("Provider is not correct.");

            var idToken = request.IdToken
                ?? throw new HttpRequestException("Id token cannot be empty.");

            var accessToken = request.AccessToken
                ?? throw new HttpRequestException("Access token cannot be empty.");

            var refreshToken = request.RefreshToken;
            if (refreshToken is null && request.Provider == "Google") throw new HttpRequestException("Refresh token cannot be empty.");

            var claimsPrincipal = idTokenHandler.GetClaimsPrincipalFromIdToken(idToken)
                ?? throw new InvalidOperationException("Claims principal cannot be null.");

            var providerKey = claimsPrincipal.FindFirst(claim => claim.Type == "sub")?.Value
                ?? throw new InvalidOperationException("Provider key cannot be null.");

            var userInfo = new ExternalLoginInfo(claimsPrincipal, request.Provider, providerKey, request.Provider)
                ?? throw new ArgumentNullException("User info cannot be null.");

            userInfo.AuthenticationTokens = new List<AuthenticationToken>()
                {
                    new AuthenticationToken(){ Name = "access_token", Value = accessToken},
                    new AuthenticationToken(){ Name = "id_token", Value = idToken},
                    new AuthenticationToken(){ Name = "scope", Value = request.Scope!},
                    new AuthenticationToken(){ Name = "expires_in", Value = request.ExpiresIn.ToString()},
                    new AuthenticationToken(){ Name = "token_type", Value = request.TokenType!}
                };

            if (refreshToken is not null) userInfo.AuthenticationTokens.Append(new AuthenticationToken() { Name = "refresh_token", Value = refreshToken });

            var loginResult = await signInManager.ExternalLoginSignInAsync(request.Provider, providerKey, isPersistent: false, bypassTwoFactor: true);
            if (loginResult.Succeeded)
            {
                var user = await userManager.FindByLoginAsync(request.Provider, providerKey)
                    ?? throw new ArgumentNullException("User cannot be null.");

                var getUserRole = await userManager.GetRolesAsync(user);
                if (getUserRole.Count == 0)
                {
                    var addRoleResult = await userManager.AddToRoleAsync(user, "User");
                    if (!addRoleResult.Succeeded) throw new InvalidOperationException("User role cannot be added.");
                }

                var updateTokensResult = await signInManager.UpdateExternalAuthenticationTokensAsync(userInfo);
                if (!updateTokensResult.Succeeded) throw new InvalidOperationException("User tokens cannot be updated.");

                return true;
            }
            else
            {
                var email = claimsPrincipal.FindFirst(claim => claim.Type == "email")?.Value;
                if (email != null)
                {
                    var user = await userManager.FindByEmailAsync(email);
                    if (user == null)
                    {
                        user = new User()
                        {
                            UserName = claimsPrincipal.FindFirst(claim => claim.Type == "name")!.Value,
                            Email = claimsPrincipal.FindFirst(claim => claim.Type == "email")!.Value,
                            SecurityStamp = Guid.NewGuid().ToString(),
                            ImageUrl = claimsPrincipal.FindFirst(claim => claim.Type == "picture")!.Value
                        };

                        var createResult = await userManager.CreateAsync(user);
                        if (!createResult.Succeeded) throw new InvalidOperationException("User cannot be created.");

                        var addClaimResult = await userManager.AddClaimAsync(user, new Claim("loginProvider", request.Provider));
                        if (!addClaimResult.Succeeded) throw new InvalidOperationException("User claim cannot be added.");
                    }
                    var addLoginResult = await userManager.AddLoginAsync(user, userInfo);
                    if (!addLoginResult.Succeeded) throw new InvalidOperationException("User login cannot be added.");

                    var getUserRole = await userManager.GetRolesAsync(user);
                    if (getUserRole.Count == 0)
                    {
                        var addRoleResult = await userManager.AddToRoleAsync(user, "User");
                        if (!addRoleResult.Succeeded) throw new InvalidOperationException("User role cannot be added.");
                    }

                    loginResult = await signInManager.ExternalLoginSignInAsync(request.Provider, providerKey, isPersistent: false, bypassTwoFactor: true);
                    if (!loginResult.Succeeded) throw new InvalidOperationException("User cannot be signed in.");

                    var updateTokensResult = await signInManager.UpdateExternalAuthenticationTokensAsync(userInfo);
                    if (!updateTokensResult.Succeeded) throw new InvalidOperationException("User tokens cannot be updated.");

                    return true;
                }
                else throw new InvalidOperationException($"Email claim not received from {userInfo.LoginProvider}");
            }
        }
    }
}
