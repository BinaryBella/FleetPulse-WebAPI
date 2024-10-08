using FleetPulse_BackEndDevelopment.Configuration;
using FleetPulse_BackEndDevelopment.Models;
using FleetPulse_BackEndDevelopment.Services.Interfaces;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Utils;

namespace FleetPulse_BackEndDevelopment.Services
{
    public class MailService : IMailService
    {
        private readonly MailSettings _mailSettings;
        private readonly ILogger<MailService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public MailService(IOptions<MailSettings> mailSettings, 
            ILogger<MailService> logger, 
            IHttpContextAccessor httpContextAccessor, 
            IHttpClientFactory httpClientFactory,
            IWebHostEnvironment webHostEnvironment) { 
            _mailSettings = mailSettings.Value;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _httpClientFactory = httpClientFactory;
            _webHostEnvironment = webHostEnvironment;
        }

        private async Task<string> GetEmailTemplateAsync(string templatePath)
        {
            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            var fullUrl = $"{baseUrl}/EmailTemplates/{templatePath}";

            _logger.LogInformation($"Fetching email template from: {fullUrl}");

            using var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(fullUrl);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to fetch email template. Status code: {response.StatusCode}");
                throw new FileNotFoundException($"Email template not found: {fullUrl}");
            }

            return await response.Content.ReadAsStringAsync();
        }

        public async Task SendEmailAsync(MailRequest mailRequest)
        {
            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(_mailSettings.Mail);
            email.To.Add(MailboxAddress.Parse(mailRequest.ToEmail));
            email.Subject = mailRequest.Subject;

            var builder = new BodyBuilder();

            // Load the HTML template
            //var templatePath = Path.Combine(_webHostEnvironment.ContentRootPath, "EmailTemplates", "VerificationEmailTemplate.html");

            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            var templatePath = Path.Combine(_webHostEnvironment.WebRootPath, "EmailTemplates/VerificationEmailTemplate.html");

            if (!File.Exists(templatePath))
            {
                _logger.LogError("Email template not found: {TemplatePath}", templatePath);
                throw new FileNotFoundException("Email template not found", templatePath);
            }

            var templateContent = await File.ReadAllTextAsync(templatePath);
            var emailBody = templateContent.Replace("{{VerificationCode}}", mailRequest.Body);
            builder.HtmlBody = emailBody;

            // Attach the logo as a linked resource
            var logoPath = Path.Combine(_webHostEnvironment.WebRootPath, "EmailTemplates", "logo.jpg");
            var image = builder.LinkedResources.Add(logoPath);
            image.ContentId = MimeUtils.GenerateMessageId();

            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            smtp.CheckCertificateRevocation = false;

            try
            {
                await smtp.ConnectAsync(_mailSettings.Host, _mailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_mailSettings.Mail, _mailSettings.Password);

                // Send the email with embedded image
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