using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Core.Models
{
    public class DayResult
    {
        public int Progress { get; set; }
        public string Status { get; set; } = default!;
        public DateOnly Date { get; init; }
    }   
}
