using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.Infrastructure.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;

namespace GoodBadHabitsTracker.Application.Services
{
    public class UserAccessor(
        IHttpContextAccessor httpContextAccessor,
        IJwtTokenHandler tokenHandler,
        UserManager<User> userManager) : IUserAccessor
    {
        public async Task<User?> GetCurrentUser()
        {
            var accessToken = httpContextAccessor.HttpContext!.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            var sub = tokenHandler.GetClaimsFromToken(accessToken).FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Sub)!.Value;

            User? user;
            if (Guid.TryParse(sub, out Guid _))
                return await userManager.FindByIdAsync(sub);

            else
            {
                var provider = sub.Contains("google") ? ExternalTokenProviders.GOOGLE : ExternalTokenProviders.FACEBOOK;
                return await userManager.FindByLoginAsync(provider, sub);
            }
        }
    }
}
