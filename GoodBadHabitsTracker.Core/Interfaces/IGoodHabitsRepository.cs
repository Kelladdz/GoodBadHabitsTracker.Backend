using GoodBadHabitsTracker.Core.Models.Habit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Core.Interfaces
{
    public interface IGoodHabitsRepository
    {
        Task<bool> CreateAsync(GoodHabit goodHabit, CancellationToken cancellationToken);
    }
}
