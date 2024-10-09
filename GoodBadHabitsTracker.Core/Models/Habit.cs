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
        public required string IconPath { get; set; }
        public bool IsTimeBased { get; set; }
        public int? Quantity { get; set; }
        public required Frequencies Frequency { get; set; }
        public required RepeatModes RepeatMode { get; set; }
        public List<DayOfWeek> RepeatDaysOfWeek { get; init; } = [];
        public List<int> RepeatDaysOfMonth { get; init; } = [];
        public int RepeatInterval { get; set; }
        [JsonConverter(typeof(DateOnlyConverter))]
        public required DateOnly StartDate { get; set; }

        [JsonConverter(typeof(TimeOnlyConverter))]
        public List<TimeOnly> ReminderTimes { get; init; } = [];
        [JsonIgnore]
        public User User { get; init; } = default!;
        [JsonIgnore]
        public Guid UserId { get; init; } = default!;
        [JsonIgnore]
        public Group Group { get; set; } = default!;
        [JsonIgnore]
        public Guid GroupId { get; set; } = default!;
    }
}
