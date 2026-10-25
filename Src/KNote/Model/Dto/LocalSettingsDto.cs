using System;
using System.Collections.Generic;

namespace KNote.Model.Dto;

// The settings of KNote Local's computer (api/local, the "This computer" tab of the web app's admin panel): kept in the
// data folder's configuration (knote-local.json), with the secrets encrypted. A secret is never sent back: Has... says
// whether there is one, and an empty one in a change keeps it.

/// <summary>A repository of the computer, as configured (Index: its position in the configuration).</summary>
public class LocalRepositoryDto
{
    public int Index { get; set; }

    // Null when it couldn't be opened (see the server's log).
    public Guid? RepositoryId { get; set; }

    public string Name { get; set; }

    // Microsoft.Data.Sqlite or Microsoft.Data.SqlClient.
    public string Provider { get; set; }

    public string Orm { get; set; }

    // SQLite: the database file. SQL Server: the server and the database (never the password).
    public string Database { get; set; }

    // The folder of its resources (images and attached files) and the container inside it.
    public string ResourcesFolder { get; set; }

    public string ResourcesContainer { get; set; }

    public bool IsDefault { get; set; }

    public bool IsOpen => RepositoryId != null;
}

/// <summary>
/// A repository to add: a new SQLite database (DatabasePath empty: in the data folder), an existing SQLite database
/// (DatabasePath, e.g. one of the desktop app) or a SQL Server database (ConnectionString).
/// </summary>
public class LocalRepositoryAddDto
{
    public string Name { get; set; }

    public string Provider { get; set; } = "Microsoft.Data.Sqlite";

    public string Orm { get; set; } = "EntityFramework";

    public string DatabasePath { get; set; }

    public string ConnectionString { get; set; }

    // Empty: a folder of its own in the data folder's Resources.
    public string ResourcesFolder { get; set; }

    public string ResourcesContainer { get; set; } = "KntCntResources";
}

/// <summary>An AI provider of the web assistant (Index: its position, -1 for a new one).</summary>
public class LocalAiProviderDto
{
    public int Index { get; set; } = -1;

    public string Alias { get; set; }

    // OpenAI, Anthropic or Ollama.
    public string Provider { get; set; }

    public string Model { get; set; }

    // Only for Ollama.
    public string Host { get; set; }

    // Write only: empty keeps the one it has.
    public string ApiKey { get; set; }

    public bool HasApiKey { get; set; }
}

/// <summary>The SMTP account of the e-mails (Email alarms) and whether the server sends the Email alarms.</summary>
public class LocalEmailSettingsDto
{
    public string Host { get; set; }

    public int Port { get; set; } = 587;

    public bool EnableSsl { get; set; } = true;

    public string FromAddress { get; set; }

    public string FromDisplayName { get; set; } = "KNote";

    public string Username { get; set; }

    // Write only: empty keeps the one it has.
    public string Password { get; set; }

    public bool HasPassword { get; set; }

    public bool SendEmailAlarms { get; set; }
}

/// <summary>The local server: where it listens (the port applies the next time it starts) and its data folder.</summary>
public class LocalServerSettingsDto
{
    public int Port { get; set; }

    public string Url { get; set; }

    public string DataPath { get; set; }

    public string Version { get; set; }
}
