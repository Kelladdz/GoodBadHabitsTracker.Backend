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
        public User User { get; set; } = default!;
        public Guid UserId { get; set; } = default!;
        public Group Group { get; set; } = default!;
        public Guid? GroupId { get; set; }
        public ICollection<Comment> Comments { get; init; } = new List<Comment>();
        public ICollection<DayResult> DayResults { get; init; } = new List<DayResult>();
    }
}
