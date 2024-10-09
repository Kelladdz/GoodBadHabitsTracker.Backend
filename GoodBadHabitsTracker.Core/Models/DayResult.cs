using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using GoodBadHabitsTracker.Core.Enums;

namespace GoodBadHabitsTracker.Core.Models
{
    [Owned]
    public class DayResult
    {
        [JsonIgnore]
        public Guid Id { get; private init; } = Guid.NewGuid();
        public required int Progress { get; set; }
        public required Statuses Status { get; set; }
        public DateOnly Date { get; private init; } = DateOnly.FromDateTime(DateTime.Now);
        public Guid HabitId { get; set; }
    }
}
