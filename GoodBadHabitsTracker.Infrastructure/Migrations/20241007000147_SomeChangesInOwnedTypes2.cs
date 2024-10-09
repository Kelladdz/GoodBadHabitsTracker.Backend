using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoodBadHabitsTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SomeChangesInOwnedTypes2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_DayResults",
                table: "DayResults");

            migrationBuilder.DropIndex(
                name: "IX_DayResults_HabitId",
                table: "DayResults");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Comments",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_HabitId",
                table: "Comments");

            migrationBuilder.AlterColumn<bool>(
                name: "IsTimeBased",
                table: "Habits",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_DayResults",
                table: "DayResults",
                columns: new[] { "HabitId", "Id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Comments",
                table: "Comments",
                columns: new[] { "HabitId", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_DayResults",
                table: "DayResults");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Comments",
                table: "Comments");

            migrationBuilder.AlterColumn<bool>(
                name: "IsTimeBased",
                table: "Habits",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DayResults",
                table: "DayResults",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Comments",
                table: "Comments",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_DayResults_HabitId",
                table: "DayResults",
                column: "HabitId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_HabitId",
                table: "Comments",
                column: "HabitId");
        }
    }
}
