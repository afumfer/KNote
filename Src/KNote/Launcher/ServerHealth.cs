using System.Text.Json;

namespace KNote.Launcher;

public enum HealthState
{
    // Nothing answers in that port.
    NotRunning,
    // This user's KNote Local (the same installation id).
    ThisKNote,
    // The KNote Local of another user of the computer, in the same port.
    OtherKNote,
    // Another program uses the port.
    OtherProgram
}

/// <summary>
/// Asks the server's health check (/KNote/health) who answers in its port: it says it is KNote and the id of its
/// installation, to tell it from another program or another user's KNote in the same port.
/// </summary>
public static class ServerHealth
{
    public static async Task<HealthState> CheckAsync(HttpClient http, Uri appUrl, string? instanceId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(3));
            using var response = await http.GetAsync(new Uri(appUrl, "health"), timeout.Token);
            if (!response.IsSuccessStatusCode)
                return HealthState.OtherProgram;
            return Interpret(await response.Content.ReadAsStringAsync(timeout.Token), instanceId);
        }
        catch (HttpRequestException)
        {
            return HealthState.NotRunning;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return HealthState.NotRunning;
        }
    }

    public static HealthState Interpret(string? body, string? instanceId)
    {
        try
        {
            using var health = JsonDocument.Parse(body ?? "");
            var root = health.RootElement;
            if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("app", out var app) || app.GetString() != "KNote")
                return HealthState.OtherProgram;
            var answered = root.TryGetProperty("instanceId", out var id) ? id.GetString() : null;
            return instanceId == null || string.Equals(answered, instanceId, StringComparison.OrdinalIgnoreCase)
                ? HealthState.ThisKNote
                : HealthState.OtherKNote;
        }
        catch (JsonException)
        {
            return HealthState.OtherProgram;
        }
    }
}
