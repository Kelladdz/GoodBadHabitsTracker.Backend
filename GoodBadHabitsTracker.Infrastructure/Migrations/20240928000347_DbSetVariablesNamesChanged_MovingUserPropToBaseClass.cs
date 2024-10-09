using System;
using GoodBadHabitsTracker.Core.Enums;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoodBadHabitsTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DbSetVariablesNamesChanged_MovingUserPropToBaseClass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quit Habits_AspNetUsers_UserId",
                table: "Quit Habits");

            migrationBuilder.DropForeignKey(
                name: "FK_Quit Habits_Groups_GroupId",
                table: "Quit Habits");

            migrationBuilder.DropTable(
                name: "Good Habits_Comments");

            migrationBuilder.DropTable(
                name: "Good Habits_DayResults");

            migrationBuilder.DropTable(
                name: "Limit Habits_Comments");

            migrationBuilder.DropTable(
                name: "Limit Habits_DayResults");

            migrationBuilder.DropTable(
                name: "Quit Habits_Comments");

            migrationBuilder.DropTable(
                name: "Quit Habits_DayResults");

            migrationBuilder.DropTable(
                name: "Good Habits");

            migrationBuilder.DropTable(
                name: "Limit Habits");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Quit Habits",
                table: "Quit Habits");

            migrationBuilder.DropTable(
                name: "Quit Habits");

            migrationBuilder.CreateTable(
                name: "Habits",
                columns: table => new
                {
                    IsQuit = table.Column<bool>(type: "bit", nullable: false),
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IconPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsGood = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    GroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Frequency = table.Column<int>(type: "int", nullable: false),
                    IsTimeBased = table.Column<bool>(type: "bit", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    ReminderTimes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RepeatDaysOfMonth = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RepeatDaysOfWeek = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RepeatInterval = table.Column<int>(type: "int", nullable: false),
                    RepeatMode = table.Column<int>(type: "int", nullable: false),
                    HabitTypes = table.Column<int>(type: "int", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Habits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Habits_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Habits_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Habits_Name",
                table: "Habits",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Habits_UserId",
                table: "Habits",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Habits_GroupId",
                table: "Habits",
                column: "GroupId");

            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "DayResult");

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    HabitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x =>  x.Id);
                    table.ForeignKey(
                        name: "FK_Comments_Habits_HabitId",
                        column: x => x.HabitId,
                        principalTable: "Habits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DayResults",
                columns: table => new
                {
                    HabitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Progress = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DayResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DayResults_Habits_HabitId",
                        column: x => x.HabitId,
                        principalTable: "Habits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Habits_AspNetUsers_UserId",
                table: "Habits");

            migrationBuilder.DropForeignKey(
                name: "FK_Habits_Groups_GroupId",
                table: "Habits");

            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "DayResult");



            

            migrationBuilder.DropTable(
                name: "Habits");

            migrationBuilder.CreateIndex(
                name: "IX_Quit Habits_UserId",
                table: "Quit Habits",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Habits_Name",
                table: "Quit Habits",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Habits_GroupId",
                table: "Quit Habits",
                column: "GroupId");



            migrationBuilder.AddPrimaryKey(
                name: "PK_Quit Habits",
                table: "Quit Habits",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Good Habits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IconPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsGood = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    GroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Frequency = table.Column<int>(type: "int", nullable: false),
                    IsTimeBased = table.Column<bool>(type: "bit", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    ReminderTimes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RepeatDaysOfMonth = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RepeatDaysOfWeek = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RepeatInterval = table.Column<int>(type: "int", nullable: false),
                    RepeatMode = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Good Habits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Good Habits_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Good Habits_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Limit Habits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IconPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsGood = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    GroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Frequency = table.Column<int>(type: "int", nullable: false),
                    IsQuit = table.Column<bool>(type: "bit", nullable: false),
                    IsTimeBased = table.Column<bool>(type: "bit", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Limit Habits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Limit Habits_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Limit Habits_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Quit Habits_Comments",
                columns: table => new
                {
                    QuitHabitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quit Habits_Comments", x => new { x.QuitHabitId, x.Id });
                    table.ForeignKey(
                        name: "FK_Quit Habits_Comments_Quit Habits_QuitHabitId",
                        column: x => x.QuitHabitId,
                        principalTable: "Quit Habits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Quit Habits_DayResults",
                columns: table => new
                {
                    QuitHabitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Progress = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quit Habits_DayResults", x => new { x.QuitHabitId, x.Id });
                    table.ForeignKey(
                        name: "FK_Quit Habits_DayResults_Quit Habits_QuitHabitId",
                        column: x => x.QuitHabitId,
                        principalTable: "Quit Habits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Good Habits_Comments",
                columns: table => new
                {
                    GoodHabitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Good Habits_Comments", x => new { x.GoodHabitId, x.Id });
                    table.ForeignKey(
                        name: "FK_Good Habits_Comments_Good Habits_GoodHabitId",
                        column: x => x.GoodHabitId,
                        principalTable: "Good Habits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Good Habits_DayResults",
                columns: table => new
                {
                    GoodHabitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Progress = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Good Habits_DayResults", x => new { x.GoodHabitId, x.Id });
                    table.ForeignKey(
                        name: "FK_Good Habits_DayResults_Good Habits_GoodHabitId",
                        column: x => x.GoodHabitId,
                        principalTable: "Good Habits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Limit Habits_Comments",
                columns: table => new
                {
                    LimitHabitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Limit Habits_Comments", x => new { x.LimitHabitId, x.Id });
                    table.ForeignKey(
                        name: "FK_Limit Habits_Comments_Limit Habits_LimitHabitId",
                        column: x => x.LimitHabitId,
                        principalTable: "Limit Habits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Limit Habits_DayResults",
                columns: table => new
                {
                    LimitHabitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Progress = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Limit Habits_DayResults", x => new { x.LimitHabitId, x.Id });
                    table.ForeignKey(
                        name: "FK_Limit Habits_DayResults_Limit Habits_LimitHabitId",
                        column: x => x.LimitHabitId,
                        principalTable: "Limit Habits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Good Habits_GroupId",
                table: "Good Habits",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Good Habits_Name",
                table: "Good Habits",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Good Habits_UserId",
                table: "Good Habits",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Limit Habits_GroupId",
                table: "Limit Habits",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Limit Habits_Name",
                table: "Limit Habits",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Limit Habits_UserId",
                table: "Limit Habits",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quit Habits_AspNetUsers_UserId",
                table: "Quit Habits",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Quit Habits_Groups_GroupId",
                table: "Quit Habits",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id");
        }
    }
}
