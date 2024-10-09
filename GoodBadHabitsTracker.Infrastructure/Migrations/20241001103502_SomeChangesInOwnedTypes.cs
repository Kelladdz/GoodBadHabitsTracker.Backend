using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoodBadHabitsTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SomeChangesInOwnedTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Habits_HabitId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_DayResults_Habits_UserId",
                table: "DayResults");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Comments");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "DayResults",
                newName: "HabitId");

            migrationBuilder.RenameIndex(
                name: "IX_DayResults_UserId",
                table: "DayResults",
                newName: "IX_DayResults_HabitId");

            migrationBuilder.AlterColumn<Guid>(
                name: "HabitId",
                table: "Comments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Habits_HabitId",
                table: "Comments",
                column: "HabitId",
                principalTable: "Habits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DayResults_Habits_HabitId",
                table: "DayResults",
                column: "HabitId",
                principalTable: "Habits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Habits_HabitId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_DayResults_Habits_HabitId",
                table: "DayResults");

            migrationBuilder.RenameColumn(
                name: "HabitId",
                table: "DayResults",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_DayResults_HabitId",
                table: "DayResults",
                newName: "IX_DayResults_UserId");

            migrationBuilder.AlterColumn<Guid>(
                name: "HabitId",
                table: "Comments",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Comments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Habits_HabitId",
                table: "Comments",
                column: "HabitId",
                principalTable: "Habits",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DayResults_Habits_UserId",
                table: "DayResults",
                column: "UserId",
                principalTable: "Habits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
