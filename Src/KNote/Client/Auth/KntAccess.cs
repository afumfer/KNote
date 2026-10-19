using System.Security.Claims;
using KNote.Model;
using Microsoft.AspNetCore.Components.Authorization;
using Radzen;

namespace KNote.Client.Auth;

/// <summary>
/// The role a user needs for an action of the pages (create a note, edit it, manage folders...), as in ClientWin: the
/// buttons are always enabled, and the action is checked when it is chosen; if the user's role doesn't allow it, they
/// are told so (a notification) and nothing is done. The checks are synchronous (the authentication state the page
/// already has), so an action that opens a window can check first (the browser only opens it right after a click).
/// The Server checks again every request.
/// </summary>
public class KntAccess
{
    private readonly NotificationService notificationService;

    public KntAccess(NotificationService notificationService)
    {
        this.notificationService = notificationService;
    }

    /// <summary>Whether the user has the role or a higher one (Guest &lt; Staff &lt; ProjectManager &lt; Admin).</summary>
    public static bool HasRole(ClaimsPrincipal user, EnumRoles minimumRole) =>
        Enum.GetValues<EnumRoles>().Any(role => role >= minimumRole && user.IsInRole(role.ToString()));

    /// <summary>Whether the user can do the action (e.g. "Creating notes"); if not, tells them and returns false.</summary>
    public bool Check(ClaimsPrincipal user, EnumRoles minimumRole, string action)
    {
        if (HasRole(user, minimumRole))
            return true;

        var detail = user.Identity?.IsAuthenticated == true
            ? $"{action} requires the {minimumRole} role or a higher one. If you need it, ask an administrator."
            : $"{action} requires signing in with a user with the {minimumRole} role or a higher one.";
        notificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Warning,
            Summary = "Access denied",
            Detail = detail,
            Duration = 6000
        });
        return false;
    }

    /// <summary>Check with the page's authentication state (completed by then: no wait before the action).</summary>
    public async Task<bool> CheckAsync(Task<AuthenticationState> authenticationState, EnumRoles minimumRole, string action) =>
        Check((await authenticationState).User, minimumRole, action);
}
