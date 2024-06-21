using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using GoodBadHabitsTracker.Core.Interfaces;

namespace GoodBadHabitsTracker.Infrastructure.Security.RefreshTokenHandler
{
    internal sealed class Handler : IRefreshTokenHandler
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
