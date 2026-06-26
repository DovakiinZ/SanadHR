using HR.Domain.Engines.Finance;
using HR.Domain.Engines.Finance.Entities;

namespace HR.Application.Engines.Finance;

/// <summary>Resolves the employee population for a payroll definition version and builds each employee's
/// fact bag for a period. This is the bridge between stored HR data (salary, allowances, additions,
/// deductions, GOSI rate, attendance) and the configurable rule engine — it produces facts, never
/// hardcoded calculations.</summary>
public interface IPayrollFactProvider
{
    Task<IReadOnlyList<EmployeePayrollInput>> BuildInputsAsync(
        PayrollDefinitionVersion version, PayrollPeriod period, CancellationToken ct = default);
}
