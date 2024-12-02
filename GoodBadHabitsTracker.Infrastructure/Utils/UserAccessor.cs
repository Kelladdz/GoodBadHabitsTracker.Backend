using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Infrastructure.Utils
{
    public class UserAccessor(IHttpContextAccessor httpContextAccessor, ITokenHandler tokenHandler, UserManager<User> userManager) : IUserAccessor
    {
        public async Task<User?> GetCurrentUser()
        {
            var accessToken = httpContextAccessor.HttpContext!.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            var sub = tokenHandler.GetClaimsFromToken(accessToken).FirstOrDefault(claim => claim.Type == "sub")!.Value;
            if (sub == null)
                return null;

            User user;
            if (Guid.TryParse(sub, out Guid _))
            {
                user = await userManager.FindByIdAsync(sub);
                if (user == null)
                    return null;
            }
            else
            {
                var provider = sub.Contains("google") ? "Google" : "Facebook";
                user = await userManager.FindByLoginAsync(provider, sub);
                if (user == null)
                    return null;
            }
            return user;
        }
    }
}
