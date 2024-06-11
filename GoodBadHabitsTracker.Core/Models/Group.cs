using GoodBadHabitsTracker.Core.Models.Habit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Core.Models
{
    public class Group
    {
        public Guid Id { get; private init; } = Guid.NewGuid();
        public string Name { get; set; } = default!;
        public Guid UserId { get; init; }
        public User User { get; init; } = default!;
        public List<GoodHabit> GoodHabits { get; init; } = new();
        public List<QuitHabit> QuitHabits { get; init; } = new();
        public List<LimitHabit> LimitHabits { get; init; } = new();
    }
}
