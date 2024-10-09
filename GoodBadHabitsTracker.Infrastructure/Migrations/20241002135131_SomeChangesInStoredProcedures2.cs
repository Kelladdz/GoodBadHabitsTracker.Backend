using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoodBadHabitsTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SomeChangesInStoredProcedures2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            const string DROP_GET_HABIT_BY_ID_PROCEDURE = @"
            IF OBJECT_ID('[dbo].[GetHabitById]', 'P') IS NOT NULL
            DROP PROCEDURE [dbo].[GetHabitById]";

            const string DROP_SEARCH_HABITS_PROCEDURE = @"
            IF OBJECT_ID('[dbo].[SearchHabits]', 'P') IS NOT NULL
            DROP PROCEDURE [dbo].[SearchHabits]";

            migrationBuilder.Sql(DROP_GET_HABIT_BY_ID_PROCEDURE);
            migrationBuilder.Sql(DROP_SEARCH_HABITS_PROCEDURE);

            const string GET_HABIT_BY_ID_PROCEDURE = @$"
                CREATE PROCEDURE [dbo].[GetHabitById]
                (@Id uniqueidentifier)
                AS BEGIN
                    SET NOCOUNT ON
                    SELECT * FROM [dbo].[Habits]
                WHERE Id = @Id
                END";

            const string SEARCH_HABITS_PROCEDURE = @$"
                CREATE PROCEDURE [dbo].[SearchHabits]
                (@UserId uniqueidentifier, @Term nvarchar(50), @Date date)
                AS
                BEGIN
                    SELECT *
                    FROM 
                        [GBHTdb].[dbo].[Habits]
                    WHERE 
                        StartDate <= @Date
                        AND UserId = @UserId
                        AND (
                            HabitType <> 1 OR
                            (
                                HabitType = 1 AND
                                (
                                    (RepeatMode = 1 AND CHARINDEX(DATENAME(WEEKDAY, @Date), RepeatDaysOfWeek) > 0) OR
                                    (RepeatMode = 2 AND CHARINDEX(CAST(DAY(@Date) AS VARCHAR), RepeatDaysOfMonth) > 0) OR
                                    (RepeatMode = 3 AND DATEDIFF(DAY, StartDate, @Date) % RepeatInterval = 0)
                                )
                            )
                        )
                        AND (
                            @Term IS NULL OR @Term = '' OR LOWER (Name) LIKE '%' + @Term + '%'
                        )
                        AND (
                            Quantity IS NULL OR Quantity IS NOT NULL
                        )
                        AND (
                            IsTimeBased IS NULL OR IsTimeBased IS NOT NULL
                        )
                END";

            migrationBuilder.Sql(GET_HABIT_BY_ID_PROCEDURE);
            migrationBuilder.Sql(SEARCH_HABITS_PROCEDURE);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            string DROP_GET_HABIT_BY_ID_PROCEDURE = @"
                IF OBJECT_ID('[dbo].[GetHabitById]', 'P') IS NOT NULL
                DROP PROCEDURE [dbo].[GetHabitById]";

            migrationBuilder.Sql(DROP_GET_HABIT_BY_ID_PROCEDURE);

            string DROP_SEARCH_HABITS_PROCEDURE = @"
                IF OBJECT_ID('[dbo].[SearchHabits]', 'P', 'P1', 'P2') IS NOT NULL
                DROP PROCEDURE [dbo].[SearchHabits]";

            migrationBuilder.Sql(DROP_SEARCH_HABITS_PROCEDURE);
        }
    }
}
