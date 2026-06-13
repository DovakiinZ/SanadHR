using System.Text.Json;
using HR.Domain.Engines.Documents;
using HR.Domain.Engines.Forms;
using HR.Domain.Engines.MasterData;
using HR.Domain.Engines.Requests;
using HR.Domain.Engines.Workflows;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Services.Requests;

/// <summary>
/// Provisions built-in System Requests fully (form + workflow + impact + print template).
/// Everything is object-referenced (category/leave-type via master data), never free text.
/// </summary>
public sealed class RequestSeeder : IRequestSeeder
{
    private readonly ApplicationDbContext _db;
    private static readonly JsonSerializerOptions Json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public RequestSeeder(ApplicationDbContext db) => _db = db;

    public async Task<int> SeedSystemRequestsAsync(CancellationToken ct)
    {
        // 0) Object references (created if missing — governance: requests reference these).
        var catTimeOff = await EnsureCategory("TIME_OFF", "الإجازات والحضور", "Time Off & Attendance", ct);
        var catFinance = await EnsureCategory("FINANCE", "المالية والرواتب", "Finance & Payroll", ct);
        var catLetters = await EnsureCategory("LETTERS", "الخطابات والشهادات", "Letters & Certificates", ct);
        var catHr = await EnsureCategory("HR", "الموارد البشرية والشخصية", "HR & Personal", ct);

        await EnsureLeaveTypesAsync(ct);
        await EnsureExpenseCategoriesAsync(ct);
        await EnsureLoanTypesAsync(ct);

        // 1) Print templates (official documents — full PDF rendering arrives with QuestPDF).
        var tplLeave = await EnsureDocTemplate("DOC_LEAVE_APPROVAL", "Leave Approval", "موافقة إجازة", LeaveApprovalHtml, ct);
        var tplSalaryCert = await EnsureDocTemplate("DOC_SALARY_CERTIFICATE", "Salary Certificate", "شهادة تعريف بالراتب", SalaryCertificateHtml, ct);

        // 2) Workflows (separate entities — requests reference them by id).
        var wfManager = await EnsureWorkflow("WF_REQ_MANAGER", "Manager Approval", "موافقة المدير",
            new[] { Step(ApproverType.DirectManager, "المدير المباشر", "Direct Manager") }, ct);
        var wfMgrFinPay = await EnsureWorkflow("WF_REQ_MGR_FIN_PAY", "Manager → Finance → Payroll", "المدير ← المالية ← الرواتب",
            new[] { Step(ApproverType.DirectManager, "المدير المباشر", "Direct Manager"), Step(ApproverType.SystemAdmin, "المالية", "Finance"), Step(ApproverType.SystemAdmin, "الرواتب", "Payroll") }, ct);
        var wfMgrFin = await EnsureWorkflow("WF_REQ_MGR_FIN", "Manager → Finance", "المدير ← المالية",
            new[] { Step(ApproverType.DirectManager, "المدير المباشر", "Direct Manager"), Step(ApproverType.SystemAdmin, "المالية", "Finance") }, ct);
        var wfMgrDeptHr = await EnsureWorkflow("WF_REQ_MGR_DEPT_HR", "Manager → Dept → HR", "المدير ← مدير الإدارة ← الموارد البشرية",
            new[] { Step(ApproverType.DirectManager, "المدير المباشر", "Direct Manager"), Step(ApproverType.DepartmentHead, "مدير الإدارة", "Department Manager"), Step(ApproverType.HrManager, "الموارد البشرية", "HR") }, ct);
        var wfHr = await EnsureWorkflow("WF_REQ_HR", "HR Approval", "موافقة الموارد البشرية",
            new[] { Step(ApproverType.HrManager, "الموارد البشرية", "HR") }, ct);

        // 3) System requests, each fully provisioned. The sub-type is a FIELD (object ref),
        //    not a separate request — one Leave Request, leave type selected inside.
        int created = 0;
        created += await EnsureRequest("LEAVE_REQUEST", "طلب إجازة", "Leave Request", catTimeOff, null, wfManager, tplLeave,
            LeaveRequestForm(), Impact(leave: true, attendance: true, document: true), "CalendarDays", "#34D399", ct);
        created += await EnsureRequest("SALARY_CERTIFICATE", "شهادة راتب", "Salary Certificate", catLetters, null, wfHr, tplSalaryCert,
            SalaryCertificateForm(), Impact(document: true), "FileText", "#60A5FA", ct);
        created += await EnsureRequest("SALARY_ADVANCE", "سلفة راتب", "Salary Advance", catFinance, null, wfMgrFinPay, null,
            AmountForm("FORM_SALARY_ADVANCE", "نموذج سلفة راتب", "Salary Advance Form"),
            Impact(payroll: true, createsLoan: true, finance: true), "Wallet", "#FBBF24", ct);
        created += await EnsureRequest("LOAN_REQUEST", "طلب قرض", "Loan Request", catFinance, null, wfMgrFin, null,
            LoanForm(), Impact(loans: true, createsLoan: true, finance: true), "HandCoins", "#A78BFA", ct);
        created += await EnsureRequest("ATTENDANCE_CORRECTION", "تصحيح حضور", "Attendance Correction", catTimeOff, null, wfManager, null,
            AttendanceCorrectionForm(), Impact(attendance: true), "Clock", "#22D3EE", ct);
        created += await EnsureRequest("BUSINESS_TRIP", "رحلة عمل", "Business Trip", catHr, null, wfMgrDeptHr, null,
            BusinessTripForm(), Impact(document: false), "Briefcase", "#94A3B8", ct);
        created += await EnsureRequest("EXPENSE_CLAIM", "مطالبة مصروف", "Expense Claim", catFinance, null, wfMgrFin, null,
            ExpenseForm(), Impact(expenses: true, finance: true), "Receipt", "#F472B6", ct);
        created += await EnsureRequest("EMPLOYEE_DATA_UPDATE", "تحديث بيانات", "Employee Data Update", catHr, null, wfHr, null,
            DataUpdateForm(), Impact(), "UserCog", "#A3E635", ct);

        // Retire the old per-leave-type requests (superseded by the single Leave Request).
        await DeactivateRequestsAsync(new[] { "ANNUAL_LEAVE", "SICK_LEAVE", "EMERGENCY_LEAVE" }, ct);

        await _db.SaveChangesAsync(ct);
        return created;
    }

    private async Task DeactivateRequestsAsync(string[] codes, CancellationToken ct)
    {
        var stale = await _db.RequestTypes.Where(t => codes.Contains(t.Code) && t.IsActive).ToListAsync(ct);
        foreach (var t in stale) t.IsActive = false;
    }

    // ── Ensure helpers ──────────────────────────────────────────────────────────

    private async Task<Guid> EnsureCategory(string code, string ar, string en, CancellationToken ct)
        => await EnsureMasterData(MasterDataObjectType.RequestCategory, code, ar, en, ct);

    private async Task<Guid> EnsureMasterData(string objectType, string code, string ar, string en, CancellationToken ct)
    {
        var existing = await _db.MasterDataItems.FirstOrDefaultAsync(m => m.ObjectType == objectType && m.Code == code, ct);
        if (existing is not null) return existing.Id;
        var item = new MasterDataItem { ObjectType = objectType, Code = code, NameAr = ar, NameEn = en, IsSystemDefault = true, IsActive = true };
        _db.MasterDataItems.Add(item);
        await _db.SaveChangesAsync(ct);
        return item.Id;
    }

    /// <summary>Upsert a master-data item, backfilling rules MetadataJson when absent.</summary>
    private async Task EnsureMasterDataWithMeta(string objectType, string code, string ar, string en, string metaJson, CancellationToken ct)
    {
        var existing = await _db.MasterDataItems.FirstOrDefaultAsync(m => m.ObjectType == objectType && m.Code == code, ct);
        if (existing is null)
            _db.MasterDataItems.Add(new MasterDataItem { ObjectType = objectType, Code = code, NameAr = ar, NameEn = en, MetadataJson = metaJson, IsSystemDefault = true, IsActive = true });
        else if (string.IsNullOrWhiteSpace(existing.MetadataJson))
            existing.MetadataJson = metaJson; // backfill rules on items seeded before rules existed
        await _db.SaveChangesAsync(ct);
    }

    // 8 leave types, each carrying its own rules (paid/percentage/maxDays/attachment/...).
    private async Task EnsureLeaveTypesAsync(CancellationToken ct)
    {
        var lt = MasterDataObjectType.LeaveType;
        await EnsureMasterDataWithMeta(lt, "ANNUAL", "إجازة سنوية", "Annual Leave", LeaveRulesJson(maxDays: 30, annual: 30), ct);
        await EnsureMasterDataWithMeta(lt, "SICK", "إجازة مرضية", "Sick Leave", LeaveRulesJson(maxDays: 120, annual: 120, attach: true, payroll: true), ct);
        await EnsureMasterDataWithMeta(lt, "EMERGENCY", "إجازة طارئة", "Emergency Leave", LeaveRulesJson(maxDays: 5, annual: 5), ct);
        await EnsureMasterDataWithMeta(lt, "EXAM", "إجازة اختبارات", "Exam Leave", LeaveRulesJson(maxDays: 10, annual: 10, attach: true), ct);
        await EnsureMasterDataWithMeta(lt, "UNPAID", "إجازة بدون راتب", "Unpaid Leave", LeaveRulesJson(paid: false, pct: 0, maxDays: 90, annual: 0, payroll: true), ct);
        await EnsureMasterDataWithMeta(lt, "MARRIAGE", "إجازة زواج", "Marriage Leave", LeaveRulesJson(maxDays: 5, annual: 5), ct);
        await EnsureMasterDataWithMeta(lt, "DEATH", "إجازة وفاة", "Bereavement Leave", LeaveRulesJson(maxDays: 5, annual: 5), ct);
        await EnsureMasterDataWithMeta(lt, "HAJJ", "إجازة حج", "Hajj Leave", LeaveRulesJson(maxDays: 10, annual: 10), ct);
    }

    private async Task EnsureExpenseCategoriesAsync(CancellationToken ct)
    {
        var t = MasterDataObjectType.ExpenseCategory;
        await EnsureMasterData(t, "TRAVEL", "سفر", "Travel", ct);
        await EnsureMasterData(t, "MEALS", "وجبات", "Meals", ct);
        await EnsureMasterData(t, "ACCOMMODATION", "إقامة", "Accommodation", ct);
        await EnsureMasterData(t, "SUPPLIES", "مستلزمات", "Supplies", ct);
        await EnsureMasterData(t, "OTHER", "أخرى", "Other", ct);
    }

    private async Task EnsureLoanTypesAsync(CancellationToken ct)
    {
        var t = MasterDataObjectType.LoanType;
        await EnsureMasterData(t, "PERSONAL", "قرض شخصي", "Personal Loan", ct);
        await EnsureMasterData(t, "EMERGENCY", "قرض طارئ", "Emergency Loan", ct);
        await EnsureMasterData(t, "HOUSING", "قرض سكني", "Housing Loan", ct);
    }

    private static string LeaveRulesJson(bool paid = true, double pct = 100, double maxDays = 30, double annual = 30,
        bool attach = false, bool payroll = false, bool attendance = true, bool weekends = false)
        => JsonSerializer.Serialize(new LeaveRules
        {
            Paid = paid, PaidPercentage = pct, MaxDays = maxDays, AnnualBalance = annual,
            RequiresAttachment = attach, AffectsPayroll = payroll, AffectsAttendance = attendance, CountWeekends = weekends,
        }, Json);

    /// <summary>Field options descriptor that tells the UI to load choices live from master data.</summary>
    private static string Lookup(string objectType) => $"{{\"lookup\":\"{objectType}\"}}";

    private async Task<Guid> EnsureDocTemplate(string code, string en, string ar, string html, CancellationToken ct)
    {
        var existing = await _db.DocumentTemplates.FirstOrDefaultAsync(d => d.Code == code, ct);
        if (existing is not null) return existing.Id;
        var tpl = new DocumentTemplate
        {
            Code = code, NameEn = en, NameAr = ar, Module = "Requests",
            Status = DocumentTemplateStatus.Published, OutputFormat = DocumentOutputFormat.Pdf,
            BodyTemplate = html, UseBranding = true, IsActive = true,
        };
        _db.DocumentTemplates.Add(tpl);
        await _db.SaveChangesAsync(ct);
        return tpl.Id;
    }

    private static WorkflowStepConfig Step(ApproverType type, string ar, string en)
        => new() { ApproverType = (int)type, NameAr = ar, NameEn = en };

    private async Task<Guid> EnsureWorkflow(string code, string en, string ar, WorkflowStepConfig[] steps, CancellationToken ct)
    {
        var existing = await _db.WorkflowDefinitions.FirstOrDefaultAsync(w => w.Code == code, ct);
        if (existing is not null) return existing.Id;
        var def = new WorkflowDefinition { Code = code, NameEn = en, NameAr = ar, TriggerEntityType = "RequestInstance", IsActive = true };
        _db.WorkflowDefinitions.Add(def);
        await _db.SaveChangesAsync(ct);
        var version = new WorkflowVersion
        {
            WorkflowDefinitionId = def.Id, VersionNumber = 1, IsPublished = true, PublishedAt = DateTime.UtcNow,
            Configuration = JsonSerializer.Serialize(new WorkflowChainConfig { Steps = steps.ToList() }, Json),
        };
        _db.WorkflowVersions.Add(version);
        await _db.SaveChangesAsync(ct);
        return def.Id;
    }

    private async Task<Guid> EnsureForm(FormSpec spec, CancellationToken ct)
    {
        var existing = await _db.FormDefinitions.FirstOrDefaultAsync(f => f.Code == spec.Code, ct);
        if (existing is not null) return existing.Id;
        var form = new FormDefinition { Code = spec.Code, NameEn = spec.NameEn, NameAr = spec.NameAr, Module = "Requests", IsPublished = true, IsActive = true };
        for (int i = 0; i < spec.Fields.Count; i++)
        {
            var f = spec.Fields[i];
            form.Fields.Add(new FormField
            {
                Code = f.Code, NameAr = f.NameAr, NameEn = f.NameEn, FieldType = f.Type,
                IsRequired = f.Required, SortOrder = i, Placeholder = f.Placeholder, Options = f.Options,
            });
        }
        _db.FormDefinitions.Add(form);
        await _db.SaveChangesAsync(ct);
        return form.Id;
    }

    private async Task<int> EnsureRequest(string code, string ar, string en, Guid categoryId, Guid? leaveTypeId,
        Guid workflowId, Guid? printTemplateId, FormSpec formSpec, ImpactSpec impact, string icon, string color, CancellationToken ct)
    {
        if (await _db.RequestTypes.AnyAsync(t => t.Code == code, ct)) return 0;
        var formId = await EnsureForm(formSpec, ct);
        var type = new RequestType
        {
            Code = code, NameAr = ar, NameEn = en, Kind = RequestKind.System, IsSystem = true, IsActive = true,
            CategoryId = categoryId, FormDefinitionId = formId, WorkflowDefinitionId = workflowId,
            PrintTemplateId = printTemplateId, LeaveTypeId = leaveTypeId, Icon = icon, Color = color,
            ImpactMapping = new RequestImpactMapping
            {
                AffectsLeaveBalance = impact.Leave, AffectsAttendance = impact.Attendance, AffectsPayroll = impact.Payroll,
                AffectsExpenses = impact.Expenses, AffectsLoans = impact.Loans, CreatesLoanRecord = impact.CreatesLoan,
                RequiresFinanceApproval = impact.Finance, GeneratesDocument = impact.Document,
            },
        };
        _db.RequestTypes.Add(type);
        return 1;
    }

    // ── Form definitions (system fields use canonical codes the engine understands) ──

    // One generic Leave Request form — the leave type is a field, sourced live from settings.
    private static FormSpec LeaveRequestForm() => new("FORM_LEAVE_REQUEST", "نموذج طلب إجازة", "Leave Request Form", new()
    {
        F("leaveType", "نوع الإجازة", "Leave Type", FieldType.Dropdown, true, options: Lookup(MasterDataObjectType.LeaveType)),
        F("startDate", "تاريخ البداية", "Start Date", FieldType.Date, true),
        F("endDate", "تاريخ النهاية", "End Date", FieldType.Date, true),
        F("attachment", "مرفق", "Attachment", FieldType.File, false),
        F("notes", "ملاحظات", "Notes", FieldType.TextArea, false),
    });

    private static FormSpec SalaryCertificateForm() => new("FORM_SALARY_CERTIFICATE", "نموذج شهادة راتب", "Salary Certificate Form", new()
    {
        F("addressedTo", "موجهة إلى", "Addressed To", FieldType.Text, true),
        F("purpose", "الغرض", "Purpose", FieldType.TextArea, true),
    });

    private static FormSpec AmountForm(string code, string ar, string en) => new(code, ar, en, new()
    {
        F("amount", "المبلغ", "Amount", FieldType.Currency, true),
        F("reason", "السبب", "Reason", FieldType.TextArea, true),
    });

    private static FormSpec LoanForm() => new("FORM_LOAN_REQUEST", "نموذج طلب قرض", "Loan Request Form", new()
    {
        F("loanType", "نوع القرض", "Loan Type", FieldType.Dropdown, true, options: Lookup(MasterDataObjectType.LoanType)),
        F("amount", "المبلغ", "Amount", FieldType.Currency, true),
        F("installmentMonths", "عدد الأشهر", "Installment Months", FieldType.Number, true),
        F("reason", "السبب", "Reason", FieldType.TextArea, true),
    });

    private static FormSpec AttendanceCorrectionForm() => new("FORM_ATTENDANCE_CORRECTION", "نموذج تصحيح حضور", "Attendance Correction Form", new()
    {
        F("startDate", "التاريخ", "Date", FieldType.Date, true),
        F("reason", "السبب", "Reason", FieldType.TextArea, true),
    });

    private static FormSpec BusinessTripForm() => new("FORM_BUSINESS_TRIP", "نموذج رحلة عمل", "Business Trip Form", new()
    {
        F("destination", "الوجهة", "Destination", FieldType.Text, true),
        F("startDate", "تاريخ البداية", "Start Date", FieldType.Date, true),
        F("endDate", "تاريخ النهاية", "End Date", FieldType.Date, true),
        F("purpose", "الغرض", "Purpose", FieldType.TextArea, true),
    });

    private static FormSpec ExpenseForm() => new("FORM_EXPENSE_CLAIM", "نموذج مطالبة مصروف", "Expense Claim Form", new()
    {
        F("expenseCategory", "فئة المصروف", "Expense Category", FieldType.Dropdown, true, options: Lookup(MasterDataObjectType.ExpenseCategory)),
        F("amount", "المبلغ", "Amount", FieldType.Currency, true),
        F("description", "الوصف", "Description", FieldType.TextArea, true),
        F("receipt", "الإيصال", "Receipt", FieldType.File, false),
    });

    private static FormSpec DataUpdateForm() => new("FORM_DATA_UPDATE", "نموذج تحديث بيانات", "Data Update Form", new()
    {
        F("reason", "ما الذي تريد تحديثه؟", "What do you want to update?", FieldType.TextArea, true),
    });

    private static FieldSpec F(string code, string ar, string en, FieldType type, bool required, string? placeholder = null, string? options = null)
        => new(code, ar, en, type, required, placeholder, options);

    private static ImpactSpec Impact(bool leave = false, bool attendance = false, bool payroll = false,
        bool expenses = false, bool loans = false, bool createsLoan = false, bool finance = false, bool document = false)
        => new(leave, attendance, payroll, expenses, loans, createsLoan, finance, document);

    private sealed record FormSpec(string Code, string NameAr, string NameEn, List<FieldSpec> Fields);
    private sealed record FieldSpec(string Code, string NameAr, string NameEn, FieldType Type, bool Required, string? Placeholder = null, string? Options = null);
    private sealed record ImpactSpec(bool Leave, bool Attendance, bool Payroll, bool Expenses, bool Loans, bool CreatesLoan, bool Finance, bool Document);

    // ── Official document HTML (tokens resolved at generation; styled for print) ──

    private const string LeaveApprovalHtml = @"
<div style='font-family:sans-serif;direction:rtl'>
  <h2 style='text-align:center'>موافقة على طلب إجازة</h2>
  <p>الشركة: {{CompanyName}} — السجل التجاري: {{CRNumber}} — الرقم الضريبي: {{VATNumber}}</p>
  <hr/>
  <p>نفيد بأنه تمت الموافقة على طلب الإجازة المقدّم من الموظف <b>{{EmployeeName}}</b> (رقم {{EmployeeNumber}})،
     القسم {{Department}}، نوع الإجازة {{LeaveType}} من {{StartDate}} إلى {{EndDate}}.</p>
  <p>تاريخ الإصدار: {{GeneratedDate}}</p>
</div>";

    private const string SalaryCertificateHtml = @"
<div style='font-family:sans-serif;direction:rtl'>
  <h2 style='text-align:center'>شهادة تعريف بالراتب</h2>
  <p>الشركة: {{CompanyName}} — السجل التجاري: {{CRNumber}} — الرقم الضريبي: {{VATNumber}}</p>
  <hr/>
  <p>تشهد {{CompanyName}} بأن الموظف <b>{{EmployeeName}}</b> (رقم {{EmployeeNumber}})،
     يعمل لدينا بمسمى {{JobTitle}} في قسم {{Department}}.</p>
  <p>تاريخ الإصدار: {{GeneratedDate}}</p>
</div>";
}
