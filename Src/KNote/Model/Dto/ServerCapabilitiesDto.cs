namespace KNote.Model.Dto;

/// <summary>
/// What this server can do, for the web app to show or hide it (api/server/capabilities, anonymous). It depends on how
/// the server is configured, never on the user.
/// </summary>
public class ServerCapabilitiesDto
{
    // KNote Local: the computer's settings (repositories, AI providers, e-mail, port) can be managed from the app.
    public bool LocalAdmin { get; set; }
}
