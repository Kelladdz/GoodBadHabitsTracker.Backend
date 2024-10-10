namespace GoodBadHabitsTracker.Core.Models
{
    public sealed class Group
    {
        public Guid Id { get; private init; } = Guid.NewGuid();
        public required string Name { get; set; }
        public Guid UserId { get; init; }
        public User User { get; init; }
        public List<Habit> Habits { get; init; } = new();
    }
}
