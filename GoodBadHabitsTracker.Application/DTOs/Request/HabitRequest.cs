using GoodBadHabitsTracker.Core.Enums;

namespace GoodBadHabitsTracker.Application.DTOs.Request
{
    public record HabitRequest
    {
        public string? Name { get; init; }
        public HabitTypes? HabitType { get; init; }
        public int? IconId { get; init; }
        public DateOnly? StartDate { get; init; }
        public bool? IsTimeBased { get; init; }
        public int? Quantity { get; init; }
        public Frequencies? Frequency { get; init; }
        public RepeatModes? RepeatMode { get; init; }
        public int[]? RepeatDaysOfMonth { get; init; }
        public DayOfWeek[]? RepeatDaysOfWeek { get; init; }
        public int? RepeatInterval { get; init; }
        public TimeOnly[]? ReminderTimes { get; init; }
        public Guid? GroupId { get; init; }
    }
}
