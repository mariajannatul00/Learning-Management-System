using LMS.Web.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace LMS.Web.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;

        public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var smtpSection = _configuration.GetSection("SmtpSettings");
                string host = smtpSection["Server"] ?? "smtp.gmail.com";
                int port = int.TryParse(smtpSection["Port"], out var parsedPort) ? parsedPort : 587;
                string senderEmail = smtpSection["SenderEmail"] ?? "naima16maria@gmail.com";
                string senderName = smtpSection["SenderName"] ?? "LearnPulse LMS";
                string username = smtpSection["Username"] ?? "naima16maria@gmail.com";
                string password = smtpSection["Password"] ?? "bbqx repk utky iscn";
                bool enableSsl = bool.TryParse(smtpSection["EnableSsl"], out var parsedSsl) ? parsedSsl : true;

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                using var smtpClient = new SmtpClient(host, port)
                {
                    Credentials = new NetworkCredential(username, password),
                    EnableSsl = enableSsl
                };

                _logger.LogInformation("Sending email to {ToEmail} via {Host}:{Port}...", toEmail, host, port);
                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation("Email successfully sent to {ToEmail}.", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {ToEmail}.", toEmail);
                // Throw exception so callers or logs know sending failed if critical
                throw;
            }
        }
    }
}
