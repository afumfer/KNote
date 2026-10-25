using System.Collections.Concurrent;
using KNote.Service.Core;

namespace KNote.Tests.Helpers;

/// <summary>
/// The Server's IEmailSender in the in-process tests: keeps the messages instead of sending them. The Server sends
/// them in the background (KntPasswordResetMail), so a test waits for one with WaitForAsync.
/// </summary>
public class FakeEmailSender : IEmailSender
{
    public record SentEmail(string To, string Subject, string Body);

    private readonly ConcurrentQueue<SentEmail> _sent = new();
    private readonly SemaphoreSlim _signal = new(0);

    public IReadOnlyCollection<SentEmail> Sent => _sent.ToArray();

    // Sending to this address fails as an SMTP server that is down would.
    public string? FailTo { get; set; }

    public void Send(SmtpSettings settings, string toAddress, string subject, string body, bool isBodyHtml = false)
        => Send(settings, new[] { toAddress }, subject, body, isBodyHtml);

    public void Send(SmtpSettings settings, IEnumerable<string> toAddresses, string subject, string body, bool isBodyHtml = false)
    {
        if (FailTo != null && toAddresses.Contains(FailTo))
            throw new System.Net.Mail.SmtpException("The SMTP server is not available.");
        foreach (var to in toAddresses)
            _sent.Enqueue(new SentEmail(to, subject, body));
        _signal.Release();
    }

    // The next message sent, or null if none comes in time.
    public async Task<SentEmail?> WaitForAsync(TimeSpan? timeout = null)
    {
        if (!await _signal.WaitAsync(timeout ?? TimeSpan.FromSeconds(5)))
            return null;
        return _sent.ToArray().LastOrDefault();
    }
}
