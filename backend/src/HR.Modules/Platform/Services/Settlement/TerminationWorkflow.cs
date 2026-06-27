using HR.Application.Common.Exceptions;
using HR.Application.Common.Interfaces;
using HR.Application.Engines.Settlement;
using HR.Domain.Engines.Expenses;
using HR.Domain.Engines.Files;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using HR.Modules.Employees.Entities;
using HR.Modules.Platform.Services.Documents;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Services.Settlement;

/// <summary>Implements the lightweight termination approval flow over the existing EOS engine + document
/// renderer. Settlement is computed/frozen at submission; on final approval the employee is terminated,
/// a pending payout Expense is created, and a settlement PDF is generated (StoredFile served via
/// /api/files/{id}).</summary>
public sealed class TerminationWorkflow : ITerminationWorkflow
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IEndOfServiceEngine _eos;
    private readonly IDocumentRenderer _renderer;
    private readonly IAuditLogService _audit;

    public TerminationWorkflow(ApplicationDbContext db, ICurrentUserService currentUser, IEndOfServiceEngine eos,
        IDocumentRenderer renderer, IAuditLogService audit)
    {
        _db = db;
        _currentUser = currentUser;
        _eos = eos;
        _renderer = renderer;
        _audit = audit;
    }

    private static string PermissionFor(SettlementApproverRole role) => role switch
    {
        SettlementApproverRole.Manager => "Employees.Edit",
        SettlementApproverRole.HR => "Employees.Terminate",
        SettlementApproverRole.Finance => "Expenses.Approve",
        _ => "Employees.Terminate",
    };

    public async Task<TerminationSettlement> RequestAsync(SettlementRequest request, CancellationToken ct = default)
    {
        var employee = await _db.Employees.FirstOrDefaultAsync(e => e.Id == request.EmployeeId, ct)
            ?? throw new NotFoundException("Employee", request.EmployeeId);
        if (employee.Status is EmployeeStatus.Terminated or EmployeeStatus.Resigned)
            throw new ConflictException("This employee has already left the organization.");
        if (await _db.TerminationSettlements.AnyAsync(s => s.EmployeeId == employee.Id && s.Status == SettlementStatus.PendingApproval, ct))
            throw new ConflictException("A termination request is already pending approval for this employee.");

        var result = await _eos.PreviewAsync(request, ct);

        var settlement = new TerminationSettlement
        {
            EmployeeId = employee.Id,
            HireDate = DateTime.SpecifyKind(employee.HireDate, DateTimeKind.Utc),
            TerminationDate = DateTime.SpecifyKind(request.TerminationDate, DateTimeKind.Utc),
            Scenario = request.Scenario,
            ContractTermType = request.ContractTermType,
            MonthlyWage = result.MonthlyWage,
            DailyWage = result.DailyWage,
            ServiceYears = result.ServiceYears,
            EffectiveServiceDays = result.EffectiveServiceDays,
            UnpaidLeaveDays = result.UnpaidLeaveDays,
            GratuityAmount = result.GratuityAmount,
            Article77Award = result.Article77Award,
            NoticeCompensation = result.NoticeCompensation,
            TotalAward = result.TotalAward,
            Currency = result.Currency,
            ComputedByUserId = _currentUser.IsAuthenticated ? _currentUser.UserId : null,
            ComputedAt = DateTime.UtcNow,
            Notes = request.Notes,
            Status = SettlementStatus.PendingApproval,
            CurrentStep = 1,
        };
        foreach (var line in result.Lines)
            settlement.Items.Add(new TerminationSettlementItem
            { LabelEn = line.LabelEn, LabelAr = line.LabelAr, ArticleRef = line.ArticleRef, Amount = line.Amount });

        // Manager → HR → Finance. Manager step is pinned to the direct manager's user when resolvable.
        Guid? managerUserId = employee.ManagerId is { } mgrId
            ? await _db.Employees.Where(e => e.Id == mgrId).Select(e => e.UserId).FirstOrDefaultAsync(ct)
            : null;
        settlement.ApprovalSteps.Add(new TerminationApprovalStep { StepOrder = 1, Role = SettlementApproverRole.Manager, ApproverUserId = managerUserId });
        settlement.ApprovalSteps.Add(new TerminationApprovalStep { StepOrder = 2, Role = SettlementApproverRole.HR });
        settlement.ApprovalSteps.Add(new TerminationApprovalStep { StepOrder = 3, Role = SettlementApproverRole.Finance });

        _db.TerminationSettlements.Add(settlement);
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync("TerminationRequested", "Access.Termination", settlement.Id, null,
            new { employee.EmployeeNumber, result.TotalAward, request.Scenario }, ct);
        return await GetAsync(settlement.Id, ct);
    }

    public async Task<TerminationSettlement> DecideAsync(Guid settlementId, bool approve, string? comment, CancellationToken ct = default)
    {
        var settlement = await _db.TerminationSettlements
            .Include(s => s.ApprovalSteps)
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == settlementId, ct)
            ?? throw new NotFoundException("TerminationSettlement", settlementId);
        if (settlement.Status != SettlementStatus.PendingApproval)
            throw new ConflictException("This settlement is not awaiting approval.");

        var step = settlement.ApprovalSteps.FirstOrDefault(s => s.StepOrder == settlement.CurrentStep)
            ?? throw new ConflictException("No active approval step.");

        // Authorize: the pinned approver, or any user holding the step role's permission.
        var perm = PermissionFor(step.Role);
        var allowed = (_currentUser.IsAuthenticated && step.ApproverUserId == _currentUser.UserId)
            || _currentUser.Permissions.Contains(perm);
        if (!allowed) throw new ForbiddenException("You are not allowed to decide this approval step.");

        var actor = _currentUser.IsAuthenticated ? _currentUser.UserId : (Guid?)null;
        step.DecidedByUserId = actor;
        step.DecidedAt = DateTime.UtcNow;
        step.Comment = comment;

        if (!approve)
        {
            step.Status = SettlementApprovalStepStatus.Rejected;
            settlement.Status = SettlementStatus.Rejected;
            settlement.RejectionReason = comment;
            await _db.SaveChangesAsync(ct);
            await _audit.LogAsync("TerminationRejected", "Access.Termination", settlement.Id, null, new { step.Role, comment }, ct);
            return await GetAsync(settlement.Id, ct);
        }

        step.Status = SettlementApprovalStepStatus.Approved;
        var lastStep = settlement.ApprovalSteps.Max(s => s.StepOrder);
        if (settlement.CurrentStep < lastStep)
        {
            settlement.CurrentStep += 1;
            await _db.SaveChangesAsync(ct);
            await _audit.LogAsync("TerminationStepApproved", "Access.Termination", settlement.Id, null, new { step.Role }, ct);
            return await GetAsync(settlement.Id, ct);
        }

        await FinalizeAsync(settlement, ct);
        return await GetAsync(settlement.Id, ct);
    }

    private async Task FinalizeAsync(TerminationSettlement settlement, CancellationToken ct)
    {
        var employee = await _db.Employees.FirstOrDefaultAsync(e => e.Id == settlement.EmployeeId, ct)
            ?? throw new NotFoundException("Employee", settlement.EmployeeId);

        // Terminate the employee.
        employee.Status = settlement.Scenario is TerminationScenario.NormalResignation
            or TerminationScenario.Article81EmployerBreachResignation
            ? EmployeeStatus.Resigned : EmployeeStatus.Terminated;
        employee.TerminationDate = settlement.TerminationDate;
        employee.ContractTermType = settlement.ContractTermType;

        // Pending payout expense — goes to the Expenses approval gate.
        var expense = new Expense
        {
            EmployeeId = employee.Id,
            Amount = settlement.TotalAward,
            Currency = settlement.Currency,
            Description = $"مخالصة نهاية الخدمة — {employee.EmployeeNumber}",
            Status = "Pending",
            DecidedAt = DateTime.UtcNow,
        };
        _db.Expenses.Add(expense);
        settlement.ExpenseId = expense.Id;

        // Generate the settlement PDF and store it (served anonymously via /api/files/{id}).
        var pdfBytes = await RenderSettlementPdfAsync(settlement, employee, ct);
        var file = new StoredFile
        {
            FileName = $"settlement-{employee.EmployeeNumber}.pdf",
            ContentType = "application/pdf",
            Data = pdfBytes,
            SizeBytes = pdfBytes.LongLength,
            Category = "Settlement",
        };
        _db.Files.Add(file);
        settlement.DocumentFileId = file.Id;

        settlement.Status = SettlementStatus.Approved;
        settlement.ApprovedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync("TerminationApproved", "Access.Termination", settlement.Id,
            null, new { employee.EmployeeNumber, settlement.TotalAward, ExpenseId = expense.Id, DocId = file.Id }, ct);
    }

    private async Task<byte[]> RenderSettlementPdfAsync(TerminationSettlement s, Employee e, CancellationToken ct)
    {
        var name = $"{e.FirstNameAr ?? e.FirstName} {e.LastNameAr ?? e.LastName}".Trim();
        string m(decimal v) => $"{v:N2} {s.Currency}";
        var details = new List<(string, string)>
        {
            ("الموظف", name),
            ("الرقم الوظيفي", e.EmployeeNumber),
            ("تاريخ الالتحاق", s.HireDate.ToString("yyyy-MM-dd")),
            ("تاريخ نهاية الخدمة", s.TerminationDate.ToString("yyyy-MM-dd")),
            ("الأجر الشهري", m(s.MonthlyWage)),
            ("سنوات الخدمة", s.ServiceYears.ToString("0.00")),
        };
        foreach (var item in s.Items) details.Add((item.LabelAr, m(item.Amount)));
        details.Add(("الإجمالي المستحق", m(s.TotalAward)));

        var approvals = s.ApprovalSteps.OrderBy(x => x.StepOrder)
            .Select(x => (x.StepOrder, RoleAr(x.Role), x.Status.ToString())).ToList();

        var tokens = new Dictionary<string, string>
        {
            ["employee"] = name, ["employeeNumber"] = e.EmployeeNumber, ["total"] = m(s.TotalAward),
        };

        var req = new DocumentRenderRequest(
            TemplateId: null,
            FallbackTitle: "مخالصة نهاية الخدمة",
            RefNumber: e.EmployeeNumber,
            Tokens: tokens,
            DefaultDetails: details,
            Approvals: approvals,
            FileName: $"settlement-{e.EmployeeNumber}.pdf");
        var (pdf, _) = await _renderer.RenderDocumentAsync(req, ct);
        return pdf;
    }

    private static string RoleAr(SettlementApproverRole r) => r switch
    {
        SettlementApproverRole.Manager => "المدير المباشر",
        SettlementApproverRole.HR => "الموارد البشرية",
        SettlementApproverRole.Finance => "المالية",
        _ => r.ToString(),
    };

    public async Task<IReadOnlyList<TerminationSettlement>> GetPendingForCurrentUserAsync(CancellationToken ct = default)
    {
        var pending = await _db.TerminationSettlements
            .Include(s => s.ApprovalSteps)
            .Where(s => s.Status == SettlementStatus.PendingApproval)
            .ToListAsync(ct);

        return pending.Where(s =>
        {
            var step = s.ApprovalSteps.FirstOrDefault(x => x.StepOrder == s.CurrentStep);
            if (step is null) return false;
            return (_currentUser.IsAuthenticated && step.ApproverUserId == _currentUser.UserId)
                || _currentUser.Permissions.Contains(PermissionFor(step.Role));
        }).ToList();
    }

    public async Task<TerminationSettlement> GetAsync(Guid settlementId, CancellationToken ct = default)
        => await _db.TerminationSettlements.Include(s => s.Items).Include(s => s.ApprovalSteps)
            .FirstOrDefaultAsync(s => s.Id == settlementId, ct)
           ?? throw new NotFoundException("TerminationSettlement", settlementId);
}
