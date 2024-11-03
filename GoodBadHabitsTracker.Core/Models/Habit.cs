using System.ComponentModel;
using System.Text.Json.Serialization;
using GoodBadHabitsTracker.Core.Enums;

namespace GoodBadHabitsTracker.Core.Models
{
    public class Habit
    {
        public Guid Id { get; private init; } = Guid.NewGuid();
        public required string Name { get; set; }
        public required HabitTypes HabitType { get; init; }
        public required int IconId { get; set; }
        public bool IsTimeBased { get; set; }
        public int? Quantity { get; set; }
        public required Frequencies Frequency { get; set; }
        public required RepeatModes RepeatMode { get; set; }
        public List<DayOfWeek> RepeatDaysOfWeek { get; init; } = [];
        public List<int> RepeatDaysOfMonth { get; init; } = [];
        public int RepeatInterval { get; set; }
        public required DateOnly StartDate { get; set; }
        public List<TimeOnly> ReminderTimes { get; init; } = [];
        [JsonIgnore]
        public User User { get; init; } = default!;
        [JsonIgnore]
        public Guid UserId { get; init; } = default!;
        [JsonIgnore]
        public Group Group { get; set; } = default!;
        [JsonIgnore]
        public Guid GroupId { get; set; } = default!;
        public ICollection<Comment> Comments { get; init; } = new List<Comment>();
        public ICollection<DayResult> DayResults { get; init; } = new List<DayResult>();
    }
}
