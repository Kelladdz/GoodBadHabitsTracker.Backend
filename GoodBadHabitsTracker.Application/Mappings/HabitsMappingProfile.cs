using AutoMapper;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.Application.DTOs.Response;
using GoodBadHabitsTracker.Application.DTOs.Request;

namespace GoodBadHabitsTracker.Application.Mappings
{
    public sealed class HabitsMappingProfile : Profile
    {
        public HabitsMappingProfile()
        {
            CreateMap<HabitRequest, Habit>();
            CreateMap<GroupRequest, Group>();
            CreateMap<ExternalLoginRequest, GetExternalTokensResponse>();
        }
    }
}
