using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Core.Models;
using AutoMapper;
using GoodBadHabitsTracker.Application.DTOs.Response;
using GoodBadHabitsTracker.Application.Exceptions;
using LanguageExt.Common;

namespace GoodBadHabitsTracker.Application.Commands.Auth.ExternalLogin
{
    internal sealed class ExternalLoginCommandHandler(
        IMapper mapper,
        IJwtTokenHandler tokenHandler,
        SignInManager<User> signInManager,
        UserManager<User> userManager,
        RoleManager<UserRole> roleManager) : IRequestHandler<ExternalLoginCommand, LoginResponse>
    {
        public async Task<LoginResponse> Handle(ExternalLoginCommand command, CancellationToken cancellationToken)
        {
                var request = command.Request
                    ?? throw new AppException(System.Net.HttpStatusCode.BadRequest, "Request cannot be null");

                if (request.Provider is null || (request.Provider != "Google" && request.Provider != "Facebook"))
                    throw new AppException(System.Net.HttpStatusCode.BadRequest, "Provider is not correct");

                var idToken = request.IdToken
                    ?? throw new AppException(System.Net.HttpStatusCode.BadRequest,"IdToken cannot be null" );

                var accessToken = request.AccessToken
                    ?? throw new AppException(System.Net.HttpStatusCode.BadRequest,"Access token cannot be null" );

                var refreshToken = request.RefreshToken;
                if (refreshToken is null && request.Provider == "Google")
                    throw new AppException(System.Net.HttpStatusCode.BadRequest, "Google must return refresh token" );

                var claimsPrincipal = tokenHandler.GetClaimsPrincipalFromIdToken(idToken)
                    ?? throw new AppException(System.Net.HttpStatusCode.BadRequest, "Claims principal cannot be null" );

                var providerKey = claimsPrincipal.FindFirst(claim => claim.Type == "sub")?.Value
                    ?? throw new AppException(System.Net.HttpStatusCode.BadRequest, "Provider key cannot be null" );


                var userInfo = new ExternalLoginInfo(claimsPrincipal, request.Provider, providerKey, request.Provider)
                {
                    AuthenticationTokens = new List<AuthenticationToken>()
                    {
                        new AuthenticationToken(){ Name = "access_token", Value = accessToken},
                        new AuthenticationToken(){ Name = "id_token", Value = idToken},
                        new AuthenticationToken(){ Name = "scope", Value = request.Scope!},
                        new AuthenticationToken(){ Name = "expires_in", Value = request.ExpiresIn.ToString()},
                        new AuthenticationToken(){ Name = "token_type", Value = request.TokenType!}
                    }
                };

                if (refreshToken is not null)
                    userInfo.AuthenticationTokens.Append(new AuthenticationToken() { Name = "refresh_token", Value = refreshToken });

                var loginResult = await signInManager.ExternalLoginSignInAsync(request.Provider, providerKey, isPersistent: false, bypassTwoFactor: true);
                if (loginResult.Succeeded)
                {
                    var user = await userManager.FindByLoginAsync(request.Provider, providerKey)
                        ?? throw new AppException(System.Net.HttpStatusCode.BadRequest, $"User not found" ); ;

                    var getUserRole = await userManager.GetRolesAsync(user);
                    var isRoleExists = await roleManager.RoleExistsAsync("User");
                    if (!isRoleExists)
                    {
                        var role = new UserRole { Id = Guid.NewGuid(), Name = "User", NormalizedName = "USER", ConcurrencyStamp = Guid.NewGuid().ToString() };

                        var createRoleResult = await roleManager.CreateAsync(role);
                        if (!createRoleResult.Succeeded)
                            throw new AppException(System.Net.HttpStatusCode.BadRequest, $"Create role failed" );
                    }
                    if (getUserRole.Count == 0)
                    {
                        var addRoleResult = await userManager.AddToRoleAsync(user, "User");
                        if (!addRoleResult.Succeeded)
                            throw new AppException(System.Net.HttpStatusCode.BadRequest, $"Add role failed" );
                    }

                    var updateExternalAuthenticationTokenResult = await signInManager.UpdateExternalAuthenticationTokensAsync(userInfo);
                    return updateExternalAuthenticationTokenResult.Succeeded
                        ? new LoginResponse(accessToken, refreshToken, "_usr_fgp")
                        : throw new AppException(System.Net.HttpStatusCode.BadRequest, $"User not found" );

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
                            if (!createResult.Succeeded)
                                throw new AppException(System.Net.HttpStatusCode.BadRequest, $"Create user failed" );

                            var addClaimResult = await userManager.AddClaimAsync(user, new Claim("loginProvider", request.Provider));
                            if (!addClaimResult.Succeeded)
                                throw new AppException(System.Net.HttpStatusCode.BadRequest, $"Add claim failed" );
                        }

                        var addLoginResult = await userManager.AddLoginAsync(user, userInfo);
                        if (!addLoginResult.Succeeded)
                            throw new AppException(System.Net.HttpStatusCode.BadRequest, $"Add login failed" );

                        var getUserRole = await userManager.GetRolesAsync(user);
                        if (getUserRole.Count == 0)
                        {
                            var addRoleResult = await userManager.AddToRoleAsync(user, "User");
                            if (!addRoleResult.Succeeded)
                                throw new AppException(System.Net.HttpStatusCode.BadRequest, $"Add role failed" );
                        }

                        loginResult = await signInManager.ExternalLoginSignInAsync(request.Provider, providerKey, isPersistent: false, bypassTwoFactor: true);
                        if (!loginResult.Succeeded)
                            throw new AppException(System.Net.HttpStatusCode.BadRequest, "External Login failed" );

                        var updateExternalAuthenticationTokenResult = await signInManager.UpdateExternalAuthenticationTokensAsync(userInfo);
                        return updateExternalAuthenticationTokenResult.Succeeded
                            ? new LoginResponse(accessToken, refreshToken, "_usr_fgp")
                            : throw new AppException(System.Net.HttpStatusCode.BadRequest, $"User not found" );
                    }
                    throw new AppException(System.Net.HttpStatusCode.BadRequest, "Email cannot be null" );
                }
        }
    }
}