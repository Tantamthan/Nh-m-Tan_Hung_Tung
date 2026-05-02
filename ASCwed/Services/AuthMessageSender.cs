using ASCwed.Cofiguration;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ASCwed.Services
{
    public class AuthMessageSender : IEmailSender, ISmsSender
    {
        private readonly ApplicationSettings _settings;
        private readonly ILogger<AuthMessageSender> _logger;

        public AuthMessageSender(IOptions<ApplicationSettings> settings, ILogger<AuthMessageSender> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            if (string.IsNullOrWhiteSpace(_settings.SMTPServer) ||
                string.IsNullOrWhiteSpace(_settings.SMTPAccount) ||
                string.IsNullOrWhiteSpace(_settings.SMTPPassword))
            {
                _logger.LogWarning("SMTP settings are incomplete. Email to {Email} was skipped.", email);
                return;
            }

            var mailMessage = new MimeMessage();
            mailMessage.From.Add(new MailboxAddress(_settings.ApplicationTitle ?? "ASC", _settings.SMTPAccount));
            mailMessage.To.Add(MailboxAddress.Parse(email));
            mailMessage.Subject = subject;
            mailMessage.Body = new BodyBuilder
            {
                HtmlBody = message
            }.ToMessageBody();

            using var smtpClient = new SmtpClient();
            await smtpClient.ConnectAsync(_settings.SMTPServer, _settings.SMTPPort, SecureSocketOptions.StartTls);
            await smtpClient.AuthenticateAsync(_settings.SMTPAccount, _settings.SMTPPassword);
            await smtpClient.SendAsync(mailMessage);
            await smtpClient.DisconnectAsync(true);
        }

        public Task SendSmsAsync(string number, string message)
        {
            _logger.LogInformation("SMS sending is not configured. Message to {Number} was skipped.", number);
            return Task.CompletedTask;
        }
    }
}
