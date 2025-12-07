using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace VirtualEventTicketing.Services
{
    // This class implements the IEmailSender interface required by Identity
    public class EmailSender : IEmailSender
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(
            IWebHostEnvironment environment, 
            IConfiguration configuration,
            ILogger<EmailSender> logger)
        {
            _environment = environment;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var emailProvider = _configuration["EmailSettings:Provider"] ?? "File"; // Default to File for development

            try
            {
                switch (emailProvider.ToLower())
                {
                    case "smtp":
                        await SendEmailViaSmtpAsync(email, subject, htmlMessage);
                        break;
                    case "sendgrid":
                        await SendEmailViaSendGridAsync(email, subject, htmlMessage);
                        break;
                    case "mailgun":
                        await SendEmailViaMailgunAsync(email, subject, htmlMessage);
                        break;
                    case "file":
                    default:
                        await SaveEmailToFileAsync(email, subject, htmlMessage);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}. Falling back to file storage.", email);
                // Fallback to file storage if email sending fails
                await SaveEmailToFileAsync(email, subject, htmlMessage);
            }
        }

        private async Task SendEmailViaSmtpAsync(string email, string subject, string htmlMessage)
        {
            var smtpHost = _configuration["EmailSettings:Smtp:Host"];
            var smtpPort = int.Parse(_configuration["EmailSettings:Smtp:Port"] ?? "587");
            var smtpUsername = _configuration["EmailSettings:Smtp:Username"];
            var smtpPassword = _configuration["EmailSettings:Smtp:Password"];
            var smtpFromEmail = _configuration["EmailSettings:Smtp:FromEmail"];
            var smtpFromName = _configuration["EmailSettings:Smtp:FromName"] ?? "Virtual Event Ticketing";
            var enableSsl = bool.Parse(_configuration["EmailSettings:Smtp:EnableSsl"] ?? "true");

            if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword))
            {
                throw new InvalidOperationException("SMTP settings are not configured. Please check appsettings.json");
            }

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                EnableSsl = enableSsl
            };

            using var message = new MailMessage
            {
                From = new MailAddress(smtpFromEmail ?? smtpUsername, smtpFromName),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };

            message.To.Add(email);

            await client.SendMailAsync(message);
            _logger.LogInformation("Email sent via SMTP to {Email}", email);
        }

        private async Task SendEmailViaSendGridAsync(string email, string subject, string htmlMessage)
        {
            var apiKey = _configuration["EmailSettings:SendGrid:ApiKey"];
            var fromEmail = _configuration["EmailSettings:SendGrid:FromEmail"];
            var fromName = _configuration["EmailSettings:SendGrid:FromName"] ?? "Virtual Event Ticketing";

            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(fromEmail))
            {
                throw new InvalidOperationException("SendGrid settings are not configured. Please check appsettings.json");
            }

            // Note: You need to install SendGrid NuGet package: dotnet add package SendGrid
            // Uncomment the following code after installing SendGrid package:
            /*
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress(fromEmail, fromName),
                Subject = subject,
                PlainTextContent = null,
                HtmlContent = htmlMessage
            };
            msg.AddTo(new EmailAddress(email));
            
            var response = await client.SendEmailAsync(msg);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Body.ReadAsStringAsync();
                throw new Exception($"SendGrid API error: {response.StatusCode} - {body}");
            }
            */
            
            throw new NotImplementedException("SendGrid integration requires the SendGrid NuGet package. Install it with: dotnet add package SendGrid");
        }

        private async Task SendEmailViaMailgunAsync(string email, string subject, string htmlMessage)
        {
            var apiKey = _configuration["EmailSettings:Mailgun:ApiKey"];
            var domain = _configuration["EmailSettings:Mailgun:Domain"];
            var fromEmail = _configuration["EmailSettings:Mailgun:FromEmail"];
            var fromName = _configuration["EmailSettings:Mailgun:FromName"] ?? "Virtual Event Ticketing";

            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(domain) || string.IsNullOrEmpty(fromEmail))
            {
                throw new InvalidOperationException("Mailgun settings are not configured. Please check appsettings.json");
            }

            // Mailgun API call using HttpClient
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", 
                    Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"api:{apiKey}")));

            var formData = new List<KeyValuePair<string, string>>
            {
                new("from", $"{fromName} <{fromEmail}>"),
                new("to", email),
                new("subject", subject),
                new("html", htmlMessage)
            };

            var content = new FormUrlEncodedContent(formData);
            var response = await client.PostAsync($"https://api.mailgun.net/v3/{domain}/messages", content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"Mailgun API error: {response.StatusCode} - {errorBody}");
            }

            _logger.LogInformation("Email sent via Mailgun to {Email}", email);
        }

        private async Task SaveEmailToFileAsync(string email, string subject, string htmlMessage)
        {
            // Fallback: Save email to file (for development/testing)
            var path = Path.Combine(_environment.WebRootPath, "sent_emails");
            Directory.CreateDirectory(path);

            var fileName = $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}_{email}.html";
            var fullPath = Path.Combine(path, fileName);

            var emailContent = $"To: {email}<br>Subject: {subject}<br><br>{htmlMessage}";

            await File.WriteAllTextAsync(fullPath, emailContent);
            _logger.LogInformation("Email saved to file: {FileName}", fileName);
        }
    }
}

