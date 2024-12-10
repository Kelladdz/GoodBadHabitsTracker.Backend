using GoodBadHabitsTracker.Core.Interfaces;
using GoodBadHabitsTracker.Core.Models;
using GoodBadHabitsTracker.Infrastructure.Configurations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace GoodBadHabitsTracker.Infrastructure.Services
{
    public class EmailSender(IOptions<MailSettings> mailSettings, IWebHostEnvironment environment) : IEmailSender
    {
        public void SendConfirmationLink(User user, string link)
        {
            using (MimeMessage emailMessage = new MimeMessage())
            {
                GetMessageDetails(user, emailMessage);

                emailMessage.Subject = "Welcome To GoodBadHabitsTracker!";

                BodyBuilder emailBodyBuilder = new BodyBuilder();
                emailBodyBuilder.HtmlBody = File.ReadAllText(environment.WebRootPath + "\\EmailBodies\\welcome.html").Replace("{userName}", user.UserName).Replace("{link}", link);
                emailMessage.Body = emailBodyBuilder.ToMessageBody();

                SendEmailAsync(emailMessage);
            }
        }
        public void SendPasswordResetLink(User user, string link)
        {
            using (MimeMessage emailMessage = new MimeMessage())
            {
                GetMessageDetails(user, emailMessage);

                emailMessage.Subject = "Password Reset Request";

                BodyBuilder emailBodyBuilder = new BodyBuilder();
                emailBodyBuilder.HtmlBody = File.ReadAllText(environment.WebRootPath + "\\EmailBodies\\resetPassword.html").Replace("{userName}", user.UserName).Replace("{link}", link);
                emailMessage.Body = emailBodyBuilder.ToMessageBody();

                SendEmailAsync(emailMessage);
            }
        }

        public void SendMessageAfterNewHabitCreate(User user, Habit habit)
        {
            using (MimeMessage emailMessage = new MimeMessage())
            {
                GetMessageDetails(user, emailMessage);

                BodyBuilder emailBodyBuilder = new BodyBuilder();
                emailBodyBuilder.HtmlBody = File.ReadAllText(environment.WebRootPath + "\\EmailBodies\\messageAfterGoodHabitCreate.html").Replace("{userName}", user.UserName);
                emailMessage.Body = emailBodyBuilder.ToMessageBody();

                SendEmailAsync(emailMessage);
            }
        }
        private void GetMessageDetails(User user, MimeMessage emailMessage)
        {
            MailboxAddress emailFrom = new MailboxAddress(mailSettings.Value.DisplayName, mailSettings.Value.Email);
            emailMessage.From.Add(emailFrom);

            MailboxAddress emailTo = new MailboxAddress(user.UserName, user.Email);
            emailMessage.To.Add(emailTo);
        }

        private void SendEmailAsync(MimeMessage emailMessage)
        {
            var mailClient = new SmtpClient { CheckCertificateRevocation = false };

            mailClient.Connect(mailSettings.Value.Host, mailSettings.Value.Port, MailKit.Security.SecureSocketOptions.StartTls);
            mailClient.Authenticate(mailSettings.Value.Email, mailSettings.Value.Password);
            mailClient.Send(emailMessage);
            mailClient.Disconnect(true);

        }
    }
}
