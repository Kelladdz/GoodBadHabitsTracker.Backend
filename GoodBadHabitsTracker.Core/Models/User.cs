using GoodBadHabitsTracker.Core.Models.Habit;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Core.Models
{
    public class User
    {
        public Guid Id { get; private init; } = Guid.NewGuid();
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public UserRole Role { get; set; } = default!;
        public List<GoodHabit> GoodHabits { get; init; } = new();
        public List<QuitHabit> QuitHabits { get; init; } = new();
        public List<LimitHabit> LimitHabits { get; init; } = new();
        public string ImagePath { get; set; } = default!;
        public string DisplayName { get; set; } = default!;
        public string RefreshToken { get; set; } = default!;
        public DateTime? RefreshTokenExpirationDate { get; init; } 
        public string Email { get; set; } = default!;
        public byte[] Hash { get; set; } = [];
        public byte[] Salt { get; set; } = [];
    }
}

