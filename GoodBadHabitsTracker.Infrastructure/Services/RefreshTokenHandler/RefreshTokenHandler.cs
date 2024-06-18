using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Infrastructure.Services.RefreshTokenHandler
{
    internal sealed class RefreshTokenHandler : IRefreshTokenHandler
    {
        public string GenerateRefreshToken()
        {
            string refreshToken;
            var randomString = new byte[32];
            using (var secureRandom = RandomNumberGenerator.Create())
            {
                secureRandom.GetBytes(randomString);
                refreshToken = Convert.ToBase64String(randomString);
            }

            return refreshToken;
        }
    }
}
