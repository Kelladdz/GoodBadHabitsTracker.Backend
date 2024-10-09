using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoodBadHabitsTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangedStoredProcedureForGetAllUserHabitsQuery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            const string DROP_PROCEDURE = (@"
            IF OBJECT_ID('[dbo].[GetUserHabits]', 'P') IS NOT NULL
            DROP PROCEDURE [dbo].[GetUserHabits]");

            migrationBuilder.Sql(DROP_PROCEDURE);

            const string GET_USER_HABITS_PROCEDURE = @"
            CREATE PROCEDURE [dbo].[GetUserHabits]
            (@UserId UNIQUEIDENTIFIER, @Term NVARCHAR(50))
            AS
            BEGIN

                SELECT *
                FROM [GBHTdb].[dbo].[Habits]
                WHERE StartDate <= CAST(GETDATE() AS DATE)
                AND 
                UserId = @UserId
                AND
                (
                    HabitType <> 'Good' OR
                    (
		                HabitType = 'Good' AND
                        (
			                (RepeatMode = 0 AND CHARINDEX(DATENAME(WEEKDAY, CAST(GETDATE() AS DATE)), RepeatDaysOfWeek) > 0) OR
			                (RepeatMode = 1 AND CHARINDEX(CAST(DAY(GETDATE()) AS VARCHAR), RepeatDaysOfMonth) > 0) OR
			                (RepeatMode = 2 AND DATEDIFF(DAY, StartDate, CAST(GETDATE() AS DATE)) % RepeatInterval = 0)
                        )
                    )
                )
                AND (
                  @Term IS NULL OR @Term = '' OR LOWER(Name) LIKE '%' + @Term + '%'
                );
            END";

            migrationBuilder.Sql(GET_USER_HABITS_PROCEDURE);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            string DROP_PROCEDURE = @"
                DROP PROCEDURE [dbo].[GetUserHabits]";

            migrationBuilder.Sql(DROP_PROCEDURE);

            string GET_USER_HABITS_PROCEDURE = @"
                CREATE PROCEDURE [dbo].[GetUserHabits]
                (@UserId uniqueidentifier)
                AS BEGIN
                    SET NOCOUNT ON;
                    SELECT * FROM [dbo].[Habits]
                    WHERE [dbo].[Habits].[UserId] = @UserId
                END";

            migrationBuilder.Sql(GET_USER_HABITS_PROCEDURE);
        }
    }
}
