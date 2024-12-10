using GoodBadHabitsTracker.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Infrastructure.Security.IdTokenHandler
{
    internal sealed class IdTokenHandler : IIdTokenHandler
    {
        public ClaimsPrincipal GetClaimsPrincipalFromIdToken(string idToken)
        {
            if (string.IsNullOrEmpty(idToken)) return null;

            var handler = new JwtSecurityTokenHandler();
            var decodedIdToken = handler.ReadJwtToken(idToken);
            var claims = decodedIdToken.Claims;
            var claimsIdentity = new ClaimsIdentity(claims, "Google");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            return claimsPrincipal;
        }
    }
}
