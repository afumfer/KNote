using System;
using System.Net;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KNote.Server.Helpers;

/// <summary>
/// The "PasswordReset" configuration section: where the links sent by email point to, and for how long they work.
/// </summary>
public class PasswordResetConfig
{
    // The web app's address as its users open it (e.g. https://server/KNote/). Configured, never taken from the
    // request: its Host header can be anything, and a link to someone else's site would get the token. Empty: no
    // password reset by email.
    public string PublicBaseUrl { get; set; }

    public int LinkMinutes { get; set; } = 30;
}

/// <summary>
/// "Forgot your password?" (UsersController.ForgotPassword): sends the user a link to set a new password, through
/// the Server's SMTP account (the "Email" section, EmailConfig). Available only when both are configured.
/// </summary>
public class KntPasswordResetMail
{
    private readonly IOptions<PasswordResetConfig> _config;
    private readonly IOptions<EmailConfig> _email;
    private readonly IEmailSender _sender;
    private readonly ILogger<KntPasswordResetMail> _logger;

    public KntPasswordResetMail(IOptions<PasswordResetConfig> config, IOptions<EmailConfig> email, IEmailSender sender, ILogger<KntPasswordResetMail> logger)
    {
        _config = config;
        _email = email;
        _sender = sender;
        _logger = logger;
    }

    public bool IsAvailable =>
        !string.IsNullOrWhiteSpace(_config.Value.PublicBaseUrl)
        && !string.IsNullOrWhiteSpace(_email.Value.Host)
        && !string.IsNullOrWhiteSpace(_email.Value.FromAddress);

    // Between 5 minutes and a day, whatever is configured.
    public TimeSpan LinkLifetime => TimeSpan.FromMinutes(Math.Clamp(_config.Value.LinkMinutes, 5, 24 * 60));

    public string LinkFor(string token) => $"{_config.Value.PublicBaseUrl.TrimEnd('/')}/reset-password?token={Uri.EscapeDataString(token)}";

    /// <summary>
    /// Sends it without waiting: the answer takes the same time whether the user exists or not (a slower one would
    /// say who is registered). A failure is only logged, for the same reason.
    /// </summary>
    public void SendInBackground(UserPasswordResetTokenDto reset)
    {
        var settings = SmtpSettings.FromConfig(_email.Value);
        var (subject, body) = Compose(reset);

        _ = Task.Run(() =>
        {
            try
            {
                _sender.Send(settings, reset.EMail, subject, body, isBodyHtml: true);
                _logger.LogInformation("Password reset link sent to the user {user}.", reset.UserName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The password reset link for the user {user} could not be sent.", reset.UserName);
            }
        });
    }

    private (string Subject, string Body) Compose(UserPasswordResetTokenDto reset)
    {
        var link = WebUtility.HtmlEncode(LinkFor(reset.Token));
        var name = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(reset.FullName) ? reset.UserName : reset.FullName);
        var minutes = (int)LinkLifetime.TotalMinutes;

        var subject = $"{KntConst.AppName} - Set a new password";
        var body =
            $"<p>Hello {name},</p>" +
            $"<p>Someone asked to set a new password for your {KntConst.AppName} user <b>{WebUtility.HtmlEncode(reset.UserName)}</b>. " +
            $"To do it, open this link within the next {minutes} minutes:</p>" +
            $"<p><a href=\"{link}\">{link}</a></p>" +
            "<p>The link works once. If you didn't ask for it, ignore this message: your password doesn't change.</p>";

        return (subject, body);
    }
}
