using AutoMapper;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.Application.DTOs.Request;
using GoodBadHabitsTracker.Core.Enums;

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
                .ForMember(x => x.Frequency, opt =>
                {
                    opt.MapFrom(x => x.Frequency);
                    opt.Condition(src => src.HabitType != HabitTypes.Quit);
                })
                .ForMember(x => x.Quantity, opt =>
                {
                    opt.MapFrom((src, dest) => (bool)src.IsTimeBased! ? src.Quantity * 60 : src.Quantity);
                    opt.Condition(src => src.HabitType != HabitTypes.Quit);
                })
                .ForMember(x => x.Quantity, opt =>
                {
                    opt.MapFrom((src, dest) => src.HabitType == HabitTypes.Quit ? src.Quantity = null : src.Quantity);
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
                    opt.MapFrom((src, dest) => src.RepeatMode == RepeatModes.Monthly ? src.RepeatDaysOfMonth : []);
                })
                .ForMember(x => x.RepeatDaysOfWeek, opt =>
                {
                    opt.MapFrom((src, dest) => src.RepeatMode == RepeatModes.Daily ? src.RepeatDaysOfWeek : []);
                })
                .ForMember(x => x.RepeatInterval, opt =>
                {
                    opt.MapFrom((src, dest) => src.RepeatMode == RepeatModes.Interval ? src.RepeatInterval : 0);
                })
                .ForMember(x => x.ReminderTimes, opt => opt.MapFrom(x => x.ReminderTimes))
                .ForMember(x => x.GroupId, opt => opt.MapFrom(x => x.GroupId));

            CreateMap<CreateCommentRequest, Comment>();
        }
    }
}