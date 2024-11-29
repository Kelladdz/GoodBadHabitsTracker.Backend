using AutoMapper;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.Application.DTOs.Habit.Request;
using GoodBadHabitsTracker.Application.DTOs.Generic.Response;
using GoodBadHabitsTracker.Application.DTOs.Auth.Request;
using GoodBadHabitsTracker.Application.DTOs.Auth.Response;

namespace GoodBadHabitsTracker.Application.Mappings
{
    public sealed class HabitsMappingProfile : Profile
    {
        public HabitsMappingProfile()
        {
            CreateMap<HabitRequest, Habit>();
            CreateMap<Habit, GenericResponse<Habit>>();
            CreateMap<GroupRequest, Group>();
            CreateMap<Group, GenericResponse<Group>>();
            CreateMap<ExternalLoginRequest, GetExternalTokensResponse>();

        }
    }
}
