using GoodBadHabitsTracker.Application.Abstractions.Messaging;
using GoodBadHabitsTracker.Application.DTOs.Habit.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.Commands.Habit.LimitHabit.Edit
{
    public record EditLimitHabitCommand(EditHabitRequest Request) : ICommand;
}
