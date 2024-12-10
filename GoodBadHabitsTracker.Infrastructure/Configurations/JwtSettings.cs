using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;

namespace GoodBadHabitsTracker.Infrastructure.Configurations
{
    public class JwtSettings
    {
        public const string Schemes = "Bearer";
        public string? Issuer { get; set; }
        public string? Subject { get; set; }
        public List<string>? Audience { get; set; }
        public DateTime NotBefore => DateTime.UtcNow;
        public DateTime IssuedAt => DateTime.UtcNow;
        public TimeSpan ValidFor { get; set; } = TimeSpan.FromMinutes(15);
        public DateTime Expiration => IssuedAt.Add(ValidFor);
        public DateTime RefreshTokenExpiration => IssuedAt.AddDays(30);
        public string Jti { get; set; } = Guid.NewGuid().ToString();
        public string Key { get; set; }
        public SigningCredentials? SigningCredentials { get; set; }
        
    }
}
