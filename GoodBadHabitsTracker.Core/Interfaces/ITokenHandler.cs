using GoodBadHabitsTracker.Core.Models;
using System.Security.Claims;

namespace GoodBadHabitsTracker.Core.Interfaces
{
    public interface ITokenHandler
    {
        string GenerateToken(ClaimsIdentity claimsIdentity, DateTime expiry);
        string GenerateAccessToken(UserSession userSession, out string userFingerprint);
        string GenerateRefreshToken(UserSession userSession);
        string GenerateUserFingerprintHash(string userFingerprint);
        ClaimsPrincipal ValidateAndGetPrincipalFromToken(string accessToken);
        IEnumerable<Claim> GetClaimsFromToken(string token);
    }
}
