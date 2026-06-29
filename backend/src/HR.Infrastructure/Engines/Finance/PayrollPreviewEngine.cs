using HR.Application.Engines.Finance;
using HR.Domain.Engines.Finance;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HR.Infrastructure.Engines.Finance;

/// <summary>Computes a full preview — calculate, validate, summarize — without any database writes.</summary>
public sealed class PayrollPreviewEngine : IPayrollPreviewEngine
{
    private readonly ApplicationDbContext _db;
    private readonly PayrollComputation _computation;
    private readonly IPayrollValidationEngine _validation;

    public PayrollPreviewEngine(ApplicationDbContext db, PayrollComputation computation, IPayrollValidationEngine validation)
    {
        _db = db;
        _computation = computation;
        _validation = validation;
    }

    public async Task<PayrollPreview> PreviewAsync(
        Guid payrollDefinitionVersionId, PayrollPeriod period, CancellationToken ct = default)
    {
        var version = await _db.PayrollDefinitionVersions.AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == payrollDefinitionVersionId, ct)
            ?? throw new InvalidOperationException($"Payroll definition version {payrollDefinitionVersionId} not found.");

        var computation = await _computation.ComputeAsync(version, period, null, ct);
        var overlapping = await _computation.OverlappingRunsAsync(version.PayrollDefinitionId, period, null, ct);

        var context = new PayrollValidationContext
        {
            Period = period,
            Currency = version.Currency,
            Inputs = computation.Inputs,
            Results = computation.Results,
            RuleCompilation = computation.Compilation,
            OverlappingRuns = overlapping,
        };
        var report = _validation.Validate(context);

        var errorEmployeeIds = report.Errors.Where(e => e.EmployeeId.HasValue).Select(e => e.EmployeeId!.Value).ToHashSet();

        var lines = computation.Results.Select(r => new PayrollPreviewLine(
            r.EmployeeId, r.Input.EmployeeNumber, r.Input.EmployeeName,
            r.Gross, r.Deductions, r.Net,
            errorEmployeeIds.Contains(r.EmployeeId) || r.Warnings.Count > 0)).ToList();

        return new PayrollPreview
        {
            EmployeeCount = computation.Results.Count,
            GrossTotal = Math.Round(computation.Results.Sum(r => r.Gross), 2),
            DeductionTotal = Math.Round(computation.Results.Sum(r => r.Deductions), 2),
            NetTotal = Math.Round(computation.Results.Sum(r => r.Net), 2),
            Currency = version.Currency,
            Validation = report,
            Lines = lines,
        };
    }
}
