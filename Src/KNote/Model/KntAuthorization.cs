using System;

namespace KNote.Model;

/// <summary>
/// What a KntAuthorizeAttribute's role is checked against: the user's role in one repository, or the
/// application role (the highest role the user has across every linked repository, Guest at least).
/// A Service command always acts on one repository, so for commands only Repository applies.
/// </summary>
public enum AuthorizationScope
{
    Repository,
    Application
}

/// <summary>
/// Declares the minimum role (EnumRoles hierarchy: a higher role includes the lower ones) needed to
/// run a use case - the KNote counterpart of ASP.NET Core's [Authorize(Roles = ...)]. Put on Service
/// command classes (checked by KntServiceBase.ExecuteCommand, see
/// KntCommandServiceBase.ValidateAuthorizationAsync). Not inherited: every class declares its own.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class KntAuthorizeAttribute : Attribute
{
    public EnumRoles MinimumRole { get; }

    public AuthorizationScope Scope { get; }

    public KntAuthorizeAttribute(EnumRoles minimumRole, AuthorizationScope scope = AuthorizationScope.Repository)
    {
        MinimumRole = minimumRole;
        Scope = scope;
    }
}

/// <summary>
/// Marks a Service command that must run without a registered user (authenticating, self-registering)
/// - the KNote counterpart of ASP.NET Core's [AllowAnonymous]. Not inherited.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class KntAllowAnonymousAttribute : Attribute
{
}
