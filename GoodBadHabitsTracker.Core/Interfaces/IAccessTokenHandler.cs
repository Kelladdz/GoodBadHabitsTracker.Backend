using GoodBadHabitsTracker.Core.Models;

namespace GoodBadHabitsTracker.Core.Interfaces
{
    public interface IAccessTokenHandler
    {
        string GenerateAccessToken(UserSession userSession, out string userFingerprint);
        public string GenerateUserFingerprint();
        public string GenerateUserFingerprintHash(string userFingerprint);
    }
}
