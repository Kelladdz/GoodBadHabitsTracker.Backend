using GoodBadHabitsTracker.Core.Enums;
using GoodBadHabitsTracker.Core.Models;

namespace GoodBadHabitsTracker.Application.Services;

public static class HabitStatistics
{
    public static Stats GetStats(Habit habit)
    {
        var completed = habit.DayResults.Count(dr => dr.Status == Statuses.Completed);
        var failed = habit.DayResults.Count(dr => dr.Status == Statuses.Failed);
        var skipped = habit.DayResults.Count(dr => dr.Status == Statuses.Skipped);

        var allDayResults = habit.DayResults.Where(dr => dr.Date <= DateOnly.FromDateTime(DateTime.Today)).OrderByDescending(dr => dr.Date.DayNumber).ToList();

        var streak = 0;

        for (var i = 0; i < allDayResults.Count; i++)
        {
            if (allDayResults[i].Status == Statuses.Completed)
            {
                streak++;
            }
            else
            {
                break;
            }
        }

        return new Stats
        {
            Completed = completed,
            Failed = failed,
            Skipped = skipped,
            Streak = streak,
            Total = allDayResults.Count(dr => dr.Status != Statuses.InProgress)
        };
    }
}