namespace HR.Application.Engines.Finance;

/// <summary>Provisions a ready-to-run "Standard Monthly" payroll for the current tenant: a published rule
/// set (BASIC + allowances + additions as earnings; GOSI + deductions as deductions) and a Monthly
/// payroll definition pinned to it. Idempotent — safe to call repeatedly; returns the definition id.</summary>
public interface IStandardPayrollSeeder
{
    Task<Guid> EnsureStandardMonthlyAsync(CancellationToken ct = default);
}
