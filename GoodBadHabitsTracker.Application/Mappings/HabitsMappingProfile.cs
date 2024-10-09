using AutoMapper;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.Application.DTOs.Habit.Request;
using GoodBadHabitsTracker.Application.DTOs.Generic.Response;

namespace GoodBadHabitsTracker.Application.Mappings
{
    public sealed class HabitsMappingProfile : Profile
    {
        public HabitsMappingProfile()
        {
            CreateMap<HabitRequest, Habit>();
            CreateMap<Habit, GenericResponse<Habit>>();
        }
    }
}
