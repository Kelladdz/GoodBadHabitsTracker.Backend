using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoodBadHabitsTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStoredProcedureForInsertHabit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            const string procedure = @"
                CREATE PROCEDURE [dbo].[InsertHabit]
                (@Name nvarchar(50), @IconPath varchar(100), 
                @HabitType int, @StartDate date, @IsTimeBased bit
                @Quantity int, @Frequency int
                @RepeatModes int, @RepeatInterval int
                @ReminderTimes nvarchar(96))
                AS BEGIN
                    SET NOCOUNT ON;
                    INSERT INTO [dbo].[Habits] ([Name], [IconPath], [HabitType], [StartDate], [IsTimeBased], [Quantity], [Frequency], [RepeatModes], [RepeatInterval], [ReminderTimes])
                    VALUES (@Name, @IconPath, @HabitType, @StartDate, @IsTimeBased, @Quantity, @Frequency, @RepeatModes, @RepeatInterval, @ReminderTimes)
                END";

            migrationBuilder.Sql(procedure);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            string dropProcedure = @"
                DROP PROCEDURE [dbo].[InsertHabit]";

            migrationBuilder.Sql(dropProcedure);
        }
    }
}
