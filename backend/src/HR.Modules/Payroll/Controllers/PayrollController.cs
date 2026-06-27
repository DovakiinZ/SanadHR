using System.Text.Json;
using HR.Api.Controllers;
using HR.Api.Filters;
using HR.Application.Common.Exceptions;
using HR.Application.Common.Models;
using HR.Application.Engines.Finance;
using HR.Domain.Engines.Finance;
using HR.Domain.Engines.Finance.Entities;
using HR.Infrastructure.Persistence;
using HR.Modules.Payroll.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Payroll.Controllers;

/// <summary>The Payroll application surface over the Financial Calculation Engine: provision a standard
/// payroll, preview, and drive a run through its lifecycle. Every endpoint is permission-gated; the engines
/// enforce the state machine, snapshots, validation and ledger posting.</summary>
[Authorize]
[ApiController]
[Route("api/payroll")]
public class PayrollController : BaseApiController
{
    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };

    private readonly ApplicationDbContext _db;
    private readonly IPayrollRunEngine _runEngine;
    private readonly IPayrollPreviewEngine _previewEngine;
    private readonly IPayrollExecutionScheduler _scheduler;
    private readonly IStandardPayrollSeeder _seeder;

    public PayrollController(ApplicationDbContext db, IPayrollRunEngine runEngine, IPayrollPreviewEngine previewEngine,
        IPayrollExecutionScheduler scheduler, IStandardPayrollSeeder seeder)
    {
        _db = db;
        _runEngine = runEngine;
        _previewEngine = previewEngine;
        _scheduler = scheduler;
        _seeder = seeder;
    }

    [HttpPost("bootstrap")]
    [RequirePermission("Payroll.Run")]
    public async Task<ActionResult<ApiResponse<Guid>>> Bootstrap(CancellationToken ct)
        => OkResponse(await _seeder.EnsureStandardMonthlyAsync(ct), "Standard monthly payroll is ready.");

    [HttpGet("definitions")]
    [RequirePermission("Payroll.View")]
    public async Task<ActionResult<ApiResponse<List<PayrollDefinitionDto>>>> Definitions(CancellationToken ct)
    {
        var defs = await _db.PayrollDefinitions.AsNoTracking()
            .OrderBy(d => d.Name)
            .Select(d => new PayrollDefinitionDto
            {
                Id = d.Id, Code = d.Code, Name = d.Name, NameAr = d.NameAr,
                Status = d.Status.ToString(), CurrentVersionId = d.CurrentVersionId,
            })
            .ToListAsync(ct);
        return OkResponse(defs);
    }

    [HttpPost("preview")]
    [RequirePermission("Payroll.View")]
    public async Task<ActionResult<ApiResponse<PayrollPreviewDto>>> Preview([FromBody] PreviewRequest req, CancellationToken ct)
    {
        var versionId = await ResolveVersionAsync(req.DefinitionId, ct);
        var preview = await _previewEngine.PreviewAsync(versionId, PayrollPeriod.Monthly(req.Year, req.Month), ct);
        return OkResponse(new PayrollPreviewDto
        {
            EmployeeCount = preview.EmployeeCount,
            GrossTotal = preview.GrossTotal,
            DeductionTotal = preview.DeductionTotal,
            NetTotal = preview.NetTotal,
            Currency = preview.Currency,
            IsValid = preview.Validation.IsValid,
            Findings = preview.Validation.Findings.Select(ToFindingDto).ToList(),
            Lines = preview.Lines.Select(l => new PayrollPreviewLineDto
            {
                EmployeeId = l.EmployeeId, EmployeeNumber = l.EmployeeNumber, EmployeeName = l.EmployeeName,
                Gross = l.Gross, Deductions = l.Deductions, Net = l.Net, HasErrors = l.HasErrors,
            }).ToList(),
        });
    }

    [HttpGet("runs")]
    [RequirePermission("Payroll.View")]
    public async Task<ActionResult<ApiResponse<List<PayrollRunListItem>>>> Runs(CancellationToken ct)
    {
        var runs = await _db.PayrollRuns.AsNoTracking()
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);
        return OkResponse(runs.Select(ToListItem).ToList());
    }

    [HttpGet("runs/{id:guid}")]
    [RequirePermission("Payroll.View")]
    public async Task<ActionResult<ApiResponse<PayrollRunDetail>>> Run(Guid id, CancellationToken ct)
        => OkResponse(await BuildDetail(id, ct));

    [HttpPost("runs")]
    [RequirePermission("Payroll.Run")]
    public async Task<ActionResult<ApiResponse<PayrollRunDetail>>> CreateRun([FromBody] CreateRunRequest req, CancellationToken ct)
    {
        var run = await _runEngine.CreateAsync(req.DefinitionId, PayrollPeriod.Monthly(req.Year, req.Month), ct);
        return CreatedResponse(await BuildDetail(run.Id, ct));
    }

    [HttpPost("runs/{id:guid}/calculate")]
    [RequirePermission("Payroll.Run")]
    public async Task<ActionResult<ApiResponse<PayrollRunDetail>>> Calculate(Guid id, CancellationToken ct)
    {
        await _runEngine.CalculateAsync(id, ct);
        return OkResponse(await BuildDetail(id, ct));
    }

    [HttpPost("runs/{id:guid}/validate")]
    [RequirePermission("Payroll.Run")]
    public async Task<ActionResult<ApiResponse<PayrollRunDetail>>> Validate(Guid id, CancellationToken ct)
    {
        await _runEngine.ValidateAsync(id, ct);
        return OkResponse(await BuildDetail(id, ct));
    }

    [HttpPost("runs/{id:guid}/submit")]
    [RequirePermission("Payroll.Run")]
    public async Task<ActionResult<ApiResponse<PayrollRunDetail>>> Submit(Guid id, CancellationToken ct)
    {
        await _runEngine.SubmitForApprovalAsync(id, ct);
        return OkResponse(await BuildDetail(id, ct));
    }

    [HttpPost("runs/{id:guid}/approve")]
    [RequirePermission("Payroll.Approve")]
    public async Task<ActionResult<ApiResponse<PayrollRunDetail>>> Approve(Guid id, CancellationToken ct)
    {
        await _runEngine.ApproveAsync(id, ct);
        return OkResponse(await BuildDetail(id, ct));
    }

    [HttpPost("runs/{id:guid}/execute")]
    [RequirePermission("Payroll.Lock")]
    public async Task<ActionResult<ApiResponse<PayrollRunDetail>>> Execute(Guid id, CancellationToken ct)
    {
        await _scheduler.EnqueueAsync(id, ct);
        return OkResponse(await BuildDetail(id, ct));
    }

    [HttpPost("runs/{id:guid}/cancel")]
    [RequirePermission("Payroll.Run")]
    public async Task<ActionResult<ApiResponse<PayrollRunDetail>>> Cancel(Guid id, [FromBody] CancelRunRequest req, CancellationToken ct)
    {
        await _runEngine.CancelAsync(id, string.IsNullOrWhiteSpace(req.Reason) ? "Cancelled" : req.Reason, ct);
        return OkResponse(await BuildDetail(id, ct));
    }

    // ---- helpers ----

    private async Task<Guid> ResolveVersionAsync(Guid definitionId, CancellationToken ct)
    {
        var versionId = await _db.PayrollDefinitions.Where(d => d.Id == definitionId)
            .Select(d => d.CurrentVersionId).FirstOrDefaultAsync(ct);
        return versionId ?? throw new ConflictException("This payroll definition has no published version.");
    }

    private async Task<PayrollRunDetail> BuildDetail(Guid id, CancellationToken ct)
    {
        var run = await _db.PayrollRuns.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new NotFoundException("PayrollRun", id);

        var payslips = await _db.PayrollPayslips.AsNoTracking()
            .Where(p => p.PayrollRunId == id)
            .OrderBy(p => p.EmployeeNumber)
            .Select(p => new PayslipDto
            {
                Id = p.Id, EmployeeId = p.EmployeeId, EmployeeNumber = p.EmployeeNumber, EmployeeName = p.EmployeeName,
                Currency = p.Currency, GrossEarnings = p.GrossEarnings, TotalDeductions = p.TotalDeductions,
                NetAmount = p.NetAmount, LedgerPosted = p.LedgerPosted, ComponentsJson = p.ComponentsJson,
            })
            .ToListAsync(ct);

        var transitions = await _db.PayrollRunTransitions.AsNoTracking()
            .Where(t => t.PayrollRunId == id)
            .OrderBy(t => t.At)
            .Select(t => new RunTransitionDto { FromState = t.FromState.ToString(), ToState = t.ToState.ToString(), At = t.At, Reason = t.Reason })
            .ToListAsync(ct);

        var detail = new PayrollRunDetail
        {
            Id = run.Id, RunNumber = run.RunNumber, PeriodStart = run.PeriodStart, PeriodEnd = run.PeriodEnd,
            State = run.State.ToString(), Currency = run.Currency, EmployeeCount = run.EmployeeCount,
            GrossTotal = run.GrossTotal, DeductionTotal = run.DeductionTotal, NetTotal = run.NetTotal, CreatedAt = run.CreatedAt,
            PayrollDefinitionId = run.PayrollDefinitionId, PayrollDefinitionVersionId = run.PayrollDefinitionVersionId,
            RuleSetVersionId = run.RuleSetVersionId, Notes = run.Notes, ValidatedAt = run.ValidatedAt, ApprovedAt = run.ApprovedAt,
            Payslips = payslips, Transitions = transitions,
        };

        if (!string.IsNullOrWhiteSpace(run.ValidationResultJson))
        {
            try
            {
                var findings = JsonSerializer.Deserialize<List<ValidationFinding>>(run.ValidationResultJson, Json) ?? new();
                detail.Validation = findings.Select(ToFindingDto).ToList();
            }
            catch (JsonException) { /* ignore malformed snapshot */ }
        }
        return detail;
    }

    private static PayrollRunListItem ToListItem(PayrollRun r) => new()
    {
        Id = r.Id, RunNumber = r.RunNumber, PeriodStart = r.PeriodStart, PeriodEnd = r.PeriodEnd,
        State = r.State.ToString(), Currency = r.Currency, EmployeeCount = r.EmployeeCount,
        GrossTotal = r.GrossTotal, DeductionTotal = r.DeductionTotal, NetTotal = r.NetTotal, CreatedAt = r.CreatedAt,
    };

    private static ValidationFindingDto ToFindingDto(ValidationFinding f) => new()
    {
        Code = f.Code, Severity = f.Severity.ToString(), Message = f.Message,
        EmployeeId = f.EmployeeId, EmployeeName = f.EmployeeName,
    };
}
