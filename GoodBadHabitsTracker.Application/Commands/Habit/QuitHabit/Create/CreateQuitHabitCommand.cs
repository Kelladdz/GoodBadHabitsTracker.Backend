using GoodBadHabitsTracker.Application.Abstractions.Messaging;
using GoodBadHabitsTracker.Application.DTOs.Habit.Request;
using GoodBadHabitsTracker.Application.DTOs.Habit.Response;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.Commands.Habit.QuitHabit.Create
{
    public record CreateQuitHabitCommand(CreateHabitRequest Request) : ICommand<QuitHabitResponse>;
}

