using System.Reflection;
using GoodBadHabitsTracker.Application.DTOs.Generic.Response;
using GoodBadHabitsTracker.Application.DTOs.Habit.Request;
using GoodBadHabitsTracker.Core.Models;

namespace GoodBadHabitsTracker.WebApi.Utils
{
    public static class EntityTypes
    {
        public static Dictionary<TypeInfo, List<TypeInfo>> ModelTypes
            => new()
            {
                { typeof(Habit).GetTypeInfo(), new() { typeof(HabitRequest).GetTypeInfo(), typeof(GenericResponse<Habit>).GetTypeInfo() } },
                { typeof(Group).GetTypeInfo(), new() { typeof(GroupRequest).GetTypeInfo(), typeof(GenericResponse<Group>).GetTypeInfo() } }
            };
    }
}
