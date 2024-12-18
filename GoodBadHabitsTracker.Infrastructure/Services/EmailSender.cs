using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Core.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using GoodBadHabitsTracker.Infrastructure.Settings;
using GoodBadHabitsTracker.Infrastructure.Persistance;


namespace GoodBadHabitsTracker.Infrastructure.Services
{
    public class EmailSender(IOptions<MailSettings> mailSettings, IWebHostEnvironment environment, IHabitsDbContext dbContext) : IEmailSender
    {
        public async Task SendConfirmationLinkAsync(User user, string link)
        {
            using (MimeMessage emailMessage = new MimeMessage())
            {
                var subject = EmailSubjects.WELCOME;
                var emailTemplate = await GetMessageDetailsAsync(user, emailMessage, subject);

                var userName = user.UserName;
                var body = emailTemplate?.Body.Replace("{userName}", userName).Replace("{link}", link);

                BuildEmailMessageBody(emailMessage, body!);
                SendEmail(emailMessage);
            }
        }
        public async Task SendPasswordResetLinkAsync(User user, string link)
        {
            using (MimeMessage emailMessage = new MimeMessage())
            {
                var subject = EmailSubjects.PASSWORD_RESET;
                var emailTemplate = await GetMessageDetailsAsync(user, emailMessage, subject);

                var userName = user.UserName;
                var body = emailTemplate?.Body.Replace("{userName}", userName).Replace("{link}", link);

                BuildEmailMessageBody(emailMessage, body!);
                SendEmail(emailMessage);
            }
        }

        public async Task SendMessageAfterNewHabitCreateAsync(User user, Habit habit)
        {
            using (MimeMessage emailMessage = new MimeMessage())
            {
                var subject = EmailSubjects.CONGRATULATIONS;
                var emailTemplate = await GetMessageDetailsAsync(user, emailMessage, subject);

                var userName = user.UserName;
                var body = emailTemplate?.Body.Replace("{userName}", userName);

                BuildEmailMessageBody(emailMessage, body!);
                SendEmail(emailMessage);
            }
        }
        private async Task<EmailTemplate?> GetMessageDetailsAsync(User user, MimeMessage emailMessage, string subject)
        {
            MailboxAddress emailFrom = new MailboxAddress(mailSettings.Value.DisplayName, mailSettings.Value.Email);
            emailMessage.From.Add(emailFrom);

            MailboxAddress emailTo = new MailboxAddress(user.UserName, user.Email);
            emailMessage.To.Add(emailTo);

            emailMessage.Subject = subject;

            return await dbContext.ReadEmailTemplate(subject);
        }

        private void BuildEmailMessageBody (MimeMessage emailMessage, string body)
        {
            BodyBuilder emailBodyBuilder = new BodyBuilder();
            emailBodyBuilder.HtmlBody = body;
            emailMessage.Body = emailBodyBuilder.ToMessageBody();
        }

        private void SendEmail(MimeMessage emailMessage)
        {
            var mailClient = new SmtpClient { CheckCertificateRevocation = false };

            mailClient.Connect(mailSettings.Value.Host, mailSettings.Value.Port, MailKit.Security.SecureSocketOptions.StartTls);
            mailClient.Authenticate(mailSettings.Value.Email, mailSettings.Value.Password);
            mailClient.Send(emailMessage);
            mailClient.Disconnect(true);

        }
    }
}
