using LanguageExt.Common;
using GoodBadHabitsTracker.Core.Enums;
using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.Infrastructure.Persistance;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using GoodBadHabitsTracker.Application.Exceptions;
using System.Net;
using GoodBadHabitsTracker.Infrastructure.Utils;
using Microsoft.Extensions.Logging;
namespace GoodBadHabitsTracker.Application.Services
{
    public class FillPastDaysService(IHabitsDbContext dbContext, ILogger<FillPastDaysService> logger) : IFillPastDaysService
    {
        public async Task<Result<bool>> UpdateAllAsync()
        {
            logger.LogDebug("Filling empty days in the past...");
            logger.LogDebug("Connecting with database...");
            var connectionString = dbContext.GetConnectionString();
            using var bulk = new SqlBulkCopy(connectionString, SqlBulkCopyOptions.Default);
            dbContext.BeginTransaction();
            logger.LogDebug("Connected with database!");
            try
            {
                logger.LogDebug("Collecting all habits!");
                var habits = await dbContext.Habits
                    .Include(h => h.DayResults)
                    .ToListAsync();

                logger.LogDebug("Habits collected!");
                logger.LogDebug("Creating new day results table...");
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

                logger.LogDebug("Table created!");

                logger.LogDebug("Adding day result rows...");
                foreach (var habit in habits)
                {
                    logger.LogDebug("Habit: {name}", habit.Name);
                    var currentDay = habit.StartDate;
                    var tommorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
                    var habitId = habit.Id;

                    while (currentDay <= tommorrow)
                    {
                        logger.LogDebug("{currentDay}", currentDay);

                        var duplicatedByDate = habit.DayResults.Any(dr => dr.Date == currentDay);
                        logger.LogDebug("Result isn't duplicated by date: {duplicatedByDate}", !duplicatedByDate);

                        var isHabitMatched = Conditions.IsHabitMatched(currentDay, habit);
                        logger.LogDebug("Is habit matched: {isHabitMatched}", isHabitMatched);

                        if (!duplicatedByDate && isHabitMatched)
                        {
                            logger.LogDebug("Conditions are met!");
                            logger.LogDebug("Adding row...");
                            dataTable.Rows.Add(
                                Guid.NewGuid(),
                                habit.HabitType == HabitTypes.Quit ? null : 0,
                                Statuses.InProgress,
                                currentDay,
                                habitId);
                            logger.LogDebug("Row added!");
                        }
                        else logger.LogDebug("Conditions aren't met!");
                        currentDay = currentDay.AddDays(1);
                    }                
                }

                logger.LogDebug("Writing rows in database...");
                bulk.DestinationTableName = destinationTableName;
                await bulk.WriteToServerAsync(dataTable);
                logger.LogDebug("Rows was written");

                logger.LogDebug("Save and commit changes...");
                await dbContext.CommitAsync();
                logger.LogDebug("Saved!");
                return new Result<bool>(true);
            }
            catch (Exception ex)
            {
                await dbContext.RollbackAsync();
                return new Result<bool>(new AppException(HttpStatusCode.InternalServerError, ex.Message));
            }
        }
    }
}
