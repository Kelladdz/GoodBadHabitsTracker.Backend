namespace GoodBadHabitsTracker.Core.Models
{
    public class Comment
    {
        public Guid Id { get; private init; } = Guid.NewGuid();
        public required string Body { get; set; } 
        public required DateOnly Date { get; init; } 
        public required Guid HabitId { get; init; }
        public Habit Habit { get; set; }
    }
}
