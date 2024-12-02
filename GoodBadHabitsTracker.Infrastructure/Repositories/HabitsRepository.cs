﻿using GoodBadHabitsTracker.Infrastructure.Persistance;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Core.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using GoodBadHabitsTracker.Core.Enums;
using System.Globalization;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Newtonsoft.Json.Linq;

namespace GoodBadHabitsTracker.Infrastructure.Repositories
{
    public sealed class HabitsRepository(HabitsDbContext dbContext) : IHabitsRepository
    {
        public async Task<Habit?> ReadByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var habit = await dbContext.Habits
                    .Include(x => x.DayResults)
                    .Include(x => x.Comments)
                    .AsNoTracking()
                    .AsSplitQuery()
                    .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);

            return habit;
        }

        public async Task<IEnumerable<Habit>> ReadAllAsync(Guid userId, CancellationToken cancellationToken)
        {
            var habits = await dbContext.Habits
                    .Where(h => h.UserId == userId)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

            return habits;
        }

        public async Task<IEnumerable<Habit>> SearchAsync(string? term, DateOnly date, Guid userId, CancellationToken cancellationToken)
        {


            var habits = await dbContext.Habits
                .Where(h => h.UserId == userId)
                .AsNoTracking()
                .AsSingleQuery()
                .ToListAsync(cancellationToken);

            if (habits is null)
                return [];

            var filteredHabits = habits
                .Where(h => IsHabitMatched(date, h));


            if (string.IsNullOrWhiteSpace(term))
            {
                if (!filteredHabits.Any())
                    return [];

                else return filteredHabits;
            }

            else
            {
                term = term.Trim().ToLower();
                var searchedHabits = filteredHabits.Where(h => h.Name!.ToLower().Contains(term, StringComparison.OrdinalIgnoreCase)).ToList();
                return searchedHabits;
            }
        }

        public async Task<Habit?> InsertAsync(Habit habitToInsert, Guid userId, CancellationToken cancellationToken)
        {
            var newHabit = habitToInsert!.HabitType switch
            {
                HabitTypes.Good => new Habit
                {
                    Name = habitToInsert.Name,
                    HabitType = habitToInsert.HabitType,
                    IconId = habitToInsert.IconId,
                    IsTimeBased = habitToInsert.IsTimeBased,
                    Quantity = habitToInsert.Quantity,
                    Frequency = habitToInsert.Frequency,
                    RepeatMode = habitToInsert.RepeatMode,
                    RepeatDaysOfWeek = habitToInsert.RepeatDaysOfWeek,
                    RepeatDaysOfMonth = habitToInsert.RepeatDaysOfMonth,
                    RepeatInterval = habitToInsert.RepeatInterval,
                    StartDate = habitToInsert.StartDate,
                    UserId = userId,
                    GroupId = habitToInsert.GroupId,
                },
                HabitTypes.Limit => new Habit
                {
                    Name = habitToInsert.Name,
                    HabitType = habitToInsert.HabitType,
                    IconId = habitToInsert.IconId,
                    IsTimeBased = habitToInsert.IsTimeBased,
                    Quantity = habitToInsert.Quantity,
                    Frequency = habitToInsert.Frequency,
                    RepeatMode = RepeatModes.NotApplicable,
                    RepeatInterval = 0,
                    StartDate = habitToInsert.StartDate,
                    UserId = userId,
                    GroupId = habitToInsert.GroupId,
                },
                HabitTypes.Quit => new Habit
                {
                    Name = habitToInsert.Name,
                    HabitType = habitToInsert.HabitType,
                    IconId = habitToInsert.IconId,
                    IsTimeBased = false,
                    Frequency = Frequencies.NotApplicable,
                    RepeatMode = RepeatModes.NotApplicable,
                    RepeatInterval = 0,
                    StartDate = habitToInsert.StartDate,
                    UserId = userId,
                    GroupId = habitToInsert.GroupId,
                },
                _ => throw new InvalidOperationException("Something goes wrong")
            };

            var currentDayResult = habitToInsert!.HabitType switch
            {
                HabitTypes.Good => new DayResult
                {
                    Progress = 0,
                    Status = Statuses.InProgress,
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    HabitId = newHabit.Id
                },
                HabitTypes.Limit => new DayResult
                {
                    Progress = 0,
                    Status = Statuses.InProgress,
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    HabitId = newHabit.Id
                },
                HabitTypes.Quit => new DayResult
                {
                    Status = Statuses.InProgress,
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    HabitId = newHabit.Id
                },
                _ => throw new InvalidOperationException("Something goes wrong")
            };

            newHabit.DayResults.Add(currentDayResult);
            dbContext.Habits.Add(newHabit);

            return await dbContext.SaveChangesAsync(cancellationToken) > 0
                ? newHabit : null;
        }

        public async Task<bool> UpdateAsync(JsonPatchDocument<Habit> document, Guid id, CancellationToken cancellationToken)
        {
            var habitToUpdate = await dbContext.Habits
                .FindAsync(id, cancellationToken)
                ?? throw new InvalidOperationException("Habit not found");

            var dayResultsDates = habitToUpdate!.DayResults.Select(dayResult => dayResult.Date.ToString("o", CultureInfo.InvariantCulture)).ToList();

            if (document.Operations.Any(o => o.OperationType == OperationType.Add
                   && o.path == "/dayResults/-"
                   && dayResultsDates.Contains((string)JObject.Parse(o.value.ToString()!)["Date"]!)))
                throw new InvalidOperationException("Two day results cannot have one date, use replace operation instead");

            if (habitToUpdate!.HabitType != HabitTypes.Quit)
            {
                var quantity = habitToUpdate.Quantity;

                if (document.Operations.Any(o => o.OperationType == OperationType.Add
                    && o.path == "/dayResults/-"
                    && (int)JObject.Parse(o.value.ToString()!)["Progress"]! < quantity
                    && (Statuses)Enum.Parse(typeof(Statuses), JObject.Parse(o.value.ToString()!)["Status"]!.ToString()) == Statuses.Completed))
                    throw new InvalidOperationException("Progress can't be less than quantity if status is completed");

                if (document.Operations.Any(o => o.OperationType == OperationType.Add
                    && o.path == "/dayResults/-"
                    && (int)JObject.Parse(o.value.ToString()!)["Progress"]! >= quantity
                    && (Statuses)Enum.Parse(typeof(Statuses), JObject.Parse(o.value.ToString()!)["Status"]!.ToString()) != Statuses.Completed))
                    throw new InvalidOperationException("Progress can't be more than quantity if status is not completed");

                if (document.Operations.Any(o => o.OperationType == OperationType.Add
                    && o.path == "/dayResults/-"
                    && (int)JObject.Parse(o.value.ToString()!)["Progress"]! >= quantity
                    && (Statuses)Enum.Parse(typeof(Statuses), JObject.Parse(o.value.ToString()!)["Status"]!.ToString()) != Statuses.Completed))
                    throw new InvalidOperationException("Progress can't be more than quantity if status is not completed");
            }

            if (habitToUpdate!.HabitType != HabitTypes.Good)
            {
                var repeatDaysOfMonth = habitToUpdate.RepeatDaysOfMonth;
                var repeatDaysOfWeek = habitToUpdate.RepeatDaysOfWeek;
                var repeatInterval = habitToUpdate.RepeatInterval;

                if (document.Operations
                .Any(o => o.OperationType == OperationType.Replace
                    && o.path == "/repeatMode"
                    && (int)o.value == (int)RepeatModes.Daily)
                    && !document.Operations
                .Any(o => o.OperationType == OperationType.Add
                    && o.path == "/repeatDaysOfWeek/-"))
                    throw new InvalidOperationException("RepeatDaysOfWeek should be added if RepeatMode is Daily");

                if (document.Operations
                    .Any(o => o.OperationType == OperationType.Replace
                        && o.path == "/repeatMode"
                        && (int)o.value == (int)RepeatModes.Monthly)
                && !document.Operations
                    .Any(o => o.OperationType == OperationType.Add
                        && o.path == "/repeatDaysOfMonth/-"))
                    throw new InvalidOperationException("RepeatDaysOfMonth should be added if RepeatMode is Monthly");

                if (document.Operations
                    .Any(o => o.OperationType == OperationType.Replace
                        && o.path == "/repeatMode"
                        && (int)o.value == (int)RepeatModes.Interval)
                && !document.Operations
                    .Any(o => o.OperationType == OperationType.Replace
                        && o.path == "/repeatInterval"
                        && ((int)o.value > 0 && (int)o.value < 7)))
                    throw new InvalidOperationException("RepeatDaysOfMonth should be added if RepeatMode is Monthly");

                if (document.Operations.Any(o => o.OperationType == OperationType.Replace
                     && o.path == "/repeatMode"
                     && (int)o.value == (int)RepeatModes.Daily))
                {
                    if (repeatDaysOfMonth.Count != 0)
                        habitToUpdate.RepeatDaysOfMonth.Clear();

                    if (repeatInterval != 0)
                        habitToUpdate.RepeatInterval = 0;
                }

                if (document.Operations.Any(o => o.OperationType == OperationType.Replace
                     && o.path == "/repeatMode"
                     && (int)o.value == (int)RepeatModes.Monthly))
                {
                    if (repeatDaysOfWeek!.Count != 0)
                        habitToUpdate.RepeatDaysOfWeek.Clear();

                    if (repeatInterval != 0)
                        habitToUpdate.RepeatInterval = 0;
                }

                if (document.Operations.Any(o => o.OperationType == OperationType.Replace
                     && o.path == "/repeatMode"
                     && (int)o.value == (int)RepeatModes.Interval))
                {
                    if (repeatDaysOfWeek.Count != 0)
                        habitToUpdate.RepeatDaysOfWeek.Clear();

                    if (repeatDaysOfMonth.Count != 0)
                        habitToUpdate.RepeatDaysOfMonth.Clear();
                }
            }

            document.ApplyTo(habitToUpdate);

            return await dbContext.SaveChangesAsync(cancellationToken) > 0;
        }
        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var habitToRemove = await dbContext.Habits.FindAsync(id, cancellationToken)
                ?? throw new InvalidOperationException("Habit not found");

            dbContext.Habits.Remove(habitToRemove);

            return await dbContext.SaveChangesAsync(cancellationToken) > 0;
        }

        private static bool IsHabitMatched(DateOnly date, Habit habit)
        {
            var dayNumber = date.DayNumber;
            var dayOfWeek = date.DayOfWeek; 

            return habit.HabitType == HabitTypes.Good && (habit.RepeatMode == RepeatModes.Daily && habit.RepeatDaysOfWeek.Contains(dayOfWeek) ||
                   habit.RepeatMode == RepeatModes.Monthly && habit.RepeatDaysOfMonth.Contains(date.Day) ||
                   habit.RepeatMode == RepeatModes.Interval && (dayNumber - habit.StartDate.DayNumber) % habit.RepeatInterval == 0) || 
                   habit.HabitType != HabitTypes.Good;
        }
    }
}