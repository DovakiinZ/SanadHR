using HR.Domain.Enums;

namespace HR.Domain.Engines.Finance.StateMachine;

/// <summary>Raised when an illegal payroll-transaction lifecycle transition is attempted.</summary>
public sealed class InvalidPayrollTransactionStateException : Exception
{
    public PayrollTransactionStatus From { get; }
    public PayrollTransactionStatus To { get; }

    public InvalidPayrollTransactionStateException(PayrollTransactionStatus from, PayrollTransactionStatus to)
        : base($"Illegal payroll transaction transition: {from} → {to}.")
    {
        From = from;
        To = to;
    }
}

/// <summary>The single source of truth for the addition/deduction lifecycle. Draft→PendingApproval→Approved
/// (and Rejected/Cancelled) are driven by HR/Finance in sub-project 2A; Posted/CarriedForward/Reversed are
/// driven by the payroll engine in 2B. Cancelled and Reversed are terminal.</summary>
public static class PayrollTransactionStateMachine
{
    private static readonly IReadOnlyDictionary<PayrollTransactionStatus, PayrollTransactionStatus[]> Allowed =
        new Dictionary<PayrollTransactionStatus, PayrollTransactionStatus[]>
        {
            [PayrollTransactionStatus.Draft] = new[] { PayrollTransactionStatus.PendingApproval, PayrollTransactionStatus.Cancelled },
            [PayrollTransactionStatus.PendingApproval] = new[] { PayrollTransactionStatus.Approved, PayrollTransactionStatus.Rejected },
            [PayrollTransactionStatus.Rejected] = new[] { PayrollTransactionStatus.Draft },
            [PayrollTransactionStatus.Approved] = new[] { PayrollTransactionStatus.Cancelled, PayrollTransactionStatus.Posted, PayrollTransactionStatus.CarriedForward },
            [PayrollTransactionStatus.CarriedForward] = new[] { PayrollTransactionStatus.Posted, PayrollTransactionStatus.Cancelled },
            [PayrollTransactionStatus.Posted] = new[] { PayrollTransactionStatus.Reversed },
            [PayrollTransactionStatus.Cancelled] = Array.Empty<PayrollTransactionStatus>(),
            [PayrollTransactionStatus.Reversed] = Array.Empty<PayrollTransactionStatus>(),
        };

    public static IReadOnlyList<PayrollTransactionStatus> NextStates(PayrollTransactionStatus from) =>
        Allowed.TryGetValue(from, out var next) ? next : Array.Empty<PayrollTransactionStatus>();

    public static bool CanTransition(PayrollTransactionStatus from, PayrollTransactionStatus to) =>
        Allowed.TryGetValue(from, out var next) && Array.IndexOf(next, to) >= 0;

    /// <summary>Throws <see cref="InvalidPayrollTransactionStateException"/> if the transition is not permitted.</summary>
    public static void EnsureCanTransition(PayrollTransactionStatus from, PayrollTransactionStatus to)
    {
        if (!CanTransition(from, to)) throw new InvalidPayrollTransactionStateException(from, to);
    }

    /// <summary>True once the transaction is financially frozen (Posted). Posted records are corrected only
    /// via reversal (2B).</summary>
    public static bool IsImmutable(PayrollTransactionStatus state) => state is PayrollTransactionStatus.Posted;

    public static bool IsTerminal(PayrollTransactionStatus state) =>
        state is PayrollTransactionStatus.Cancelled or PayrollTransactionStatus.Reversed;

    /// <summary>True only in Draft — the sole state in which a transaction may be edited.</summary>
    public static bool IsEditable(PayrollTransactionStatus state) => state is PayrollTransactionStatus.Draft;
}
