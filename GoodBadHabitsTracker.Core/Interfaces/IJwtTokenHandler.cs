using GoodBadHabitsTracker.Core.Models;
using System.Security.Claims;

namespace GoodBadHabitsTracker.Core.Interfaces
{
    public interface IJwtTokenHandler
    {
        string GenerateToken(ClaimsIdentity claimsIdentity, DateTime expiry);
        string GenerateAccessToken(UserSession userSession, out string userFingerprint);
        string GenerateRefreshToken(UserSession userSession);
        string GenerateUserFingerprint();
        string GenerateUserFingerprintHash(string userFingerprint);
        ClaimsPrincipal ValidateAndGetPrincipalFromToken(string accessToken);
        ClaimsPrincipal GetClaimsPrincipalFromIdToken(string idToken);
        IEnumerable<Claim> GetClaimsFromToken(string token);
    }
}
