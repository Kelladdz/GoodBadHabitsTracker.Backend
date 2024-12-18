using GoodBadHabitsTracker.Core.Enums;

namespace GoodBadHabitsTracker.Core.Models
{
    public class DayResult
    {
        public Guid Id { get; private init; } = Guid.NewGuid();
        public int? Progress { get; set; }
        public required Statuses Status { get; set; }
        public required DateOnly Date { get; init; } 
        public Guid HabitId { get; init; }
        public Habit Habit { get; init; } = default!;
    }
}
