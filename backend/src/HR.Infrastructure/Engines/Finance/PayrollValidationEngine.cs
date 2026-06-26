using HR.Application.Engines.Finance;
using HR.Domain.Engines.Finance;

namespace HR.Infrastructure.Engines.Finance;

/// <summary>Runs every registered <see cref="IPayrollValidator"/> and merges their findings into one
/// report. Validators are discovered via DI, so adding a new check is just adding a class.</summary>
public sealed class PayrollValidationEngine : IPayrollValidationEngine
{
    private readonly IEnumerable<IPayrollValidator> _validators;

    public PayrollValidationEngine(IEnumerable<IPayrollValidator> validators) => _validators = validators;

    public ValidationReport Validate(PayrollValidationContext context)
    {
        var findings = new List<ValidationFinding>();
        foreach (var validator in _validators)
            findings.AddRange(validator.Validate(context));
        return ValidationReport.From(findings);
    }
}
