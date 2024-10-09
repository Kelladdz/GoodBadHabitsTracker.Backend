using GoodBadHabitsTracker.Core.Enums;

namespace GoodBadHabitsTracker.Application.DTOs.Habit.Request
{
    public class HabitRequest
    {
        public string? Name { get; set; }
        public HabitTypes? HabitType { get; set; }
        public string? IconPath { get; set; }
        public DateOnly? StartDate { get; set; }
        public bool? IsTimeBased { get; set; }
        public int? Quantity { get; set; }
        public Frequencies? Frequency { get; set; }
        public RepeatModes? RepeatMode { get; set; }
        public int[]? RepeatDaysOfMonth { get; set; }
        public DayOfWeek[]? RepeatDaysOfWeek { get; set; }
        public int? RepeatInterval { get; set; }
        public TimeOnly[]? ReminderTimes { get; set; }
    }
}
