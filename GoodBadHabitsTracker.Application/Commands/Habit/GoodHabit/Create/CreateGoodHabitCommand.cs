using GoodBadHabitsTracker.Application.DTOs.Habit.Request;
using GoodBadHabitsTracker.Application.DTOs.Habit.Response;
using GoodBadHabitsTracker.Application.Abstractions.Messaging;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.Commands.Habit.GoodHabit.Create
{
    public record CreateGoodHabitCommand(HabitRequest Request) : ICommand<GoodHabitResponse>;
}
