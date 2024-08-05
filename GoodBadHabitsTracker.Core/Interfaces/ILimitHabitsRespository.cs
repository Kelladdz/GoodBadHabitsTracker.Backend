using GoodBadHabitsTracker.Core.Models.Habit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Core.Interfaces
{
    public interface ILimitHabitsRepository
    {
        Task<bool> CreateAsync(LimitHabit habit, CancellationToken cancellationToken);
        Task<List<LimitHabit>> GetLimitHabitsAsync(string term, DateOnly date, Guid userId, CancellationToken cancellationToken);
        Task<bool> EditAsync(LimitHabit habit, CancellationToken cancellationToken);
    }
}
