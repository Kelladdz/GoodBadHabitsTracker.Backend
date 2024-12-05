using GoodBadHabitsTracker.Infrastructure.Persistance;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Core.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using GoodBadHabitsTracker.Core.Enums;
using System.Globalization;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Newtonsoft.Json.Linq;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.SignalR.Protocol;

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
        public async Task<bool> DeleteAllProgressAsync(Guid userId, CancellationToken cancellationToken)
        {

                var habits = await dbContext.Habits
                    .Where(habit => habit.UserId == userId)
                    .Include(habit => habit.DayResults)
                    .ToListAsync(cancellationToken);

                foreach (var habit in habits)
                {
                    habit.DayResults.Clear();
                }

                await dbContext.SaveChangesAsync(cancellationToken);
                return true;
        }
        public async Task<bool> UpdateAllAsync()
        {
            var connectionString = dbContext.Database.GetConnectionString();
            using var bulk = new SqlBulkCopy(connectionString, SqlBulkCopyOptions.Default);
            using var transaction = await dbContext.Database.BeginTransactionAsync();

            try
            {
                var habits = await dbContext.Habits.Include(h => h.DayResults).ToListAsync();

                var destinationTableName = "dbo.DayResults";
                bulk.ColumnMappings.Add(nameof(DayResult.Id), "Id");
                bulk.ColumnMappings.Add(nameof(DayResult.Progress), "Progress");
                bulk.ColumnMappings.Add(nameof(DayResult.Status), "Status");
                bulk.ColumnMappings.Add(nameof(DayResult.Date), "Date");
                bulk.ColumnMappings.Add(nameof(DayResult.HabitId), "HabitId");

                var dataTable = new DataTable();

                dataTable.Columns.Add(nameof(DayResult.Id), typeof(Guid));
                dataTable.Columns.Add(nameof(DayResult.Progress), typeof(int));
                dataTable.Columns.Add(nameof(DayResult.Status), typeof(Statuses));
                dataTable.Columns.Add(nameof(DayResult.Date), typeof(DateOnly));
                dataTable.Columns.Add(nameof(DayResult.HabitId), typeof(Guid));

                foreach (var habit in habits)
                {
                    var habitId = habit.Id;
                    var currentDay = DateOnly.FromDateTime(DateTime.Today);
                    while (currentDay > habit.StartDate)
                    {
                        if (currentDay == DateOnly.FromDateTime(DateTime.Today) && !habit.DayResults.Any(dayResult => dayResult.Date == currentDay) && IsHabitMatched(currentDay, habit))
                            dataTable.Rows.Add(Guid.NewGuid(), 0, Statuses.InProgress, currentDay, habitId);
                        else if (!habit.DayResults.Any(dayResult => dayResult.Date == currentDay) && IsHabitMatched(currentDay, habit))
                            dataTable.Rows.Add(Guid.NewGuid(), 0, Statuses.Failed, currentDay, habitId);
                        else if (!habit.DayResults.Any(dayResult => dayResult.Date == currentDay) && !IsHabitMatched(currentDay, habit))
                            dataTable.Rows.Add(Guid.NewGuid(), 0, Statuses.None, currentDay, habitId);
                        currentDay = currentDay.AddDays(-1);
                    }
                }
                bulk.DestinationTableName = destinationTableName;
                await bulk.WriteToServerAsync(dataTable);

                await dbContext.Habits.Include(h => h.DayResults)
                .SelectMany(habit => habit.DayResults)
                .Where(dayResult => dayResult.Date < DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1))
                && dayResult.Status == Statuses.InProgress)
                .ExecuteUpdateAsync(dayResult => dayResult.SetProperty(d => d.Status, Statuses.Failed));
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
        public async Task<bool> PostInProgressTodayAsync(Guid userId, CancellationToken cancellationToken)
        {
            var connectionString = dbContext.Database.GetConnectionString();
            using var bulk = new SqlBulkCopy(connectionString, SqlBulkCopyOptions.Default);
            using var transaction = await dbContext.Database.BeginTransactionAsync();

            try
            {
                var habits = await dbContext.Habits
                    .Include(habit => habit.DayResults)
                    .Where(h => h.UserId == userId)
                    .ToListAsync(cancellationToken);

                var currentDay = DateOnly.FromDateTime(DateTime.Today);

                var destinationTableName = "dbo.DayResults";
                bulk.ColumnMappings.Add(nameof(DayResult.Id), "Id");
                bulk.ColumnMappings.Add(nameof(DayResult.Progress), "Progress");
                bulk.ColumnMappings.Add(nameof(DayResult.Status), "Status");
                bulk.ColumnMappings.Add(nameof(DayResult.Date), "Date");
                bulk.ColumnMappings.Add(nameof(DayResult.HabitId), "HabitId");

                var dataTable = new DataTable();

                dataTable.Columns.Add(nameof(DayResult.Id), typeof(Guid));
                dataTable.Columns.Add(nameof(DayResult.Progress), typeof(int));
                dataTable.Columns.Add(nameof(DayResult.Status), typeof(Statuses));
                dataTable.Columns.Add(nameof(DayResult.Date), typeof(DateOnly));
                dataTable.Columns.Add(nameof(DayResult.HabitId), typeof(Guid));

                foreach (var habit in habits)
                {
                    var habitId = habit.Id;
                    dataTable.Rows.Add(Guid.NewGuid(), 0, Statuses.InProgress, currentDay, habitId);
                }
                bulk.DestinationTableName = destinationTableName;
                await bulk.WriteToServerAsync(dataTable, cancellationToken);

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var habitToRemove = await dbContext.Habits.FindAsync(id, cancellationToken)
                ?? throw new InvalidOperationException("Habit not found");

            dbContext.Habits.Remove(habitToRemove);

            return await dbContext.SaveChangesAsync(cancellationToken) > 0;
        }

        public async Task<bool> DeleteAllAsync(Guid userId, CancellationToken cancellationToken)
        {
            var habits = await dbContext.Habits
                .Where(habit => habit.UserId == userId)
                .ToListAsync(cancellationToken);

            dbContext.Habits.RemoveRange(habits);

            return await dbContext.SaveChangesAsync(cancellationToken) > 0;
        }

        private static bool IsHabitMatched(DateOnly date, Habit habit)
        {
            var dayNumber = date.DayNumber;
            var dayOfWeek = date.DayOfWeek;

            return (habit.HabitType == HabitTypes.Good
                && ((habit.RepeatMode == RepeatModes.Daily && habit.RepeatDaysOfWeek.Contains(dayOfWeek)) ||
                   (habit.RepeatMode == RepeatModes.Monthly && habit.RepeatDaysOfMonth.Contains(date.Day)) ||
                   (habit.RepeatMode == RepeatModes.Interval && (dayNumber - habit.StartDate.DayNumber) % habit.RepeatInterval == 0))) ||
                   habit.HabitType != HabitTypes.Good;
        }
    }
}
