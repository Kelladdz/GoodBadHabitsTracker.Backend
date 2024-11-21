using GoodBadHabitsTracker.Core.Models;

namespace GoodBadHabitsTracker.Core.Interfaces
{
    public interface IEmailSender
    {
        Task SendWelcomeMessageAsync(User user, string email);
        Task SendPasswordResetLinkAsync(User user, string email, string resetLink, string token);
    }
}
