using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Core.Models.Habit;
using GoodBadHabitsTracker.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Infrastructure.Repositories
{
    public class LimitHabitsRepository(HabitsDbContext dbContext) : ILimitHabitsRepository
    {
        public async Task<bool> CreateAsync(LimitHabit habit, CancellationToken cancellationToken)
        {
            await dbContext.LimitHabit.AddAsync(habit, cancellationToken);
            return await dbContext.SaveChangesAsync(cancellationToken) > 0;
        }

        public async Task<List<LimitHabit>> GetLimitHabitsAsync(string term, DateOnly date, Guid userId, CancellationToken cancellationToken)
        {
            int dayNumber = date.DayNumber;
            string dayOfWeekLower = date.DayOfWeek.ToString().ToLower();

            var limitHabits = await dbContext.LimitHabit
                .Where(h => h.UserId == userId)
                .ToListAsync();

            if (string.IsNullOrWhiteSpace(term))
                return limitHabits;
            else
            {
                term = term.Trim().ToLower();
                var searchedHabits = limitHabits.Where(h => h.Name!.ToLower().Contains(term, StringComparison.OrdinalIgnoreCase)).ToList();
                return searchedHabits;
            }
        }
    }
}
