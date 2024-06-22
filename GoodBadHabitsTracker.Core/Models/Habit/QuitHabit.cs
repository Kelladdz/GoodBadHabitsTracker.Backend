using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Core.Models.Habit
{
    public class QuitHabit : Habit
    {
        public QuitHabit()
        {
            IsGood = false;
        }
        public User User { get; init; } = default!;
        public Guid UserId { get; init; }
        public bool IsQuit { get; private init; } = true;
        public Group? Group { get; set; }
        public Guid? GroupId { get; set; }
    }
}
