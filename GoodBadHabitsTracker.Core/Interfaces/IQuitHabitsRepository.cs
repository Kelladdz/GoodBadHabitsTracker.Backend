using GoodBadHabitsTracker.Core.Models.Habit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Core.Interfaces
{
    public interface IQuitHabitsRepository
    {
        Task<bool> CreateAsync(QuitHabit habit, CancellationToken cancellationToken);
        Task<List<QuitHabit>> GetQuitHabitsAsync(string term, DateOnly date, Guid userId, CancellationToken cancellationToken);
        Task<bool> EditAsync(QuitHabit habit, CancellationToken cancellationToken);
    }
}
