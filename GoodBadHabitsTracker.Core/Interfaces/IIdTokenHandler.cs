using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Core.Interfaces
{
    public interface IIdTokenHandler
    {
        public ClaimsPrincipal GetClaimsPrincipalFromIdToken(string idToken);
    }
}
