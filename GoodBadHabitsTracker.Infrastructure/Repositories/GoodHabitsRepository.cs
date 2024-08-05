using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Core.Models.Habit;
using GoodBadHabitsTracker.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Net;

namespace GoodBadHabitsTracker.Infrastructure.Repositories
{
    public class GoodHabitsRepository(HabitsDbContext dbContext) : IGoodHabitsRepository
    {
        public async Task<bool> CreateAsync(GoodHabit habit, CancellationToken cancellationToken)
        {
            await dbContext.GoodHabits.AddAsync(habit, cancellationToken);
            return await dbContext.SaveChangesAsync(cancellationToken) > 0;
        }

        public async Task<List<GoodHabit>> GetGoodHabitsAsync(string term, DateOnly date, Guid userId, CancellationToken cancellationToken)
        {
            int dayNumber = date.DayNumber;
            string dayOfWeekLower = date.DayOfWeek.ToString();

            var goodHabits = await dbContext.GoodHabits
                .Where(h => h.UserId == userId)
                .ToListAsync();

            var filteredHabits = goodHabits
            .Where(h => h.RepeatMode == Core.Enums.RepeatModes.Daily && h.RepeatDaysOfWeek.Contains(dayOfWeekLower) ||
            h.RepeatMode == Core.Enums.RepeatModes.Monthly && h.RepeatDaysOfMonth.Contains(date.Day) ||
                            h.RepeatMode == Core.Enums.RepeatModes.Interval && (dayNumber - h.StartDate.DayNumber) % h.RepeatInterval == 0 ||
                            !h.IsGood)
                .ToList();

            if (string.IsNullOrWhiteSpace(term))
                return filteredHabits;
            else
            {
                term = term.Trim().ToLower();
                var searchedHabits = filteredHabits.Where(h => h.Name!.ToLower().Contains(term, StringComparison.OrdinalIgnoreCase)).ToList();
                return searchedHabits;
            }
        }

        public async Task<bool> EditAsync(GoodHabit habitToUpdate, CancellationToken cancellationToken)
        {
            var habit = await dbContext.GoodHabits.FirstOrDefaultAsync(h => h.Id == habitToUpdate.Id);

            if (habit is null) return false;

            habit.Name = habitToUpdate.Name;
            habit.IconPath = habitToUpdate.IconPath;
            habit.Quantity = habitToUpdate.Quantity;
            habit.Frequency = habitToUpdate.Frequency;
            habit.RepeatMode = habitToUpdate.RepeatMode;

            habit.RepeatDaysOfWeek.Clear();
            habit.RepeatDaysOfWeek.AddRange(habitToUpdate.RepeatDaysOfWeek);

            habit.RepeatDaysOfMonth.Clear();
            habit.RepeatDaysOfMonth.AddRange(habitToUpdate.RepeatDaysOfMonth);

            habit.RepeatInterval = habitToUpdate.RepeatInterval;
            habit.StartDate = habitToUpdate.StartDate;

            habit.ReminderTimes.Clear();
            habit.ReminderTimes.AddRange(habitToUpdate.ReminderTimes);

            return await dbContext.SaveChangesAsync() > 0;
        }
    }
}
