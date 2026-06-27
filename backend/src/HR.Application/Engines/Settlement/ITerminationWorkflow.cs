using HR.Modules.Employees.Entities;

namespace HR.Application.Engines.Settlement;

/// <summary>A lightweight, self-contained approval flow for employee termination. Requesting a
/// termination computes and freezes the end-of-service settlement and routes it through a
/// Manager → HR → Finance approval chain WITHOUT terminating the employee yet. On final approval the
/// employee is terminated, a pending settlement-payout Expense is created (for the Expenses approval
/// gate), and a printable settlement PDF is generated. Rejection leaves the employee untouched.</summary>
public interface ITerminationWorkflow
{
    /// <summary>Compute + freeze the settlement and open the approval chain (status = PendingApproval).</summary>
    Task<TerminationSettlement> RequestAsync(SettlementRequest request, CancellationToken ct = default);

    /// <summary>Approve or reject the current step. Final approval finalizes termination + expense + document.</summary>
    Task<TerminationSettlement> DecideAsync(Guid settlementId, bool approve, string? comment, CancellationToken ct = default);

    /// <summary>Settlements awaiting a decision the current user is allowed to make.</summary>
    Task<IReadOnlyList<TerminationSettlement>> GetPendingForCurrentUserAsync(CancellationToken ct = default);

    Task<TerminationSettlement> GetAsync(Guid settlementId, CancellationToken ct = default);
}
