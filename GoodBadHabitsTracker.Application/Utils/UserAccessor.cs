using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace GoodBadHabitsTracker.Application.Utils
{
    public class UserAccessor(
        IHttpContextAccessor httpContextAccessor, 
        ITokenHandler tokenHandler, 
        UserManager<User> userManager) : IUserAccessor
    {
        public async Task<User?> GetCurrentUser()
        {
            var accessToken = httpContextAccessor.HttpContext!.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            var sub = tokenHandler.GetClaimsFromToken(accessToken).FirstOrDefault(claim => claim.Type == "sub")!.Value;

            User? user;
            if (Guid.TryParse(sub, out Guid _))
                return await userManager.FindByIdAsync(sub);

            else
            {
                var provider = sub.Contains("google") ? "Google" : "Facebook";
                return await userManager.FindByLoginAsync(provider, sub);
            }
        }
    }
}
