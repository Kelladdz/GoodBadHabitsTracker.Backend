using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoodBadHabitsTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Aggregate_AllHabitEntitiesToOneMainEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AlterColumn<string>(
                name: "ReminderTimes",
                table: "Habits",
                type: "nvarchar(96)",
                nullable: false,
                defaultValue: "[]",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldNullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Habits_HabitId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_DayResults_Habits_UserId",
                table: "DayResults");

            migrationBuilder.DropForeignKey(
                name: "FK_Habits_AspNetUsers_UserId",
                table: "Habits");

            migrationBuilder.DropForeignKey(
                name: "FK_Habits_Groups_GroupId",
                table: "Habits");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Comments");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Habits",
                newName: "User Id");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "Habits",
                newName: "Start Date");

            migrationBuilder.RenameColumn(
                name: "IconPath",
                table: "Habits",
                newName: "Icon Path");

            migrationBuilder.RenameColumn(
                name: "GroupId",
                table: "Habits",
                newName: "Group Id");

            migrationBuilder.RenameColumn(
                name: "ReminderTimes",
                table: "Habits",
                newName: "Reminder Times");

            migrationBuilder.RenameColumn(
                name: "IsTimeBased",
                table: "Habits",
                newName: "Is Time Based");

            migrationBuilder.RenameColumn(
                name: "HabitType",
                table: "Habits",
                newName: "Type");

            migrationBuilder.RenameIndex(
                name: "IX_Habits_UserId",
                table: "Habits",
                newName: "IX_Habits_User Id");

            migrationBuilder.RenameIndex(
                name: "IX_Habits_GroupId",
                table: "Habits",
                newName: "IX_Habits_Group Id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "DayResults",
                newName: "HabitId");

            migrationBuilder.RenameIndex(
                name: "IX_DayResults_UserId",
                table: "DayResults",
                newName: "IX_DayResults_HabitId");

            migrationBuilder.AlterColumn<string>(
                name: "Reminder Times",
                table: "Habits",
                type: "nvarchar(50)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(96)");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Habits_AspNetUsers_User Id",
                table: "Habits",
                column: "User Id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Habits_Groups_Group Id",
                table: "Habits",
                column: "Group Id",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
