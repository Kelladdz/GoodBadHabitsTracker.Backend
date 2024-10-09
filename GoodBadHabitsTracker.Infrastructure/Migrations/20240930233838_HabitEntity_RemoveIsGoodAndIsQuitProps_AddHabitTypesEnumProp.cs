using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoodBadHabitsTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class HabitEntity_RemoveIsGoodAndIsQuitProps_AddHabitTypesEnumProp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {



            migrationBuilder.DropIndex(
                name: "IX_Habits_Name",
                table: "Habits");

            migrationBuilder.DropColumn(
                name: "HabitType",
                table: "Habits");

            migrationBuilder.DropColumn(
                name: "IsGood",
                table: "Habits");

            migrationBuilder.DropColumn(
                name: "IsQuit",
                table: "Habits");

            migrationBuilder.DropColumn(
                name: "IsTimeBased",
                table: "Habits");


            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Habits",
                newName: "User Id");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "Habits",
                newName: "Start Date");

            migrationBuilder.RenameColumn(
                name: "ReminderTimes",
                table: "Habits",
                newName: "Reminder Times");

            migrationBuilder.RenameColumn(
                name: "IconPath",
                table: "Habits",
                newName: "Icon Path");





            migrationBuilder.AlterColumn<int>(
                name: "RepeatMode",
                table: "Habits",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "RepeatInterval",
                table: "Habits",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RepeatDaysOfWeek",
                table: "Habits",
                type: "nvarchar(100)",
                nullable: false,
                defaultValue: "[]",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RepeatDaysOfMonth",
                table: "Habits",
                type: "nvarchar(50)",
                nullable: false,
                defaultValue: "[]",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Habits",
                type: "nvarchar(50)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<int>(
                name: "Frequency",
                table: "Habits",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Reminder Times",
                table: "Habits",
                type: "nvarchar(50)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Icon Path",
                table: "Habits",
                type: "nvarchar(100)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "HabitType",
                table: "Habits",
                type: "int",
                nullable: false,
                defaultValue: 0);



        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Habits_AspNetUsers_User Id",
                table: "Habits");

            migrationBuilder.DropForeignKey(
                name: "FK_Habits_Groups_Group Id",
                table: "Habits");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Habits");

            migrationBuilder.RenameColumn(
                name: "User Id",
                table: "Habits",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "Start Date",
                table: "Habits",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "Reminder Times",
                table: "Habits",
                newName: "ReminderTimes");

            migrationBuilder.RenameColumn(
                name: "Is Time Based",
                table: "Habits",
                newName: "LimitHabit_IsTimeBased");

            migrationBuilder.RenameColumn(
                name: "Icon Path",
                table: "Habits",
                newName: "IconPath");

            migrationBuilder.RenameColumn(
                name: "Group Id",
                table: "Habits",
                newName: "GroupId");

            migrationBuilder.RenameIndex(
                name: "IX_Habits_User Id",
                table: "Habits",
                newName: "IX_Habits_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Habits_Group Id",
                table: "Habits",
                newName: "IX_Habits_GroupId");

            migrationBuilder.AlterColumn<int>(
                name: "RepeatMode",
                table: "Habits",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "RepeatInterval",
                table: "Habits",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "RepeatDaysOfWeek",
                table: "Habits",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)");

            migrationBuilder.AlterColumn<string>(
                name: "RepeatDaysOfMonth",
                table: "Habits",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Habits",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)");

            migrationBuilder.AlterColumn<int>(
                name: "Frequency",
                table: "Habits",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "ReminderTimes",
                table: "Habits",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IconPath",
                table: "Habits",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)");

            migrationBuilder.AlterColumn<Guid>(
                name: "GroupId",
                table: "Habits",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "HabitType",
                table: "Habits",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsGood",
                table: "Habits",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsQuit",
                table: "Habits",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsTimeBased",
                table: "Habits",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LimitHabit_Frequency",
                table: "Habits",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LimitHabit_Quantity",
                table: "Habits",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Habits_Name",
                table: "Habits",
                column: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_Habits_AspNetUsers_UserId",
                table: "Habits",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Habits_Groups_GroupId",
                table: "Habits",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id");
        }
    }
}
