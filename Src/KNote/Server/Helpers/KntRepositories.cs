using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Service.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KNote.Server.Helpers;

/// <summary>
/// A repository of the "Repositories" configuration section: a RepositoryRef, plus which one is the default (the
/// repository of the anonymous requests and of the sign-ups). Only in the Server's configuration.
/// </summary>
public class KntRepositoryConfig : RepositoryRef
{
    public bool IsDefault { get; set; }
}

/// <summary>A repository the Server opened: its id (DB_REPOSITORYID) and its configuration.</summary>
public sealed record KntRepositoryEntry(Guid RepositoryId, RepositoryRef RepositoryRef, bool IsDefault);

/// <summary>
/// The repositories this Server serves (a singleton, opened at startup and, in KNote Local, opened again when its
/// configuration changes: ReloadAsync). A session is in one of them: its token carries
/// the RepositoryId (KntConst.RepositoryClaimType), and the IKntService of each request is created for that repository
/// (KntExtensions.KntAddServices); a request without a user is served by the default repository.
///
/// The configuration is the "Repositories" list or, when it is empty, the "RepositoryRef" section (a server with one
/// repository). A repository that can't be opened is left out (logged) and the others still start; two databases with
/// the same RepositoryId (a copy of a database) and two repositories whose resources would share a folder or a url
/// are refused the same way. The repositories' names aren't kept here: they are in their databases and an Admin can
/// change them.
///
/// The repositories are a snapshot that is replaced whole when they are opened again, so a request always sees one
/// consistent list (a reader never waits). With serveResources (AppSettings:ServeRepositoryResources, KNote Local), a
/// repository without a ResourcesContainerRootUrl has its resources served by this host at
/// /KNote/Resources/{RepositoryId} (KntExtensions.MapKntRepositoryResources).
/// </summary>
public class KntRepositories
{
    public const string ResourcesPath = "/Resources";

    private sealed record Snapshot(IReadOnlyList<KntRepositoryEntry> Entries, KntRepositoryEntry Default);

    private readonly string _webRootPath;
    private readonly bool _serveResources;
    private readonly SemaphoreSlim _reloading = new(1, 1);
    private volatile Snapshot _snapshot;

    public KntRepositories(IEnumerable<KntRepositoryEntry> entries, string webRootPath, bool serveResources = false)
    {
        _snapshot = ToSnapshot(entries.ToList());
        _webRootPath = webRootPath;
        _serveResources = serveResources;
    }

    public IReadOnlyList<KntRepositoryEntry> All => _snapshot.Entries;

    public KntRepositoryEntry Default => _snapshot.Default;

    public KntRepositoryEntry Find(Guid repositoryId) => _snapshot.Entries.FirstOrDefault(e => e.RepositoryId == repositoryId);

    /// <summary>The repository of the user's session (null if the token doesn't name one of these repositories).</summary>
    public KntRepositoryEntry FindForUser(ClaimsPrincipal user) =>
        RepositoryIdOf(user) is Guid repositoryId ? Find(repositoryId) : null;

    /// <summary>The session's repository, or the default one for a request without a user.</summary>
    public KntRepositoryEntry ForUser(ClaimsPrincipal user) => FindForUser(user) ?? Default;

    public static Guid? RepositoryIdOf(ClaimsPrincipal user) =>
        Guid.TryParse(user?.FindFirst(KntConst.RepositoryClaimType)?.Value, out var repositoryId) ? repositoryId : null;

    /// <summary>
    /// A service for the repository, for userName (null: no user yet, the caller sets it). The resources are kept in
    /// this host and served from the request's address unless the repository's configuration says otherwise; that is
    /// filled in a copy of its RepositoryRef, so each request has its own.
    /// </summary>
    public IKntService CreateService(KntRepositoryEntry repository, string userName, HttpRequest request)
    {
        var repositoryRef = Copy(repository.RepositoryRef);
        if (string.IsNullOrEmpty(repositoryRef.ResourcesContainerRootPath))
            repositoryRef.ResourcesContainerRootPath = _webRootPath;
        if (string.IsNullOrEmpty(repositoryRef.ResourcesContainerRootUrl) && request != null)
            repositoryRef.ResourcesContainerRootUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";

        return new KntService(KntRepositoryFactory.Create(repositoryRef)) { UserIdentityName = userName };
    }

    #region Startup

    public static List<KntRepositoryConfig> ReadConfiguration(IConfiguration configuration)
    {
        var repositories = (configuration.GetSection("Repositories").Get<List<KntRepositoryConfig>>() ?? new())
            .Where(r => !string.IsNullOrWhiteSpace(r?.ConnectionString))
            .ToList();
        if (repositories.Count > 0)
            return repositories;

        var section = configuration.GetSection("RepositoryRef");
        var repository = section.Get<KntRepositoryConfig>() ?? new KntRepositoryConfig();
        // LEGACY(repository-alias): remove once every Server configuration uses "Name".
        // Before the name was kept in the database, this setting was called "Alias" (KntSchemaUpdater gives it
        // to the database when it has no name yet).
        if (string.IsNullOrWhiteSpace(repository.Name))
            repository.Name = section["Alias"];
        repository.IsDefault = true;
        return [repository];
    }

    public static async Task<KntRepositories> OpenAsync(IReadOnlyList<KntRepositoryConfig> configs, string webRootPath, ILogger logger,
        bool serveResources = false)
    {
        var entries = await OpenEntriesAsync(configs, webRootPath, serveResources, logger, new List<string>());
        if (entries.Count == 0)
            throw new InvalidOperationException("The Server has no repository: none of the configured ones could be opened (see the log).");
        return new KntRepositories(entries, webRootPath, serveResources);
    }

    /// <summary>
    /// Opens the repositories of configs again (KNote Local, after its configuration changed), with the same rules as at
    /// startup, and replaces the ones served: a session in a repository that is no longer there is refused. If none
    /// could be opened, the current ones are kept. What was left out, and why, is returned (and logged).
    /// </summary>
    public async Task<List<string>> ReloadAsync(IReadOnlyList<KntRepositoryConfig> configs, ILogger logger)
    {
        await _reloading.WaitAsync();
        try
        {
            var problems = new List<string>();
            var entries = await OpenEntriesAsync(configs, _webRootPath, _serveResources, logger, problems);
            if (entries.Count == 0)
                problems.Add("None of the repositories could be opened: the current ones are kept.");
            else
                _snapshot = ToSnapshot(entries);
            return problems;
        }
        finally
        {
            _reloading.Release();
        }
    }

    private static Snapshot ToSnapshot(List<KntRepositoryEntry> entries)
    {
        if (entries.Count == 0)
            throw new InvalidOperationException("The Server has no repository: none of the configured ones could be opened.");
        return new Snapshot(entries, entries.First(e => e.IsDefault));
    }

    private static async Task<List<KntRepositoryEntry>> OpenEntriesAsync(IReadOnlyList<KntRepositoryConfig> configs, string webRootPath,
        bool serveResources, ILogger logger, List<string> problems)
    {
        var opened = new List<(KntRepositoryConfig Config, Guid RepositoryId)>();
        var resourceFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var resourceUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var config in configs)
        {
            var label = string.IsNullOrWhiteSpace(config.Name) ? config.GetDefaultName() : config.Name;

            Guid repositoryId;
            try
            {
                repositoryId = await ReadRepositoryIdAsync(config);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "The repository {repository} could not be opened: it is left out until the Server restarts.", label);
                problems.Add($"The repository {label} could not be opened: {ex.Message}");
                continue;
            }

            var sameDatabase = opened.FirstOrDefault(o => o.RepositoryId == repositoryId);
            if (sameDatabase.Config != null)
            {
                logger.LogError("The repository {repository} is the same database as (or a copy of) {other} (RepositoryId {id}): it is left out.",
                    label, sameDatabase.Config.Name ?? sameDatabase.Config.GetDefaultName(), repositoryId);
                problems.Add($"The repository {label} is the same database as (or a copy of) {sameDatabase.Config.Name ?? sameDatabase.Config.GetDefaultName()}.");
                continue;
            }

            // The resources of each repository in their own folder, served from their own url.
            if (serveResources && string.IsNullOrEmpty(config.ResourcesContainerRootUrl))
                config.ResourcesContainerRootUrl = $"/KNote{ResourcesPath}/{repositoryId}";
            var rootPath = string.IsNullOrEmpty(config.ResourcesContainerRootPath) ? webRootPath ?? "(web root)" : config.ResourcesContainerRootPath;
            var folder = Path.Combine(rootPath, config.ResourcesContainer ?? "").TrimEnd('/', '\\');
            var url = $"{(string.IsNullOrEmpty(config.ResourcesContainerRootUrl) ? "(this host)" : config.ResourcesContainerRootUrl.TrimEnd('/'))}/{config.ResourcesContainer}";
            if (resourceFolders.Contains(folder) || resourceUrls.Contains(url))
            {
                logger.LogError("The resources of the repository {repository} ({folder}, {url}) would be shared with another repository: it is left out. " +
                    "Give each repository its own ResourcesContainerRootPath and ResourcesContainerRootUrl.", label, folder, url);
                problems.Add($"The resources of the repository {label} ({folder}) would be shared with another repository.");
                continue;
            }
            resourceFolders.Add(folder);
            resourceUrls.Add(url);

            opened.Add((config, repositoryId));
            logger.LogInformation("Repository {repository} ({id}) opened.", label, repositoryId);
        }

        if (opened.Count == 0)
            return new List<KntRepositoryEntry>();

        var defaultIndex = opened.FindIndex(o => o.Config.IsDefault);
        if (defaultIndex < 0)
        {
            defaultIndex = 0;
            if (configs.Any(c => c.IsDefault))
                logger.LogWarning("The default repository could not be opened: the default one is {repository}.", opened[0].Config.Name ?? opened[0].Config.GetDefaultName());
        }
        else if (opened.Count(o => o.Config.IsDefault) > 1)
            logger.LogWarning("Several repositories are marked IsDefault: the default one is {repository}.", opened[defaultIndex].Config.Name ?? opened[defaultIndex].Config.GetDefaultName());

        return opened.Select((o, i) => new KntRepositoryEntry(o.RepositoryId, ToRepositoryRef(o.Config, o.RepositoryId), i == defaultIndex)).ToList();
    }

    // Opening a repository creates or updates its database (KntSchemaUpdater), which gives it its id.
    private static async Task<Guid> ReadRepositoryIdAsync(RepositoryRef repositoryRef)
    {
        using var service = new KntService(KntRepositoryFactory.Create(repositoryRef));
        var info = await service.SystemValues.GetRepositoryInfoAsync();
        if (!info.IsValid)
            throw new InvalidOperationException(info.ErrorMessage);
        return info.Entity.RepositoryId;
    }

    private static RepositoryRef ToRepositoryRef(RepositoryRef config, Guid repositoryId)
    {
        var repositoryRef = Copy(config);
        repositoryRef.RepositoryId = repositoryId;
        return repositoryRef;
    }

    #endregion

    private static RepositoryRef Copy(RepositoryRef source) => new()
    {
        Name = source.Name,
        RepositoryId = source.RepositoryId,
        ConnectionString = source.ConnectionString,
        Provider = source.Provider,
        Orm = source.Orm,
        ResourcesContainer = source.ResourcesContainer,
        ResourcesContainerRootPath = source.ResourcesContainerRootPath,
        ResourcesContainerRootUrl = source.ResourcesContainerRootUrl,
        ResourceContentInDB = source.ResourceContentInDB
    };
}
