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
using LanguageExt.SomeHelp;

namespace GoodBadHabitsTracker.Infrastructure.Repositories
{
    public sealed class HabitsRepository(HabitsDbContext dbContext) : IHabitsRepository
    {
        public async Task<Habit?> FindAsync(Guid id, CancellationToken cancellationToken)
            => await dbContext.Habits
                        .FindAsync(id, cancellationToken);
        public async Task<Habit?> ReadByIdAsync(Guid id, CancellationToken cancellationToken)
            => await dbContext.Habits
                    .Include(x => x.DayResults)
                    .Include(x => x.Comments)
                    .AsNoTracking()
                    .AsSplitQuery()
                    .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
        public async Task<IEnumerable<Habit>> ReadAllAsync(Guid userId, CancellationToken cancellationToken)
            => await dbContext.Habits
                    .Where(h => h.UserId == userId)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
        public async Task<IEnumerable<Habit>> SearchAsync(string? term, DateOnly date, Guid userId, CancellationToken cancellationToken)
        {


            var habits = await dbContext.Habits
                .Where(h => h.UserId == userId)
                .AsNoTracking()
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

        public async Task InsertAsync(Habit habitToInsert, CancellationToken cancellationToken)
        {
            var currentDayResult = habitToInsert!.HabitType switch
            {
                HabitTypes.Good => new DayResult
                {
                    Progress = 0,
                    Status = Statuses.InProgress,
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    HabitId = habitToInsert.Id
                },
                HabitTypes.Limit => new DayResult
                {
                    Progress = 0,
                    Status = Statuses.InProgress,
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    HabitId = habitToInsert.Id
                },
                HabitTypes.Quit => new DayResult
                {
                    Status = Statuses.InProgress,
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    HabitId = habitToInsert.Id
                },
                _ => throw new InvalidOperationException("Something goes wrong")
            };

            habitToInsert.DayResults.Add(currentDayResult);
            dbContext.Habits.Add(habitToInsert);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> UpdateAsync(JsonPatchDocument document, Habit habitToUpdate, CancellationToken cancellationToken)
        {
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
        public async Task DeleteAllProgressAsync(Guid userId, CancellationToken cancellationToken)
        {
            var connectionString = dbContext.Database.GetConnectionString();
            using var bulk = new SqlBulkCopy(connectionString, SqlBulkCopyOptions.Default);
            using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var allHabits = await dbContext.Habits
                .Where(h => h.UserId == userId)
                .Include(h => h.DayResults)
                .ToListAsync(cancellationToken);

                var currentDay = DateOnly.FromDateTime(DateTime.Today);
                foreach (var habit in allHabits)
                {
                    habit.DayResults.Clear();
                    habit.StartDate = currentDay;
                }
                await dbContext.SaveChangesAsync(cancellationToken);
                transaction.CreateSavepoint("AllProgressDeleted");

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

                foreach (var habit in allHabits)
                {
                    var habitId = habit.Id;
                    dataTable.Rows.Add(Guid.NewGuid(), 0, Statuses.InProgress, currentDay, habitId);
                }
                bulk.DestinationTableName = destinationTableName;
                await bulk.WriteToServerAsync(dataTable, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception _)
            {
                await transaction.RollbackToSavepointAsync("AllProgressDeleted", cancellationToken);
            }

            
        }
        public async Task UpdateAllAsync()
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
                    switch (habit.Frequency)
                    {
                        case Frequencies.PerDay:
                            while (currentDay >= habit.StartDate)
                            {
                                if (currentDay == DateOnly.FromDateTime(DateTime.Today) && !habit.DayResults.Any(dayResult => dayResult.Date == currentDay) && IsHabitMatched(currentDay, habit))
                                    dataTable.Rows.Add(Guid.NewGuid(), 0, Statuses.InProgress, currentDay, habitId);
                                else if (currentDay != DateOnly.FromDateTime(DateTime.Today) && (!habit.DayResults.Any(dayResult => dayResult.Date == currentDay) || habit.DayResults.Any(dayResult => dayResult.Date == currentDay && dayResult.Status == Statuses.InProgress)) && IsHabitMatched(currentDay, habit))
                                    dataTable.Rows.Add(Guid.NewGuid(), 0, Statuses.Failed, currentDay, habitId);
                                else if (!IsHabitMatched(currentDay, habit))
                                    dataTable.Rows.Add(Guid.NewGuid(), 0, Statuses.None, currentDay, habitId);
                                currentDay = currentDay.AddDays(-1);
                            }
                            break;  
                        case Frequencies.PerWeek:
                            var startDateDayOfWeek = habit.StartDate.DayOfWeek;
                            while (currentDay >= habit.StartDate)
                            {
                                if ((currentDay.DayOfWeek == startDateDayOfWeek && (habit.DayResults.Any(dayResult => dayResult.Date == currentDay && dayResult.Status == Statuses.InProgress) || !habit.DayResults.Any(dayResult => dayResult.Date == currentDay)) && IsHabitMatched(currentDay, habit)))     
                                {
                                   for (int i = (int)currentDay.DayOfWeek; i >= 0; i--)
                                   {
                                        if (habit.RepeatDaysOfWeek.Contains((DayOfWeek)i))
                                            dataTable.Rows.Add(Guid.NewGuid(), habit.DayResults.First(dr => dr.Date == currentDay.AddDays(-i)).Progress, Statuses.Failed, currentDay.AddDays(-i), habitId);
                                   }
                                }
                                else if ((currentDay.DayOfWeek != startDateDayOfWeek) && IsHabitMatched(currentDay, habit))
                                    dataTable.Rows.Add(Guid.NewGuid(), habit.DayResults.First(dr => dr.Date == currentDay).Progress, Statuses.InProgress, currentDay, habitId);
                                else if (!IsHabitMatched(currentDay, habit))
                                    dataTable.Rows.Add(Guid.NewGuid(), 0, Statuses.None, currentDay, habitId);
                                currentDay = currentDay.AddDays(-1);
                            }
                            break;
                        case Frequencies.PerMonth:
                            var startDateDay = habit.StartDate.Day;
                            while (currentDay >= habit.StartDate)
                            {
                                if ((currentDay.Day == startDateDay && (habit.DayResults.Any(dayResult => dayResult.Date == currentDay && dayResult.Status == Statuses.InProgress) || !habit.DayResults.Any(dayResult => dayResult.Date == currentDay)) && IsHabitMatched(currentDay, habit)))
                                {
                                    for (int i = currentDay.Day; i >= 0; i--)
                                    {
                                        if (habit.RepeatDaysOfMonth.Contains(i))
                                            dataTable.Rows.Add(Guid.NewGuid(), habit.DayResults.First(dr => dr.Date == currentDay.AddDays(-i)).Progress, Statuses.Failed, currentDay.AddDays(-i), habitId);
                                    }
                                }
                                else if ((currentDay.Day != startDateDay) && IsHabitMatched(currentDay, habit))
                                    dataTable.Rows.Add(Guid.NewGuid(), habit.DayResults.First(dr => dr.Date == currentDay).Progress, Statuses.InProgress, currentDay, habitId);
                                else if (!IsHabitMatched(currentDay, habit))
                                    dataTable.Rows.Add(Guid.NewGuid(), 0, Statuses.None, currentDay, habitId);
                                currentDay = currentDay.AddDays(-1);
                            }
                            break;
                    };

                    
                }
                bulk.DestinationTableName = destinationTableName;
                await bulk.WriteToServerAsync(dataTable);

                await dbContext.Habits.Include(h => h.DayResults)
                .SelectMany(habit => habit.DayResults)
                .Where(dayResult => dayResult.Date < DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1))
                && dayResult.Status == Statuses.InProgress)
                .ExecuteUpdateAsync(dayResult => dayResult.SetProperty(d => d.Status, Statuses.Failed));
            }
            catch (Exception _)
            {
                await transaction.RollbackAsync();
            }
        }


        public async Task DeleteAsync(Habit habitToDelete, CancellationToken cancellationToken)
        {
            dbContext.Habits.Remove(habitToDelete);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAllAsync(IEnumerable<Habit> allHabits, CancellationToken cancellationToken)
        {
            dbContext.Habits.RemoveRange(allHabits); 
            await dbContext.SaveChangesAsync(cancellationToken);
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
