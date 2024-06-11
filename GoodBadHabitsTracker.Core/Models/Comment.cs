using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Core.Models
{
    public class Comment
    {
        public string Body { get; init; } = default!;
        public DateOnly Date { get; init; }
    }
}
