namespace HR.Application.Engines.Permissions;

/// <summary>Creates the ready-made permission templates (Super Admin, HR Manager, Payroll Officer,
/// Finance, Department Manager, Employee Self-Service, Read Only Auditor) for the current tenant.
/// Idempotent — only missing templates are created — so it can be re-run safely and called for existing
/// tenants.</summary>
public interface IAccessTemplateSeeder
{
    Task<int> EnsureDefaultsAsync(CancellationToken ct = default);
}
