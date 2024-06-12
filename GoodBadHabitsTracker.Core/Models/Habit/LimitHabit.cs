using GoodBadHabitsTracker.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Core.Models.Habit
{
    public class LimitHabit : Habit
    {
        public LimitHabit()
        {
            IsGood = false;
        }
        public User User { get; init; } = default!;
        public Guid UserId { get; init; }
        public bool IsQuit { get; private init; } = false;
        public bool IsTimeBased { get; set; }
        public int Quantity { get; set; }
        public Frequencies Frequency { get; set; } = default!;
        public Group? Group { get; set; }
        public Guid GroupId { get; set; }
    }
}
