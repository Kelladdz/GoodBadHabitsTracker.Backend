using AutoMapper;
using GoodBadHabitsTracker.Application.DTOs.Habit;
using GoodBadHabitsTracker.Core.Models.Habit;
using GoodBadHabitsTracker.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodBadHabitsTracker.Application.DTOs.Habit.Request;

namespace GoodBadHabitsTracker.Application.Mappings
{
    public class HabitsMappingProfile : Profile
    {
        public HabitsMappingProfile()
        {
            CreateMap<HabitRequest, GoodHabit>();
            CreateMap<HabitRequest, LimitHabit>();
            CreateMap<HabitRequest, QuitHabit>();
        }
    }
}
