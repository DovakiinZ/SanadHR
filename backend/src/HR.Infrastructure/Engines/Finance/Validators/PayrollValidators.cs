using HR.Application.Engines.Finance;
using HR.Domain.Engines.Finance;

namespace HR.Infrastructure.Engines.Finance.Validators;

/// <summary>No employee may have a negative basic salary, and no computed net pay may be negative.</summary>
public sealed class NegativeSalaryValidator : IPayrollValidator
{
    public string Code => "NEGATIVE_SALARY";

    public IEnumerable<ValidationFinding> Validate(PayrollValidationContext ctx)
    {
        foreach (var i in ctx.Inputs)
        {
            if (i.Facts.TryGetValue("BasicSalary", out var v) && ToDecimal(v) < 0m)
                yield return ValidationFinding.Error(Code,
                    $"Employee {i.EmployeeNumber} has a negative basic salary.", i.EmployeeId, i.EmployeeName);
        }
        foreach (var r in ctx.Results)
        {
            if (r.Net < 0m)
                yield return ValidationFinding.Error(Code,
                    $"Employee {r.Input.EmployeeNumber} has a negative net pay ({r.Net}).",
                    r.EmployeeId, r.Input.EmployeeName);
        }
    }

    private static decimal ToDecimal(object? v) => v switch
    {
        decimal d => d, int i => i, long l => l, double db => (decimal)db, _ => 0m,
    };
}

/// <summary>The GOSI rate must be a sane percentage (0–50%).</summary>
public sealed class InvalidGosiValidator : IPayrollValidator
{
    public string Code => "INVALID_GOSI";

    public IEnumerable<ValidationFinding> Validate(PayrollValidationContext ctx)
    {
        foreach (var i in ctx.Inputs)
        {
            if (i.Facts.TryGetValue("GosiRate", out var v))
            {
                var rate = v switch { decimal d => d, int n => n, double db => (decimal)db, _ => -1m };
                if (rate < 0m || rate > 50m)
                    yield return ValidationFinding.Error(Code,
                        $"Employee {i.EmployeeNumber} has an invalid GOSI rate ({rate}%).", i.EmployeeId, i.EmployeeName);
            }
        }
    }
}

/// <summary>An employee may appear at most once in a run.</summary>
public sealed class DuplicateEmployeeValidator : IPayrollValidator
{
    public string Code => "DUPLICATE_EMPLOYEE";

    public IEnumerable<ValidationFinding> Validate(PayrollValidationContext ctx)
    {
        foreach (var dup in ctx.Inputs.GroupBy(i => i.EmployeeId).Where(g => g.Count() > 1))
        {
            var first = dup.First();
            yield return ValidationFinding.Error(Code,
                $"Employee {first.EmployeeNumber} appears {dup.Count()} times in this run.",
                first.EmployeeId, first.EmployeeName);
        }
    }
}

/// <summary>No other run for the same definition may cover an overlapping period.</summary>
public sealed class OverlappingPayrollValidator : IPayrollValidator
{
    public string Code => "OVERLAPPING_PAYROLL";

    public IEnumerable<ValidationFinding> Validate(PayrollValidationContext ctx)
    {
        if (ctx.OverlappingRuns.Count > 0)
            yield return ValidationFinding.Error(Code,
                $"This period overlaps {ctx.OverlappingRuns.Count} existing run(s) for the same payroll definition.");
    }
}

/// <summary>Every employee's currency must match the run currency.</summary>
public sealed class CurrencyValidator : IPayrollValidator
{
    public string Code => "CURRENCY_MISMATCH";

    public IEnumerable<ValidationFinding> Validate(PayrollValidationContext ctx)
    {
        foreach (var i in ctx.Inputs)
        {
            if (!string.Equals(i.Currency, ctx.Currency, StringComparison.OrdinalIgnoreCase))
                yield return ValidationFinding.Error(Code,
                    $"Employee {i.EmployeeNumber} currency ({i.Currency}) differs from the run currency ({ctx.Currency}).",
                    i.EmployeeId, i.EmployeeName);
        }
    }
}

/// <summary>Warn when an employee has no attendance data for the period (may distort attendance-driven
/// components). Non-blocking.</summary>
public sealed class MissingAttendanceValidator : IPayrollValidator
{
    public string Code => "MISSING_ATTENDANCE";

    public IEnumerable<ValidationFinding> Validate(PayrollValidationContext ctx)
    {
        foreach (var i in ctx.Inputs.Where(x => !x.HasAttendanceData))
            yield return ValidationFinding.Warning(Code,
                $"Employee {i.EmployeeNumber} has no attendance records for the period.", i.EmployeeId, i.EmployeeName);
    }
}

/// <summary>Surfaces rule-set compilation problems (parse errors, dependency cycles) as blocking errors.</summary>
public sealed class RuleConflictValidator : IPayrollValidator
{
    public string Code => "RULE_CONFLICT";

    public IEnumerable<ValidationFinding> Validate(PayrollValidationContext ctx)
    {
        if (ctx.RuleCompilation is { IsValid: false } rc)
            foreach (var err in rc.Errors)
                yield return ValidationFinding.Error(Code, err);
    }
}
