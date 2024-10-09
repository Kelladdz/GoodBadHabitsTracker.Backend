using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoodBadHabitsTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStoredProcedureForGetHabitByIdEndpoint : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            const string procedure = @"
                CREATE PROCEDURE [dbo].[GetHabitById]
                (@Id uniqueidentifier)
                AS BEGIN
                    SET NOCOUNT ON;
                    SELECT * FROM [dbo].[Habits]
                    WHERE [dbo].[Habits].[Id] = @Id
                END";

            migrationBuilder.Sql(procedure);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            string dropProcedure = @"
                DROP PROCEDURE [dbo].[GetHabitById]";

            migrationBuilder.Sql(dropProcedure);
        }
    }
}
