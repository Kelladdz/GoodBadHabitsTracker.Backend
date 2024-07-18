using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodBadHabitsTracker.Application.DTOs.Habit.Response;
using GoodBadHabitsTracker.Application.DTOs.Habit.Request;

namespace GoodBadHabitsTracker.Application.Queries.Habits.GetHabits
{
    public record GetHabitsQuery(GetHabitsRequest Request) : IRequest<HabitsResponse>;
}
