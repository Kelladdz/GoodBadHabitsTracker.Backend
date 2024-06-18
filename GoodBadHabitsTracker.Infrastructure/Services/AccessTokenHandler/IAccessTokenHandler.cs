using GoodBadHabitsTracker.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Infrastructure.Services.AccessTokenHandler
{
    public interface IAccessTokenHandler
    {
        string GenerateAccessToken(UserSession userSession, out string userFingerprint);
        public string GenerateUserFingerprint();
        public string GenerateUserFingerprintHash(string userFingerprint);
    }
}
