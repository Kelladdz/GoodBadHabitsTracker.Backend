using GoodBadHabitsTracker.Application.Abstractions.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.Commands.Habits.DeleteAllProgress
{
    public sealed record DeleteAllProgressCommand() : ICommand<bool>;
}
