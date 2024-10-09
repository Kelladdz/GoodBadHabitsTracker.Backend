using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoodBadHabitsTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStoredProcedureForGetAllHabitsEndpoint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            const string procedure = @"
                CREATE PROCEDURE [dbo].[GetUserHabits]
                (@UserId uniqueidentifier)
                AS BEGIN
                    SET NOCOUNT ON;
                    SELECT * FROM [dbo].[Habits]
                    WHERE [dbo].[Habits].[UserId] = @UserId
                END";

            migrationBuilder.Sql(procedure);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            string dropProcedure = @"
                DROP PROCEDURE [dbo].[GetUserHabits]";

            migrationBuilder.Sql(dropProcedure);
        }
    }
}
