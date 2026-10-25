using System;
using System.Threading;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KNote.Server.Helpers;

/// <summary>
/// The "Alarms" configuration section: how often the web app checks the user's Application info alarms, and the
/// service that sends the Email alarms (off by default: ClientWin may be sending them, see EmailAlarmsService).
/// </summary>
public class AlarmsConfig
{
    public int AppInfoCheckSeconds { get; set; } = 60;

    public EmailAlarmsConfig Email { get; set; } = new();

    // Between 15 seconds and an hour, whatever is configured.
    public int CheckSeconds => Math.Clamp(AppInfoCheckSeconds, 15, 3600);
}

public class EmailAlarmsConfig
{
    public bool Enabled { get; set; }

    public int IntervalSeconds { get; set; } = 60;
}

public static class KntAlarms
{
    public static IServiceCollection KntAddAlarms(this IServiceCollection services, IConfiguration configuration)
    {
        // Bound when used (the in-process tests can override it).
        services.Configure<AlarmsConfig>(configuration.GetSection("Alarms"));
        services.AddSingleton<EmailAlarmsService>();
        services.AddHostedService(provider => provider.GetRequiredService<EmailAlarmsService>());
        return services;
    }
}

/// <summary>
/// Sends the Email alarms of every user of every repository of the Server (KntRepositories), every
/// Alarms:Email:IntervalSeconds, without a browser open: each due one, consumed (Notes.ConsumeAllUsersDueAlarmsAsync:
/// sent once, also with ClientWin sending them), is sent to its user's email through the Server's SMTP account (the
/// "Email" section) with the same email as ClientWin (KntAlarmEmail). A repository or an email that fails is logged and
/// the others go on. When the SMTP server fails, the alarm is put back as it was, to be sent the next time; a user
/// without an email is a permanent failure, written in the alarm's comment (as ClientWin does). It runs without a user
/// (the Server's services don't enforce authorization). Off unless Alarms:Email:Enabled.
/// </summary>
public class EmailAlarmsService : BackgroundService
{
    private readonly KntRepositories _repositories;
    private readonly IEmailSender _sender;
    private readonly IOptions<EmailConfig> _email;
    private readonly IOptions<AlarmsConfig> _config;
    private readonly ILogger<EmailAlarmsService> _logger;

    public EmailAlarmsService(KntRepositories repositories, IEmailSender sender, IOptions<EmailConfig> email,
        IOptions<AlarmsConfig> config, ILogger<EmailAlarmsService> logger)
    {
        _repositories = repositories;
        _sender = sender;
        _email = email;
        _config = config;
        _logger = logger;
    }

    // Whether it is on is read in each round: in KNote Local it is turned on and off from the app (the interval applies
    // the next time the server starts).
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(Math.Clamp(_config.Value.Email.IntervalSeconds, 15, 3600));
        bool? wasEnabled = null;

        using var timer = new PeriodicTimer(interval);
        do
        {
            var enabled = _config.Value.Email.Enabled;
            if (enabled != wasEnabled)
            {
                if (enabled)
                    _logger.LogInformation("The Email alarms service sends the alarms of every repository every {seconds} seconds, without a user (authorization is not enforced).",
                        interval.TotalSeconds);
                else
                    _logger.LogInformation("The Email alarms service is off (Alarms:Email:Enabled).");
                wasEnabled = enabled;
            }

            if (enabled)
                await SendDueAlarmsAsync(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    /// <summary>One round: the due Email alarms of every repository. The number of emails sent.</summary>
    public async Task<int> SendDueAlarmsAsync(CancellationToken cancellationToken = default)
    {
        var email = _email.Value;
        if (string.IsNullOrWhiteSpace(email.Host) || string.IsNullOrWhiteSpace(email.FromAddress))
        {
            // Nothing is consumed: the alarms wait until the account is configured.
            _logger.LogWarning("The Email alarms are not sent: the \"Email\" section (Host, FromAddress) is not configured.");
            return 0;
        }

        var settings = SmtpSettings.FromConfig(email);
        var sent = 0;
        foreach (var repository in _repositories.All)
        {
            if (cancellationToken.IsCancellationRequested)
                break;
            try
            {
                sent += await SendDueAlarmsAsync(repository, settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The Email alarms of the repository {repository} could not be checked.", repository.RepositoryId);
            }
        }
        return sent;
    }

    private async Task<int> SendDueAlarmsAsync(KntRepositoryEntry repository, SmtpSettings settings)
    {
        using var service = _repositories.CreateService(repository, null, null);
        service.Logger = _logger;

        var due = await service.Notes.ConsumeAllUsersDueAlarmsAsync(EnumNotificationType.Email);
        if (!due.IsValid)
        {
            _logger.LogError("The Email alarms of the repository {repository} could not be read: {error}", repository.RepositoryId, due.ErrorMessage);
            return 0;
        }

        var sent = 0;
        foreach (var message in due.Entity)
        {
            try
            {
                if (await SendAsync(service, message, settings))
                    sent++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The Email alarm {message} could not be sent.", message.KMessageId);
            }
        }
        return sent;
    }

    private async Task<bool> SendAsync(IKntService service, KMessageDto message, SmtpSettings settings)
    {
        var note = (await service.Notes.GetAsync(message.NoteId ?? Guid.Empty)).Entity;
        var recipient = message.UserId == null ? null : (await service.Users.GetAsync(message.UserId.Value)).Entity;
        if (note == null)
            return false;
        if (string.IsNullOrWhiteSpace(recipient?.EMail))
        {
            await WriteFailureAsync(service, message, "The assigned user has no valid email address.");
            return false;
        }

        var (subject, body) = KntAlarmEmail.Compose(note, message);
        try
        {
            await Task.Run(() => _sender.Send(settings, recipient.EMail, subject, body));
            return true;
        }
        catch (Exception ex)
        {
            // The SMTP server failed: the alarm as it was when due, to be sent the next time.
            _logger.LogWarning(ex, "The Email alarm {message} could not be sent: it will be sent again.", message.KMessageId);
            message.SetIsDirty(true);
            await service.Notes.SaveMessageAsync(message);
            return false;
        }
    }

    // In the alarm as it is now (already consumed), so it isn't put back.
    private async Task WriteFailureAsync(IKntService service, KMessageDto message, string reason)
    {
        _logger.LogWarning("The Email alarm {message} could not be sent: {reason}", message.KMessageId, reason);
        var current = (await service.Notes.GetMessageAsync(message.KMessageId)).Entity;
        if (current == null)
            return;
        current.Comment = KntAlarmEmail.FailedComment(current, reason);
        current.SetIsDirty(true);
        await service.Notes.SaveMessageAsync(current);
    }
}
