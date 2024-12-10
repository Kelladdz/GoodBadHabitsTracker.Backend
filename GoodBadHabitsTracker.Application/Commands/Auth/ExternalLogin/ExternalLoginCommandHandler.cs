using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Core.Models;
using AutoMapper;
using GoodBadHabitsTracker.Application.Exceptions;

namespace GoodBadHabitsTracker.Application.Commands.Auth.ExternalLogin
{
    internal sealed class ExternalLoginCommandHandler(
        IMapper mapper,
        IIdTokenHandler idTokenHandler,
        ITokenHandler tokenHandler,
        SignInManager<User> signInManager,
        UserManager<User> userManager,
        RoleManager<UserRole> roleManager) : IRequestHandler<ExternalLoginCommand, IdentityResult>
    {
        public async Task<IdentityResult> Handle(ExternalLoginCommand command, CancellationToken cancellationToken)
        {
                var request = command.Request;
                if (request is null)
                    return IdentityResult.Failed(new IdentityError { Code = "NullRequest", Description = "Request cannot be null" });

                if (request.Provider is null || (request.Provider != "Google" && request.Provider != "Facebook")) 
                    return IdentityResult.Failed(new IdentityError { Code = "InvalidProvider", Description = "Provider is not correct" });

                var idToken = request.IdToken;
                if (idToken is null)
                    return IdentityResult.Failed(new IdentityError { Code = "NullIdToken", Description = "IdToken cannot be null" });

                var accessToken = request.AccessToken;
                if (accessToken is null)
                    return IdentityResult.Failed(new IdentityError { Code = "NullAccessToken", Description = "Access token cannot be null" });

                var refreshToken = request.RefreshToken;
                if (refreshToken is null && request.Provider == "Google")
                    return IdentityResult.Failed(new IdentityError { Code = "NullRefreshToken", Description = "Google must return refresh token" });

                var claimsPrincipal = idTokenHandler.GetClaimsPrincipalFromIdToken(idToken);
                if (claimsPrincipal is null)
                    return IdentityResult.Failed(new IdentityError { Code = "NullClaimsPrincipal", Description = "Claims principal cannot be null" });

                var providerKey = claimsPrincipal.FindFirst(claim => claim.Type == "sub")?.Value;
                if (providerKey is null)
                    return IdentityResult.Failed(new IdentityError { Code = "NullProviderKey", Description = "Provider key cannot be null" });


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
                    var user = await userManager.FindByLoginAsync(request.Provider, providerKey);
                    if (user is null)
                        return IdentityResult.Failed(new IdentityError { Code = "UserLoginNotFound", Description = $"User not found" }); ;

                    var getUserRole = await userManager.GetRolesAsync(user);
                    var isRoleExists = await roleManager.RoleExistsAsync("User");
                    if (!isRoleExists)
                    {
                        var role = new UserRole { Id = Guid.NewGuid(), Name = "User", NormalizedName = "USER", ConcurrencyStamp = Guid.NewGuid().ToString() };

                        var createRoleResult = await roleManager.CreateAsync(role);
                        if (!createRoleResult.Succeeded)
                            return createRoleResult;
                    }
                    if (getUserRole.Count == 0)
                    {
                        var addRoleResult = await userManager.AddToRoleAsync(user, "User");
                        if (!addRoleResult.Succeeded)
                            return addRoleResult;
                    }

                    return await signInManager.UpdateExternalAuthenticationTokensAsync(userInfo);
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
                                return createResult;

                            var addClaimResult = await userManager.AddClaimAsync(user, new Claim("loginProvider", request.Provider));
                            if (!addClaimResult.Succeeded) 
                                return addClaimResult;
                        }

                        var addLoginResult = await userManager.AddLoginAsync(user, userInfo);
                        if (!addLoginResult.Succeeded) 
                            return addLoginResult;

                        var getUserRole = await userManager.GetRolesAsync(user);
                        if (getUserRole.Count == 0)
                        {
                            var addRoleResult = await userManager.AddToRoleAsync(user, "User");
                            if (!addRoleResult.Succeeded) 
                                return addRoleResult;
                        }

                        loginResult = await signInManager.ExternalLoginSignInAsync(request.Provider, providerKey, isPersistent: false, bypassTwoFactor: true);
                        if (!loginResult.Succeeded)
                            return IdentityResult.Failed(new IdentityError { Code = "ExternalLoginFailed", Description = "External Login failed" });

                        return await signInManager.UpdateExternalAuthenticationTokensAsync(userInfo);
                    }
                    return IdentityResult.Failed(new IdentityError { Code = "NullEmail", Description = "Email cannot be null" });
                }
        }
    }
}
