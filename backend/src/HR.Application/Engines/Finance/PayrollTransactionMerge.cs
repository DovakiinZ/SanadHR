using HR.Domain.Engines.Finance;
using HR.Domain.Enums;

namespace HR.Application.Engines.Finance;

/// <summary>Folds consumed payroll transactions into a rule-set evaluation as per-record components,
/// recomputing gross/deductions/net. Pure — unit-testable without a DB or rule engine.</summary>
public static class PayrollTransactionMerge
{
    /// <summary>Stable component-code prefix identifying a transaction-sourced line. The ledger mapper
    /// parses the transaction id from the suffix to tag the posting.</summary>
    public const string ComponentCodePrefix = "TXN:";

    public static RuleSetEvaluation Apply(RuleSetEvaluation evaluation, IReadOnlyList<ConsumableTransaction> txns)
    {
        if (txns.Count == 0) return evaluation;

        var components = new List<ComponentResult>(evaluation.Components);
        var order = new List<string>(evaluation.ExecutionOrder);
        var gross = evaluation.GrossEarnings;
        var deductions = evaluation.TotalDeductions;

        foreach (var t in txns)
        {
            var kind = t.Kind == PayrollTransactionKind.Addition ? PayComponentKind.Earning : PayComponentKind.Deduction;
            var code = $"{ComponentCodePrefix}{t.TransactionId:N}";
            components.Add(new ComponentResult(code, t.TypeCode, kind, t.Amount, true));
            order.Add(code);
            if (kind == PayComponentKind.Earning) gross += t.Amount; else deductions += t.Amount;
        }

        return new RuleSetEvaluation(
            components, order,
            Math.Round(gross, 2, MidpointRounding.AwayFromZero),
            Math.Round(deductions, 2, MidpointRounding.AwayFromZero),
            Math.Round(gross - deductions, 2, MidpointRounding.AwayFromZero));
    }
}
