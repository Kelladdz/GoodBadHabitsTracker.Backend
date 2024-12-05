using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodBadHabitsTracker.Application.Abstractions.Messaging;

namespace GoodBadHabitsTracker.Application.Commands.Habits.PostInProgressToday
{
    public sealed record PostInProgressTodayCommand() : ICommand<bool>;
}
