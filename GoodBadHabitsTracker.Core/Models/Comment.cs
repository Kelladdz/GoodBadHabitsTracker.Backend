using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace GoodBadHabitsTracker.Core.Models
{
    [Owned]
    public class Comment
    {
        [JsonIgnore]
        public Guid Id { get; set; }
        public required string Body { get; set; } 
        public DateOnly Date { get; private init; } = DateOnly.FromDateTime(DateTime.Now);
        public Guid HabitId { get; set; }
    }
}
