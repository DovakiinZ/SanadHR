using HR.Domain.Engines.Settlement;
using HR.Domain.Enums;
using HR.Modules.Employees.Entities;

namespace HR.Application.Engines.Settlement;

/// <summary>A request to compute (and optionally persist) an end-of-service settlement for an
/// employee. The engine resolves the monthly wage and unpaid-leave days server-side; the caller
/// supplies the legal scenario and contract terms.</summary>
public record SettlementRequest
{
    public Guid EmployeeId { get; init; }
    public DateTime TerminationDate { get; init; }
    public TerminationScenario Scenario { get; init; } = TerminationScenario.NormalEmployerTermination;
    public ContractTermType ContractTermType { get; init; } = ContractTermType.Indefinite;
    public DateTime? ContractEndDate { get; init; }
    public string? Notes { get; init; }
}

/// <summary>Computes Saudi Labor Law end-of-service settlements (Articles 77/80/81 over the Art. 84/85
/// gratuity). Pure math lives in <see cref="EndOfServiceCalculator"/>; this engine loads the employee,
/// resolves the wage base and unpaid-leave days, and (for Settle) persists the result + flips the
/// employee's status.</summary>
public interface IEndOfServiceEngine
{
    /// <summary>Compute the award without writing anything.</summary>
    Task<SettlementResult> PreviewAsync(SettlementRequest request, CancellationToken ct = default);

    /// <summary>Compute, persist a <see cref="TerminationSettlement"/>, and set the employee's
    /// status/termination date/contract term.</summary>
    Task<TerminationSettlement> SettleAsync(SettlementRequest request, CancellationToken ct = default);
}
