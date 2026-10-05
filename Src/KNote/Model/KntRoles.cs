using System;
using System.Collections.Generic;
using System.Linq;

namespace KNote.Model;

/// <summary>
/// Reads User.RoleDefinition: the comma-separated list of EnumRoles names stored for each user (e.g.
/// "Staff, Admin"). Roles are hierarchical (see EnumRoles), so what decides a user's permissions is the
/// highest role in that list.
/// </summary>
public static class KntRoles
{
    /// <summary>
    /// The roles named in roleDefinition. Names are matched exactly (case-sensitive, as they are
    /// persisted); unknown names and numeric values are ignored rather than parsed as an EnumRoles value.
    /// </summary>
    public static List<EnumRoles> Parse(string roleDefinition)
    {
        if (string.IsNullOrWhiteSpace(roleDefinition))
            return new List<EnumRoles>();

        return roleDefinition
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Where(name => Enum.GetNames<EnumRoles>().Contains(name))
            .Select(Enum.Parse<EnumRoles>)
            .Distinct()
            .ToList();
    }

    /// <summary>
    /// The highest role in roleDefinition, or null when it names no known role.
    /// </summary>
    public static EnumRoles? Highest(string roleDefinition)
    {
        var roles = Parse(roleDefinition);
        return roles.Count == 0 ? null : roles.Max();
    }

    /// <summary>
    /// Whether roleDefinition grants minimumRole, either directly or through a higher role.
    /// </summary>
    public static bool IsInRole(string roleDefinition, EnumRoles minimumRole)
        => Highest(roleDefinition) >= minimumRole;
}
