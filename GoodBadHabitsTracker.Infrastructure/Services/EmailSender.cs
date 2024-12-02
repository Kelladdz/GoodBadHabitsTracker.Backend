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
        public Task SendConfirmationLinkAsync(User user, string link)
        {
            using (MimeMessage emailMessage = new MimeMessage())
            {
                MailboxAddress emailFrom = new MailboxAddress(mailSettings.Value.DisplayName, mailSettings.Value.Email);
                emailMessage.From.Add(emailFrom);

                MailboxAddress emailTo = new MailboxAddress(user.UserName, user.Email);
                emailMessage.To.Add(emailTo);

                emailMessage.Subject = "Welcome To GoodBadHabitsTracker.";
                BodyBuilder emailBodyBuilder = new BodyBuilder();
                emailBodyBuilder.HtmlBody = File.ReadAllText(environment.WebRootPath + "\\EmailBodies\\welcome.html").Replace("{userName}", user.UserName).Replace("{link}", link);
                emailMessage.Body = emailBodyBuilder.ToMessageBody();

                using (SmtpClient mailClient = new SmtpClient())
                {

                    mailClient.CheckCertificateRevocation = false;
                    mailClient.Connect(mailSettings.Value.Host, mailSettings.Value.Port, MailKit.Security.SecureSocketOptions.StartTls);
                    mailClient.Authenticate(mailSettings.Value.Email, mailSettings.Value.Password);
                    mailClient.Send(emailMessage);
                    mailClient.Disconnect(true);
                }
            }
            return Task.CompletedTask;
        }
        public Task SendPasswordResetLinkAsync(User user, string link)
        {
            using (MimeMessage emailMessage = new MimeMessage())

            {
                MailboxAddress emailFrom = new MailboxAddress(mailSettings.Value.DisplayName, mailSettings.Value.Email);
                emailMessage.From.Add(emailFrom);

                MailboxAddress emailTo = new MailboxAddress(user.UserName, user.Email);
                emailMessage.To.Add(emailTo);

                emailMessage.Subject = "Password Reset Request";

                BodyBuilder emailBodyBuilder = new BodyBuilder();
                emailBodyBuilder.HtmlBody = File.ReadAllText(environment.WebRootPath + "\\EmailBodies\\resetPassword.html").Replace("{userName}", user.UserName).Replace("{link}", link);
                emailMessage.Body = emailBodyBuilder.ToMessageBody();

                using (SmtpClient mailClient = new SmtpClient())
                {
                    mailClient.CheckCertificateRevocation = false;
                    mailClient.Connect(mailSettings.Value.Host, mailSettings.Value.Port, MailKit.Security.SecureSocketOptions.StartTls);
                    mailClient.Authenticate(mailSettings.Value.Email, mailSettings.Value.Password);
                    mailClient.Send(emailMessage);
                    mailClient.Disconnect(true);
                }
            }
            return Task.CompletedTask;
        }

        public Task SendMessageAfterNewHabitCreateAsync(User user, Habit habit)
        {
            using (MimeMessage emailMessage = new MimeMessage())
            {
                MailboxAddress emailFrom = new MailboxAddress(mailSettings.Value.DisplayName, mailSettings.Value.Email);
                emailMessage.From.Add(emailFrom);

                MailboxAddress emailTo = new MailboxAddress(user.UserName, user.Email);
                emailMessage.To.Add(emailTo);

                emailMessage.Subject = "Congratulations";

                BodyBuilder emailBodyBuilder = new BodyBuilder();
                emailBodyBuilder.HtmlBody = File.ReadAllText(environment.WebRootPath + "\\EmailBodies\\messageAfterGoodHabitCreate.html").Replace("{userName}", user.UserName).Replace("{habits}", GetHabits(habits));
                emailMessage.Body = emailBodyBuilder.ToMessageBody();

                using (SmtpClient mailClient = new SmtpClient())
                {
                    mailClient.CheckCertificateRevocation = false;
                    mailClient.Connect(mailSettings.Value.Host, mailSettings.Value.Port, MailKit.Security.SecureSocketOptions.StartTls);
                    mailClient.Authenticate(mailSettings.Value.Email, mailSettings.Value.Password);
                    mailClient.Send(emailMessage);
                    mailClient.Disconnect(true);
                }
            }
            return Task.CompletedTask;
        }
    }
}
