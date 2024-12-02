using Amazon.S3.Model;
using GoodBadHabitsTracker.Core.Enums;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.Infrastructure.Persistance;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace GoodBadHabitsTracker.Infrastructure.Repositories
{
    public sealed class DayResultsRepository(HabitsDbContext dbContext) : IDayResultsRepository
    {
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
                        if (currentDay == DateOnly.FromDateTime(DateTime.Today) && !habit.DayResults.Any(dayResult => dayResult.Date == currentDay))
                            dataTable.Rows.Add(Guid.NewGuid(), 0, Statuses.InProgress, currentDay, habitId);
                        else if (!habit.DayResults.Any(dayResult => dayResult.Date == currentDay))
                            dataTable.Rows.Add(Guid.NewGuid(), 0, Statuses.Failed, currentDay, habitId);
                        currentDay = currentDay.AddDays(-1);
                    }
                }
                

                

                

               




                bulk.DestinationTableName = destinationTableName;
                await bulk.WriteToServerAsync(dataTable);

                await dbContext.Habits.Include(h => h.DayResults)
                .SelectMany(habit => habit.DayResults)
                .Where(dayResult => dayResult.Date == DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1))
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
    }
}
