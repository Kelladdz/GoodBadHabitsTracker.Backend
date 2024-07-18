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
    public class QuitHabitsRepository(HabitsDbContext dbContext) : IQuitHabitsRepository
    {
        public async Task<bool> CreateAsync(QuitHabit habit, CancellationToken cancellationToken)
        {
            await dbContext.QuitHabit.AddAsync(habit, cancellationToken);
            return await dbContext.SaveChangesAsync(cancellationToken) > 0;
        }

        public async Task<List<QuitHabit>> GetQuitHabitsAsync(string term, DateOnly date, Guid userId, CancellationToken cancellationToken)
        {
            int dayNumber = date.DayNumber;
            string dayOfWeekLower = date.DayOfWeek.ToString().ToLower();

            var quitHabits = await dbContext.QuitHabit
                .Where(h => h.UserId == userId)
                .ToListAsync();

            if (string.IsNullOrWhiteSpace(term))
                return quitHabits;
            else
            {
                term = term.Trim().ToLower();
                var searchedHabits = quitHabits.Where(h => h.Name!.ToLower().Contains(term, StringComparison.OrdinalIgnoreCase)).ToList();
                return searchedHabits;
            }
        }
    }
}
