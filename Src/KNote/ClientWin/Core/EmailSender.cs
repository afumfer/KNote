using System.Net;
using System.Net.Mail;

using KNote.Model;

namespace KNote.ClientWin.Core;

// The settings needed to send a message through the single, per-installation SMTP account
// configured in AppConfig (Options > Email). Kept separate from AppConfig itself so this project's
// email sending code isn't tied to the specific shape of the app configuration.
public class SmtpSettings
{
    public string Host { get; set; }
    public int Port { get; set; }
    public bool EnableSsl { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string FromAddress { get; set; }
    public string FromDisplayName { get; set; }

    public static SmtpSettings FromAppConfig(AppConfig config)
    {
        return new SmtpSettings
        {
            Host = config.SmtpHost,
            Port = config.SmtpPort,
            EnableSsl = config.SmtpEnableSsl,
            Username = string.IsNullOrEmpty(config.SmtpUsername) ? config.SmtpFromAddress : config.SmtpUsername,
            Password = config.SmtpPassword,
            FromAddress = config.SmtpFromAddress,
            FromDisplayName = config.SmtpFromDisplayName
        };
    }
}

public interface IEmailSender
{
    void Send(SmtpSettings settings, string toAddress, string subject, string body, bool isBodyHtml = false);
}

public class SmtpEmailSender : IEmailSender
{
    public void Send(SmtpSettings settings, string toAddress, string subject, string body, bool isBodyHtml = false)
    {
        using var message = new MailMessage
        {
            From = new MailAddress(settings.FromAddress, settings.FromDisplayName),
            Subject = subject,
            Body = body,
            IsBodyHtml = isBodyHtml
        };
        message.To.Add(toAddress);

        using var client = new SmtpClient(settings.Host, settings.Port)
        {
            EnableSsl = settings.EnableSsl,
            Credentials = new NetworkCredential(settings.Username, settings.Password)
        };

        client.Send(message);
    }
}
