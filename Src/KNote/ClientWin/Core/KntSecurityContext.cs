using System.Reflection;
using KNote.Model;
using KNote.Service.Core;

namespace KNote.ClientWin.Core;

/// <summary>
/// Who is using the application in this session and with which roles, exposed as Store.Security. The user
/// name itself is Store.AppUserName (what every ServiceRef is created with); this holds the rest:
/// how that user signed in, the password typed in the sign-in dialog, and the role the user has in each
/// linked repository (set by Store.AuthenticateRepositoryAsync when the repository is linked).
/// </summary>
public class KntSecurityContext
{
    private readonly Dictionary<Guid, EnumRoles> _repositoryRoles = new();

    /// <summary>
    /// How the user signed in for this session. Fixed at startup: changing it in Options takes a restart.
    /// </summary>
    public AppAuthenticationMode AuthenticationMode { get; private set; } = AppAuthenticationMode.Windows;

    /// <summary>
    /// The password typed in the sign-in dialog (AppAuthenticationMode.Credentials), kept in memory only -
    /// never persisted - to authenticate every repository linked during this session with the same
    /// credentials. Null when signed in with the Windows account.
    /// </summary>
    public string Password { get; private set; }

    public void StartSession(AppAuthenticationMode authenticationMode, string password = null)
    {
        AuthenticationMode = authenticationMode;
        Password = authenticationMode == AppAuthenticationMode.Credentials ? password : null;
        _repositoryRoles.Clear();
    }

    // Repositories are identified by their service (IKntService.IdServiceRef, which is also ServiceRef's).
    public void SetRepositoryRole(IKntService service, EnumRoles role)
        => _repositoryRoles[service.IdServiceRef] = role;

    public void RemoveRepository(IKntService service)
        => _repositoryRoles.Remove(service.IdServiceRef);

    /// <summary>
    /// The user's role in a linked repository, or null when it has none there.
    /// </summary>
    public EnumRoles? GetRepositoryRole(IKntService service)
        => service != null && _repositoryRoles.TryGetValue(service.IdServiceRef, out var role) ? role : null;

    /// <summary>
    /// The user's role in a linked repository as shown to the user ("Project manager"...), or "no role".
    /// </summary>
    public string GetRepositoryRoleName(IKntService service)
    {
        var role = GetRepositoryRole(service);
        return role == null ? "no role" : KntConst.Roles[role.Value];
    }

    /// <summary>
    /// Whether the user meets a [KntAuthorize] requirement: no requirement is always met; an Application
    /// one is checked against ApplicationRole; a Repository one against the user's role in that repository
    /// (against ApplicationRole when there is no repository yet, e.g. a note editor before loading a note).
    /// </summary>
    public bool IsAuthorized(KntAuthorizeAttribute requirement, IKntService repository = null)
    {
        if (requirement == null)
            return true;

        var role = requirement.Scope == AuthorizationScope.Repository && repository != null
            ? GetRepositoryRole(repository)
            : ApplicationRole;

        return role >= requirement.MinimumRole;
    }

    /// <summary>
    /// Whether the user meets the requirement a type declares with [KntAuthorize]: a controller (its use
    /// case) or a Service command (e.g. to know beforehand whether saving a note will be allowed).
    /// </summary>
    public bool IsAuthorized(Type type, IKntService repository = null)
        => IsAuthorized(type.GetCustomAttribute<KntAuthorizeAttribute>(inherit: false), repository);

    /// <summary>
    /// What the use cases that don't belong to one repository (Options, AI providers, creating a
    /// repository...) are checked against: the highest role the user has across the linked repositories,
    /// and Guest at least for anybody who signed in, so a user without repositories can still link one.
    /// </summary>
    public EnumRoles ApplicationRole
        => _repositoryRoles.Count == 0 ? EnumRoles.Guest : _repositoryRoles.Values.Max();
}
