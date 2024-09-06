// ResetPasswordService.cs
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using FleetPulse_BackEndDevelopment.Configuration;
using FleetPulse_BackEndDevelopment.Configurations;
using FleetPulse_BackEndDevelopment.Services.Interfaces;

namespace FleetPulse_BackEndDevelopment.Services
{
    public class ResetPasswordService : IResetPasswordService
    {
        private readonly MailSettings _mailSettings;
        private readonly IWebHostEnvironment _webHostEnvironment;


        public ResetPasswordService(MailSettings mailSettings, IWebHostEnvironment webHostEnvironment)
        {
            _mailSettings = mailSettings;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task SendResetPasswordEmailAsync(string toEmail, string userName, string userEmail)
        {
            using (var smtpClient = new SmtpClient(_mailSettings.Host, _mailSettings.Port))
            {
                smtpClient.Credentials = new NetworkCredential(_mailSettings.Mail, _mailSettings.Password);
                smtpClient.EnableSsl = true;

                // Create the subject and body for the email
                var subject = "Password Reset Request";
                var body = $"A password reset has been requested for user: {userName}\n" +
                           $"Email: {userEmail}";

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_mailSettings.Mail, _mailSettings.DisplayName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false
                };

                mailMessage.To.Add(toEmail);

                // Send the email
                await smtpClient.SendMailAsync(mailMessage);
            }
        }
    }
}