using GoodBadHabitsTracker.Core.Models;

namespace GoodBadHabitsTracker.Core.Interfaces
{
    public interface IEmailSender
    {
        void SendConfirmationLink(User user, string link);
        void SendPasswordResetLink(User user, string link);
        void SendMessageAfterNewHabitCreate(User user, Habit habit);
    }
}
