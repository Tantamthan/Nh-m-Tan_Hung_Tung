using Lab8_PhamVanTung_2324801030079.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Lab8_PhamVanTung_2324801030079.Services;

public sealed class EmailSender : IEmailSender
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(
        IOptions<EmailSettings> emailOptions,
        ILogger<EmailSender> logger)
    {
        _emailSettings = emailOptions.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        if (!_emailSettings.HasValidConfiguration())
        {
            throw new InvalidOperationException(
                $"Email settings are incomplete. Update '{EmailSettings.SectionName}' in appsettings.json before sending mail.");
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_emailSettings.DisplayName, _emailSettings.FromEmail));
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = subject;
        message.Body = new BodyBuilder
        {
            HtmlBody = htmlMessage
        }.ToMessageBody();

        using var client = new SmtpClient();

        try
        {
            await client.ConnectAsync(
                _emailSettings.Host,
                _emailSettings.Port,
                ResolveSecurityOption(_emailSettings.SecureSocketOption));

            await client.AuthenticateAsync(_emailSettings.UserName, _emailSettings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Reset email sent successfully to {EmailAddress}.", email);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to send email to {EmailAddress}.", email);
            throw;
        }
    }

    private static SecureSocketOptions ResolveSecurityOption(string option)
    {
        return Enum.TryParse<SecureSocketOptions>(option, ignoreCase: true, out var parsedOption)
            ? parsedOption
            : SecureSocketOptions.StartTls;
    }
}
