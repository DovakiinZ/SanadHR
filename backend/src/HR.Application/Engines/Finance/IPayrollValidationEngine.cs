using HR.Domain.Engines.Finance;

namespace HR.Application.Engines.Finance;

/// <summary>Everything a validator needs, loaded once by the orchestrator: the period/currency, the
/// resolved inputs, the computed results, the rule-set compilation outcome, and any other runs that
/// overlap the same period/definition. Keeping this a plain data bag lets validators stay pure and
/// unit-testable.</summary>
public sealed record PayrollValidationContext
{
    public required PayrollPeriod Period { get; init; }
    public required string Currency { get; init; }
    public IReadOnlyList<EmployeePayrollInput> Inputs { get; init; } = new List<EmployeePayrollInput>();
    public IReadOnlyList<EmployeePayrollResult> Results { get; init; } = new List<EmployeePayrollResult>();
    public RuleCompilationResult? RuleCompilation { get; init; }

    /// <summary>Other runs (id + period) for the same definition that overlap this period.</summary>
    public IReadOnlyList<(Guid RunId, DateTime Start, DateTime End)> OverlappingRuns { get; init; }
        = new List<(Guid, DateTime, DateTime)>();
}

/// <summary>A single validation rule (specification). Implementations are discovered via DI and run by the
/// <see cref="IPayrollValidationEngine"/>. Each returns zero or more findings.</summary>
public interface IPayrollValidator
{
    string Code { get; }
    IEnumerable<ValidationFinding> Validate(PayrollValidationContext context);
}

/// <summary>Aggregates every registered <see cref="IPayrollValidator"/> into a single report. A run may
/// not advance to execution while the report contains any error.</summary>
public interface IPayrollValidationEngine
{
    ValidationReport Validate(PayrollValidationContext context);
}
