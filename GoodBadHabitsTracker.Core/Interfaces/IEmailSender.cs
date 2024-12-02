using GoodBadHabitsTracker.Core.Models;

namespace GoodBadHabitsTracker.Core.Interfaces
{
    public interface IEmailSender
    {
        Task SendConfirmationLinkAsync(User user, string link);
        Task SendPasswordResetLinkAsync(User user, string link);
        Task SendMessageAfterNewHabitCreateAsync(User user, Habit habit);
    }
}
