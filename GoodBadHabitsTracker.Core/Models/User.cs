using GoodBadHabitsTracker.Core.Models.Habit;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Core.Models
{
    public class User : IdentityUser<Guid>
    {
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public ICollection<GoodHabit> GoodHabits { get; init; } = new List<GoodHabit>();
        public ICollection<QuitHabit> QuitHabits { get; init; } = new List<QuitHabit>();
        public ICollection<LimitHabit> LimitHabits { get; init; } = new List<LimitHabit>();
        public ICollection<Group> Groups { get; init; } = new List<Group>();
        public string ImagePath { get; set; } = default!;
        public string DisplayName { get; set; } = default!;
        public string RefreshToken { get; set; } = default!;
        public DateTime? RefreshTokenExpirationDate { get; init; }
        public override string Email { get; set; } = default!;
    }
}

