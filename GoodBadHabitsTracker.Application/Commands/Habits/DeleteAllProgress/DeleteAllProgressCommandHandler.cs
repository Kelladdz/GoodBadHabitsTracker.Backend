using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Core.Interfaces;
using MediatR;
using LanguageExt.Common;
using System.Net;
using Microsoft.EntityFrameworkCore;
using GoodBadHabitsTracker.Infrastructure.Persistance;
using Microsoft.Data.SqlClient;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.Core.Enums;
using System.Data;

namespace GoodBadHabitsTracker.Application.Commands.Habits.DeleteAllProgress
{
    internal sealed class DeleteAllProgressCommandHandler(
        IHabitsDbContext dbContext,
        IUserAccessor userAccessor) : IRequestHandler<DeleteAllProgressCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(DeleteAllProgressCommand command, CancellationToken cancellationToken)
        {
            var user = await userAccessor.GetCurrentUser();
            if (user == null)
                return new Result<bool>(new AppException(HttpStatusCode.Unauthorized, "User Not Found"));
            var userId = user.Id;

            var connectionString = dbContext.GetConnectionString();
            using var bulk = new SqlBulkCopy(connectionString, SqlBulkCopyOptions.Default);
            dbContext.BeginTransaction();

            try
            {
                var allHabits = await dbContext.ReadAllHabitsAsync(userId);
                if (!allHabits.Any())
                {
                    await dbContext.CommitAsync();
                    return new Result<bool>(true);
                }
                var currentDay = DateOnly.FromDateTime(DateTime.Today);
                foreach (var habit in allHabits!)
                {
                    habit.DayResults.Clear();
                    habit.StartDate = currentDay;
                }

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

                await dbContext.CommitAsync();
                return new Result<bool>(true);
            }
            catch (Exception ex)
            {
                await dbContext.RollbackAsync();
                return new Result<bool>(new AppException(HttpStatusCode.BadRequest, ex.Message));
            }
        }
    }
}
