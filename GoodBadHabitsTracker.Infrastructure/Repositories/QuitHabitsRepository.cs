using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Core.Models.Habit;
using GoodBadHabitsTracker.Infrastructure.Persistance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Infrastructure.Repositories
{
    public class QuitHabitsRepository(HabitsDbContext dbContext) : IQuitHabitsRepository
    {
        public async Task<bool> CreateAsync(QuitHabit habit, CancellationToken cancellationToken)
        {
            await dbContext.QuitHabit.AddAsync(habit, cancellationToken);
            return await dbContext.SaveChangesAsync(cancellationToken) > 0;
        }
    }
}
