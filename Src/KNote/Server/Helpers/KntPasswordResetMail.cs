using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
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

/// <summary>The token of the user in a repository of the Server (Users.CreatePasswordResetAsync, only valid there).</summary>
public sealed record KntRepositoryResetToken(Guid RepositoryId, string RepositoryName, UserPasswordResetTokenDto Reset);

/// <summary>
/// The token of a "set a new password" link: the tokens of the same user (the same user name and email) in each
/// repository of the Server, so one link sets the new password in all of them. Not signed itself: each token is (and
/// is only valid in its repository), so nothing can be added to it that wasn't valid on its own.
/// </summary>
public static class KntPasswordResetLinkToken
{
    private sealed record Entry(Guid R, string T);

    public static string Create(IEnumerable<KntRepositoryResetToken> tokens) =>
        Base64Url(JsonSerializer.SerializeToUtf8Bytes(tokens.Select(t => new Entry(t.RepositoryId, t.Reset.Token)).ToList()));

    /// <summary>The token of each repository, or null if it isn't a link's token.</summary>
    public static List<(Guid RepositoryId, string Token)> Read(string token)
    {
        try
        {
            var base64 = (token ?? "").Replace('-', '+').Replace('_', '/');
            var entries = JsonSerializer.Deserialize<List<Entry>>(Convert.FromBase64String(base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=')));
            return entries is { Count: > 0 } && entries.All(e => !string.IsNullOrEmpty(e.T))
                ? entries.Select(e => (e.R, e.T)).ToList()
                : null;
        }
        catch (Exception ex) when (ex is FormatException or JsonException)
        {
            return null;
        }
    }

    private static string Base64Url(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}

/// <summary>
/// "Forgot your password?" (UsersController.ForgotPassword): sends the user one link to set a new password in every
/// repository where that user is with the same email, through the Server's SMTP account (the "Email" section,
/// EmailConfig). Available only when both are configured.
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
    /// Sends the link of the tokens of a user (the same user name and email in each repository) without waiting: the
    /// answer takes the same time whether the user exists or not (a slower one would say who is registered). A failure
    /// is only logged, for the same reason.
    /// </summary>
    public void SendInBackground(IReadOnlyList<KntRepositoryResetToken> tokens)
    {
        var settings = SmtpSettings.FromConfig(_email.Value);
        var user = tokens[0].Reset;
        var (subject, body) = Compose(tokens);

        _ = Task.Run(() =>
        {
            try
            {
                _sender.Send(settings, user.EMail, subject, body, isBodyHtml: true);
                _logger.LogInformation("Password reset link sent to the user {user}.", user.UserName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The password reset link for the user {user} could not be sent.", user.UserName);
            }
        });
    }

    private (string Subject, string Body) Compose(IReadOnlyList<KntRepositoryResetToken> tokens)
    {
        var user = tokens[0].Reset;
        var link = WebUtility.HtmlEncode(LinkFor(KntPasswordResetLinkToken.Create(tokens)));
        var name = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(user.FullName) ? user.UserName : user.FullName);
        var repositories = WebUtility.HtmlEncode(string.Join(", ", tokens.Select(t => t.RepositoryName)));
        var minutes = (int)LinkLifetime.TotalMinutes;

        var subject = $"{KntConst.AppName} - Set a new password";
        var body =
            $"<p>Hello {name},</p>" +
            $"<p>Someone asked to set a new password for your {KntConst.AppName} user <b>{WebUtility.HtmlEncode(user.UserName)}</b>. " +
            $"To do it, open this link within the next {minutes} minutes:</p>" +
            $"<p><a href=\"{link}\">{link}</a></p>" +
            (tokens.Count == 1
                ? $"<p>The new password is set in the repository <b>{repositories}</b>.</p>"
                : $"<p>The new password is set in all your repositories with this user and email: <b>{repositories}</b>.</p>") +
            "<p>The link works once. If you didn't ask for it, ignore this message: your password doesn't change.</p>";

        return (subject, body);
    }
}
