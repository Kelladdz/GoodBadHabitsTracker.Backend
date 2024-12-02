using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.Utils
{
    public class UserAccessor(IHttpContextAccessor httpContextAccessor, ITokenHandler tokenHandler, UserManager<User> userManager) : IUserAccessor
    {
        public async Task<User> GetCurrentUser()
        {
            var accessToken = httpContextAccessor.HttpContext!.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var sub = tokenHandler.GetClaimsFromToken(accessToken).FirstOrDefault(claim => claim.Type == "sub")!.Value
                ?? throw new AppException(System.Net.HttpStatusCode.Unauthorized, "Invalid access token");

            User user;
            if (Guid.TryParse(sub, out Guid _))
                user = await userManager.FindByIdAsync(sub)
                ?? throw new AppException(System.Net.HttpStatusCode.Unauthorized, "User not found");

            else
            {
                var provider = sub.Contains("google") ? "Google" : "Facebook";
                user = await userManager.FindByLoginAsync(provider, sub)
                    ?? throw new AppException(System.Net.HttpStatusCode.Unauthorized, "User not found");
            }
            return user;
        }
    }
}
