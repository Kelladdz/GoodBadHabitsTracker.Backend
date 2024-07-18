using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.DTOs.Habit.Request
{
    public class GetHabitsRequest
    {
        public string? Term { get; set; }
        public DateOnly Date {  get; set; }
    }
}
