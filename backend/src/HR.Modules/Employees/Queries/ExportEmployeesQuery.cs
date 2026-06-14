using ClosedXML.Excel;
using HR.Application.Common.Interfaces;
using HR.Infrastructure.Persistence;
using HR.Modules.Employees.DTOs;
using HR.Modules.Employees.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Employees.Queries;

public sealed class EmployeeExportResult
{
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = "employees.xlsx";
}

/// <summary>
/// Builds an .xlsx of employees. Field groups are chosen by the caller; the salary &amp; bank groups
/// are dropped server-side unless the caller has Payroll.View / Employees.Edit (permission-gated,
/// not just hidden in the UI). Filters mirror the employees list (department/job title/branch/status/ids).
/// </summary>
public record ExportEmployeesQuery : IRequest<EmployeeExportResult>
{
    public List<string> Groups { get; init; } = new();   // personal, employment, salary, bank, leave, attendance
    public Guid? DepartmentId { get; init; }
    public Guid? JobTitleId { get; init; }
    public Guid? BranchId { get; init; }
    public string? Status { get; init; }
    public List<Guid> Ids { get; init; } = new();
}

public class ExportEmployeesQueryHandler : IRequestHandler<ExportEmployeesQuery, EmployeeExportResult>
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _user;
    public ExportEmployeesQueryHandler(ApplicationDbContext db, ICurrentUserService user) { _db = db; _user = user; }

    public async Task<EmployeeExportResult> Handle(ExportEmployeesQuery request, CancellationToken ct)
    {
        var canSeeSalary = _user.Permissions.Contains("Payroll.View") || _user.Permissions.Contains("Payroll.Edit")
            || _user.Permissions.Contains("Employees.Edit") || _user.Permissions.Contains("Employees.Create");

        var groups = new HashSet<string>(request.Groups.Select(g => g.ToLowerInvariant()));
        if (groups.Count == 0) { groups.Add("personal"); groups.Add("employment"); }
        if (!canSeeSalary) { groups.Remove("salary"); groups.Remove("bank"); }   // permission gate

        // Filter the employee set.
        var q = _db.Employees.AsQueryable();
        if (request.Ids.Count > 0) q = q.Where(e => request.Ids.Contains(e.Id));
        if (request.DepartmentId is { } dep) q = q.Where(e => e.DepartmentId == dep);
        if (request.JobTitleId is { } jt) q = q.Where(e => e.JobTitleId == jt);
        if (request.BranchId is { } br) q = q.Where(e => e.BranchId == br);
        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<HR.Domain.Enums.EmployeeStatus>(request.Status, true, out var st))
            q = q.Where(e => e.Status == st);

        var employees = await EmployeeProjection.MapAsync(q.OrderBy(e => e.EmployeeNumber), _db, ct);

        // Optional aggregates.
        Dictionary<Guid, decimal> leaveRemaining = new();
        Dictionary<Guid, int> attendanceDays = new();
        var ids = employees.Select(e => e.Id).ToList();
        var year = DateTime.UtcNow.Year;
        if (groups.Contains("leave"))
            leaveRemaining = await _db.LeaveBalances.Where(b => ids.Contains(b.EmployeeId) && b.Year == year)
                .GroupBy(b => b.EmployeeId)
                .Select(g => new { g.Key, Rem = g.Sum(x => x.EntitledDays + x.CarriedForwardDays - x.UsedDays) })
                .ToDictionaryAsync(x => x.Key, x => x.Rem, ct);
        if (groups.Contains("attendance"))
            attendanceDays = await _db.AttendanceRecords.Where(a => ids.Contains(a.EmployeeId) && a.Date.Year == year)
                .GroupBy(a => a.EmployeeId).Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count, ct);

        // Build columns: (header, selector).
        var cols = new List<(string Header, Func<EmployeeDto, object?> Get)>();
        if (groups.Contains("personal"))
        {
            cols.Add(("الرقم الوظيفي", e => e.EmployeeNumber));
            cols.Add(("الاسم", e => e.FullNameAr ?? e.FullName));
            cols.Add(("الاسم (EN)", e => e.FullName));
            cols.Add(("الجنس", e => e.GenderAr));
            cols.Add(("تاريخ الميلاد", e => e.DateOfBirth.ToString("yyyy-MM-dd")));
            cols.Add(("الجنسية", e => e.NationalityAr ?? e.Nationality));
            cols.Add(("الهوية", e => e.NationalId));
            cols.Add(("الجوال", e => e.Phone));
            cols.Add(("البريد", e => e.Email));
            cols.Add(("المدينة", e => e.City));
        }
        if (groups.Contains("employment"))
        {
            cols.Add(("الحالة", e => e.StatusAr));
            cols.Add(("تاريخ التعيين", e => e.HireDate.ToString("yyyy-MM-dd")));
            cols.Add(("المسمى الوظيفي", e => e.JobTitleAr ?? e.JobTitle));
            cols.Add(("الإدارة", e => e.DepartmentName));
            cols.Add(("الفرع", e => e.BranchName));
            cols.Add(("المدير", e => e.ManagerName));
            cols.Add(("نوع العقد", e => e.ContractTypeAr ?? e.ContractType));
            cols.Add(("نوع التوظيف", e => e.EmploymentTypeAr ?? e.EmploymentType));
        }
        if (groups.Contains("salary"))
        {
            cols.Add(("الراتب الأساسي", e => e.BasicSalary));
            cols.Add(("إجمالي البدلات", e => e.TotalAllowances));
            cols.Add(("الإضافات", e => e.TotalAdditions));
            cols.Add(("الاستقطاعات", e => e.TotalDeductions));
            cols.Add(("GOSI", e => e.GosiAmount));
            cols.Add(("صافي الراتب", e => e.NetSalary));
            cols.Add(("العملة", e => e.Currency));
        }
        if (groups.Contains("bank"))
        {
            cols.Add(("طريقة الدفع", e => e.PaymentMethodAr ?? e.PaymentMethod));
            cols.Add(("البنك", e => e.BankAr ?? e.Bank));
            cols.Add(("الآيبان", e => e.Iban));
            cols.Add(("رقم الحساب", e => e.BankAccountNumber));
            cols.Add(("رقم بطاقة الراتب", e => e.SalaryCardNumber));
        }
        if (groups.Contains("leave"))
            cols.Add(("رصيد الإجازات المتبقي", e => leaveRemaining.TryGetValue(e.Id, out var r) ? r : 0m));
        if (groups.Contains("attendance"))
            cols.Add(("أيام الحضور المسجلة", e => attendanceDays.TryGetValue(e.Id, out var c) ? c : 0));

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Employees");
        ws.RightToLeft = true;
        for (int c = 0; c < cols.Count; c++)
        {
            var cell = ws.Cell(1, c + 1);
            cell.Value = cols[c].Header;
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
        }
        for (int r = 0; r < employees.Count; r++)
            for (int c = 0; c < cols.Count; c++)
            {
                var val = cols[c].Get(employees[r]);
                var cell = ws.Cell(r + 2, c + 1);
                switch (val)
                {
                    case null: break;
                    case decimal d: cell.Value = (double)d; break;
                    case int i: cell.Value = i; break;
                    default: cell.Value = val.ToString(); break;
                }
            }
        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return new EmployeeExportResult
        {
            Content = ms.ToArray(),
            FileName = $"employees-{DateTime.UtcNow:yyyyMMdd}.xlsx",
        };
    }
}
