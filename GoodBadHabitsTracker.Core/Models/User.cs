using Microsoft.AspNetCore.Identity;

namespace GoodBadHabitsTracker.Core.Models
{
    public sealed class User : IdentityUser<Guid>
    {
        public List<Habit> Habits { get; init; } = [];
        public ICollection<Group> Groups { get; init; } = [];
        public string? ImageUrl { get; set; }
    }
}

