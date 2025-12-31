using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace CoinUpAPI.Services.Email
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpOptions _options;
        private readonly ILogger<SmtpEmailSender> _logger;

        public SmtpEmailSender(IOptions<SmtpOptions> options, ILogger<SmtpEmailSender> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task SendAsync(string toEmail, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(_options.Host) || string.IsNullOrWhiteSpace(_options.FromEmail))
            {
                _logger.LogWarning("SMTP not configured. Skipping email to {To}. Subject: {Subject}", toEmail, subject);
                return;
            }

            var password = _options.Password ?? string.Empty;
            if (IsGmail(_options.Host) && password.Contains(' '))
            {
                // Gmail "App Password" is often displayed with spaces. Remove them.
                password = password.Replace(" ", string.Empty);
            }

            using var client = new SmtpClient(_options.Host, _options.Port)
            {
                EnableSsl = _options.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };

            if (!string.IsNullOrWhiteSpace(_options.Username))
            {
                client.Credentials = new NetworkCredential(_options.Username, password);
            }

            var from = new MailAddress(_options.FromEmail, _options.FromName);
            var to = new MailAddress(toEmail);
            using var message = new MailMessage(from, to)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };

            try
            {
                await client.SendMailAsync(message);
            }
            catch (SmtpException ex) when (IsGmail(_options.Host))
            {
                _logger.LogError(ex,
                    "Gmail SMTP authentication failed. Ensure 2FA is enabled and you are using a Gmail App Password (not your normal password). " +
                    "Host={Host} Port={Port} EnableSsl={EnableSsl} Username={Username} FromEmail={FromEmail}",
                    _options.Host, _options.Port, _options.EnableSsl, _options.Username, _options.FromEmail);
                throw;
            }
        }

        private static bool IsGmail(string host)
            => !string.IsNullOrWhiteSpace(host)
               && host.Contains("gmail", StringComparison.OrdinalIgnoreCase);
    }
}
