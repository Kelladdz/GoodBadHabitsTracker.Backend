using GoodBadHabitsTracker.Core.Models;
using System.Security.Claims;

namespace GoodBadHabitsTracker.Core.Interfaces
{
    public interface IAccessTokenHandler
    {
        string GenerateAccessToken(UserSession userSession, out string userFingerprint);
        string GenerateUserFingerprint();
        string GenerateUserFingerprintHash(string userFingerprint);
        ClaimsPrincipal GetPrincipalFromExpiredToken(string accessToken);
        ClaimsPrincipal GetClaims(string token);
    }
}
