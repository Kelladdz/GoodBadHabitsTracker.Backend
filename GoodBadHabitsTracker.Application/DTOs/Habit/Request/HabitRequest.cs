using GoodBadHabitsTracker.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.DTOs.Habit.Request
{
    public class HabitRequest
    {
        public string? Name { get; init; }
        public string? IconPath { get; init; }
        public bool IsGood { get; init; }
        public bool? IsQuit { get; init; }
        public DateOnly StartDate { get; init; }
        public bool? IsTimeBased { get; init; }
        public int? Quantity { get; init; }
        public Frequencies? Frequency { get; init; }
        public RepeatModes? RepeatMode { get; init; }
        public string[] RepeatDaysOfWeek { get; set; }
        public int[] RepeatDaysOfMonth { get; set; }
        public int RepeatInterval { get; set; }
        public TimeOnly[] ReminderTimes { get; init; }
    }
}
