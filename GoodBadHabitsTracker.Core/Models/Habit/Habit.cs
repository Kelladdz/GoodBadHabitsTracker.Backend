using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GoodBadHabitsTracker.Core.Models.Habit
{
    public class Habit
    {
        public Guid Id { get; private init; } = Guid.NewGuid();
        public string Name { get; set; } = default!;
        public string IconPath { get; set; } = default!;
        public bool IsGood { get; init; }
        public DateOnly StartDate { get; set; }
        public User User { get; init; } = default!;
        public Guid UserId { get; init; }
        public List<DayResult> DayResults { get; init; } = new();
        public List<Comment> Comments { get; init; } = new();
    }
}
