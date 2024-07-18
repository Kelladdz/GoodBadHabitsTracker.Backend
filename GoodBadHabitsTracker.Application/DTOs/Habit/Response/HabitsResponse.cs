using GoodBadHabitsTracker.Core.Models.Habit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.DTOs.Habit.Response
{
    public record HabitsResponse(
        IEnumerable<GoodHabit> GoodHabits, 
        IEnumerable<LimitHabit> LimitHabits, 
        IEnumerable<QuitHabit> QuitHabits);
}
