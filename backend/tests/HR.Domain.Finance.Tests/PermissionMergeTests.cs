using FluentAssertions;
using HR.Domain.Engines.Permissions;
using Xunit;

namespace HR.Domain.Finance.Tests;

// Unit tests for the unified access resolver merge logic (lives in HR.Domain.Engines.Permissions).
public class PermissionMergeTests
{
    private static readonly string[] None = System.Array.Empty<string>();

    [Fact]
    public void Unions_roles_direct_templates_and_allow_overrides()
    {
        var result = PermissionMerge.Resolve(
            rolePermissions: new[] { "Employees.View" },
            directPermissions: new[] { "Payroll.View" },
            templatePermissions: new[] { "Requests.Approve" },
            allowOverrides: new[] { "Settings.ManageUsers" },
            denyOverrides: None);

        result.Should().BeEquivalentTo(new[] { "Employees.View", "Payroll.View", "Requests.Approve", "Settings.ManageUsers" });
    }

    [Fact]
    public void Explicit_deny_wins_over_role_grant()
    {
        var result = PermissionMerge.Resolve(
            rolePermissions: new[] { "Employees.View", "Employees.Terminate" },
            directPermissions: None,
            templatePermissions: None,
            allowOverrides: None,
            denyOverrides: new[] { "Employees.Terminate" });

        result.Should().Contain("Employees.View");
        result.Should().NotContain("Employees.Terminate");
    }

    [Fact]
    public void Deny_wins_even_over_allow_override_and_template()
    {
        var result = PermissionMerge.Resolve(
            rolePermissions: None,
            directPermissions: None,
            templatePermissions: new[] { "Payroll.Approve" },
            allowOverrides: new[] { "Payroll.Approve" },
            denyOverrides: new[] { "Payroll.Approve" });

        result.Should().NotContain("Payroll.Approve");
    }

    [Fact]
    public void Is_case_insensitive_and_deduplicated()
    {
        var result = PermissionMerge.Resolve(
            rolePermissions: new[] { "Employees.View", "employees.view" },
            directPermissions: new[] { "EMPLOYEES.VIEW" },
            templatePermissions: None, allowOverrides: None, denyOverrides: None);

        result.Should().ContainSingle();
    }

    [Fact]
    public void Result_is_sorted_for_determinism()
    {
        var result = PermissionMerge.Resolve(
            new[] { "Zeta.View", "Alpha.View", "Mid.View" }, None, None, None, None);

        result.Should().Equal("Alpha.View", "Mid.View", "Zeta.View");
    }

    [Fact]
    public void Empty_sources_yield_empty()
    {
        PermissionMerge.Resolve(None, None, None, None, None).Should().BeEmpty();
    }
}
