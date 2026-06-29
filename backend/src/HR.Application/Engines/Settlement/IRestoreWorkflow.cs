using HR.Modules.Employees.Entities;

namespace HR.Application.Engines.Settlement;

/// <summary>A lightweight approval flow for reinstating an employee who previously left the
/// organization (Terminated/Resigned). Requesting a restore opens a Manager → HR approval chain
/// WITHOUT reactivating the employee yet. On final approval the employee returns to Active and the
/// termination data is cleared. Rejection leaves the employee untouched.</summary>
public interface IRestoreWorkflow
{
    /// <summary>Open a restore approval chain for a former employee (status = PendingApproval).</summary>
    Task<EmployeeRestoreRequest> RequestAsync(Guid employeeId, string? reason, CancellationToken ct = default);

    /// <summary>Approve or reject the current step. Final approval reactivates the employee.</summary>
    Task<EmployeeRestoreRequest> DecideAsync(Guid requestId, bool approve, string? comment, CancellationToken ct = default);

    /// <summary>Restore requests awaiting a decision the current user is allowed to make.</summary>
    Task<IReadOnlyList<EmployeeRestoreRequest>> GetPendingForCurrentUserAsync(CancellationToken ct = default);

    Task<EmployeeRestoreRequest> GetAsync(Guid requestId, CancellationToken ct = default);
}
