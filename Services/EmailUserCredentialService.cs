using FleetPulse_BackEndDevelopment.Configuration;
using Microsoft.Extensions.Options;
using MimeKit;
using FleetPulse_BackEndDevelopment.Services.Interfaces;
using MailKit.Net.Smtp;
using MimeKit.Utils;

namespace FleetPulse_BackEndDevelopment.Services
{
    public class EmailUserCredentialService : IEmailUserCredentialService
    {
        private readonly MailSettings _mailSettings;
        private readonly ILogger<EmailUserCredentialService> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EmailUserCredentialService(IOptions<MailSettings> mailSettings,
            ILogger<EmailUserCredentialService> logger,IWebHostEnvironment webHostEnvironment)
        {
            _mailSettings = mailSettings.Value;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task SendUsernameAndPassword(string emailAddress, string userName, string password)
        {
            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(_mailSettings.Mail);
            email.To.Add(MailboxAddress.Parse(emailAddress));
            email.Subject = "Your FleetPulse Account Credentials";

            var builder = new BodyBuilder();

            // Load the HTML template
            var templatePath = Path.Combine(_webHostEnvironment.ContentRootPath, "EmailTemplates", "AccountCredentialsEmailTemplate.html");

            if (!File.Exists(templatePath))
            {
                _logger.LogError("Email template not found: {TemplatePath}", templatePath);
                throw new FileNotFoundException("Email template not found", templatePath);
            }

            var templateContent = await File.ReadAllTextAsync(templatePath);
            var emailBody = templateContent
                .Replace("{UserName}", userName)
                .Replace("{Password}", password);

            builder.HtmlBody = emailBody;

            // Attach the logo as a linked resource
            var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "EmailTemplates", "logo.jpg");
            if (File.Exists(logoPath))
            {
                var image = builder.LinkedResources.Add(logoPath);
                image.ContentId = MimeUtils.GenerateMessageId();
            }

            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            smtp.CheckCertificateRevocation = false;

            try
            {
                await smtp.ConnectAsync(_mailSettings.Host, _mailSettings.Port,
                    MailKit.Security.SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_mailSettings.Mail, _mailSettings.Password);

                // Send the email
                await smtp.SendAsync(email);

                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending email: {Message}", ex.Message);
                throw;
            }
        }
    }
}