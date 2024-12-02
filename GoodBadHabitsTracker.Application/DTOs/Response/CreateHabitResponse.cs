using GoodBadHabitsTracker.Core.Models;

namespace GoodBadHabitsTracker.Application.DTOs.Response
{
    public record CreateHabitResponse(Habit Habit, User User);
}
