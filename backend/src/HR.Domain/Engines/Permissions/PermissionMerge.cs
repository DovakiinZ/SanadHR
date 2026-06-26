namespace HR.Domain.Engines.Permissions;

/// <summary>Pure resolution of a user's effective permission set from its sources. The rule:
///
///   effective = (rolePermissions ∪ templatePermissions ∪ allowOverrides) − denyOverrides
///
/// An explicit deny always wins, even over a role or template grant. Side-effect free and case-insensitive
/// on permission codes, so it is fully unit-testable and the JWT, the [RequirePermission] checks and the UI
/// all resolve identically.</summary>
public static class PermissionMerge
{
    public static IReadOnlyList<string> Resolve(
        IEnumerable<string> rolePermissions,
        IEnumerable<string> directPermissions,
        IEnumerable<string> templatePermissions,
        IEnumerable<string> allowOverrides,
        IEnumerable<string> denyOverrides)
    {
        var granted = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        granted.UnionWith(rolePermissions);
        granted.UnionWith(directPermissions);
        granted.UnionWith(templatePermissions);
        granted.UnionWith(allowOverrides);

        // Explicit deny wins.
        granted.ExceptWith(denyOverrides);

        return granted.OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToList();
    }
}
