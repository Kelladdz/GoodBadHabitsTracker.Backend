using GoodBadHabitsTracker.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.DTOs.Habit.Request
{
    public class CreateHabitRequest
    {
        public string? Name { get; set; }
        public string? IconPath { get; set; }
        public bool IsGood { get; set; }
        public bool? IsQuit { get; set; }
        public DateOnly StartDate { get; set; }
        public bool? IsTimeBased { get; set; }
        public int? Quantity { get; set; }
        public Frequencies? Frequency { get; set; }
        public RepeatModes? RepeatMode { get; set; }
        public string[] RepeatDaysOfWeek { get; set; }
        public int[] RepeatDaysOfMonth { get; set; }
        public int RepeatInterval { get; set; }
        public TimeOnly[] ReminderTimes { get; set; }
    }
}
