using AutoMapper;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.Application.DTOs.Response;
using GoodBadHabitsTracker.Application.DTOs.Request;
using GoodBadHabitsTracker.Core.Enums;
using AutoMapper.Extensions.EnumMapping;

namespace GoodBadHabitsTracker.Application.Mappings
{
    public sealed class HabitsMappingProfile : Profile
    {
        public HabitsMappingProfile()
        {
            CreateMap<HabitRequest, Habit>()
                .ForMember(x => x.Name, opt => opt.MapFrom(x => x.Name))
                .ForMember(x => x.HabitType, opt => opt.MapFrom(x => x.HabitType))
                .ForMember(x => x.IconId, opt => opt.MapFrom(x => x.IconId))
                .ForMember(x => x.StartDate, opt => opt.MapFrom(x => x.StartDate))
                .ForMember(x => x.IsTimeBased, opt =>
                {
                    opt.MapFrom(x => x.IsTimeBased);
                    opt.Condition(src => src.HabitType != HabitTypes.Quit);
                })
                .ForMember(x => x.Quantity, opt =>
                {
                    opt.MapFrom((src, dest) => (bool)src.IsTimeBased! ? src.Quantity * 60 : src.Quantity);
                    opt.Condition(src => src.HabitType != HabitTypes.Quit);
                })
                .ForMember(x => x.Frequency, opt =>
                {
                    opt.MapFrom(x => x.Frequency);
                    opt.Condition(src => src.HabitType != HabitTypes.Quit);
                })
                .ForMember(x => x.RepeatMode, opt =>
                {
                    opt.MapFrom(src =>
                        src.HabitType != HabitTypes.Good 
                            ? RepeatModes.NonApplicable 
                            : src.RepeatMode);
                })
                .ForMember(x => x.RepeatDaysOfMonth, opt =>
                {
                    opt.MapFrom(x => x.RepeatDaysOfMonth);
                    opt.Condition(src => src.HabitType == HabitTypes.Good && src.RepeatMode == RepeatModes.Monthly);
                })
                .ForMember(x => x.RepeatDaysOfWeek, opt =>
                {
                    opt.MapFrom(x => x.RepeatDaysOfWeek);
                    opt.Condition(src => src.HabitType == HabitTypes.Good && src.RepeatMode == RepeatModes.Daily);
                })
                .ForMember(x => x.RepeatInterval, opt =>
                {
                    opt.MapFrom(x => x.RepeatInterval);
                    opt.Condition(src => src.HabitType == HabitTypes.Good && src.RepeatMode == RepeatModes.Interval);
                })
                .ForMember(x => x.ReminderTimes, opt => opt.MapFrom(x => x.ReminderTimes))
                .ForMember(x => x.GroupId, opt => opt.MapFrom(x => x.GroupId));



            CreateMap<ExternalLoginRequest, GetExternalTokensResponse>();
        }
    }
}
