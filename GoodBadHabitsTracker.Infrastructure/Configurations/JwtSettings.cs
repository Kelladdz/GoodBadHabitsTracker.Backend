﻿using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Infrastructure.Configurations
{
    public class JwtSettings
    {
        public const string SCHEMES = "Bearer";
        public string? Issuer { get; set; }
        public string? Subject { get; set; }
        public string? Audience { get; set; }
        public DateTime NotBefore => DateTime.UtcNow;
        public DateTime IssuedAt => DateTime.UtcNow;
        public TimeSpan ValidFor { get; set; } = TimeSpan.FromMinutes(15);
        public DateTime Expiration => IssuedAt.Add(ValidFor);
        public string JtiGenerator { get; set; } = Guid.NewGuid().ToString();
        public SigningCredentials? SigningCredentials { get; set; }
    }
}
