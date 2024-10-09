using System.Security.Cryptography;
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
