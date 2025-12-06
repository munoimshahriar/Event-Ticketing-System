using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace VirtualEventTicketing.Services
{
    // This class implements the IEmailSender interface required by Identity
    public class EmailSender : IEmailSender
    {
        private readonly IWebHostEnvironment _environment;

        public EmailSender(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // We create a directory named "sent_emails" in wwwroot to store the files
            var path = Path.Combine(_environment.WebRootPath, "sent_emails");
            Directory.CreateDirectory(path);

            // Create a unique file name for each email
            var fileName = $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}_{email}.html";
            var fullPath = Path.Combine(path, fileName);

            // The content of the "email"
            var emailContent = $"To: {email}<br>Subject: {subject}<br><br>{htmlMessage}";

            // Write the email to a file
            await File.WriteAllTextAsync(fullPath, emailContent);
        }
    }
}

