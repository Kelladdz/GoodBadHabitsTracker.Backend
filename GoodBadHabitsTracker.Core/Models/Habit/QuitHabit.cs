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
        public bool IsQuit { get; private init; } = true;
    }
}
