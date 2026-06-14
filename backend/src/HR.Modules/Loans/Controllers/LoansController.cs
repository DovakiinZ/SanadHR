using HR.Application.Common.Interfaces;
using HR.Application.Common.Models;
using HR.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Loans.Controllers;

/// <summary>Loans &amp; salary advances created when Loan/Advance requests are approved, with their
/// installment (payroll deduction) schedules.</summary>
[Authorize]
[ApiController]
[Route("api/loans")]
public class LoansController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _user;
    public LoansController(ApplicationDbContext db, ICurrentUserService user) { _db = db; _user = user; }

    public sealed class LoanDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? LoanType { get; set; }
        public string Kind { get; set; } = null!;
        public decimal Principal { get; set; }
        public int InstallmentMonths { get; set; }
        public decimal MonthlyInstallment { get; set; }
        public string Status { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public List<InstallmentDto> Installments { get; set; } = new();
    }
    public sealed class InstallmentDto { public DateTime DueMonth { get; set; } public decimal Amount { get; set; } public bool Paid { get; set; } }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<LoanDto>>>> Get([FromQuery] string? scope, [FromQuery] Guid? employeeId, CancellationToken ct)
    {
        var canSeeAll = _user.Permissions.Contains("Loans.View") || _user.Permissions.Contains("Payroll.View") || _user.Permissions.Contains("Employees.View");
        var q = _db.Loans.AsNoTracking().Include(l => l.Installments).AsQueryable();
        if (employeeId is { } eid && canSeeAll) q = q.Where(l => l.EmployeeId == eid);
        else if (scope == "all" && canSeeAll) { /* all */ }
        else
        {
            var myId = await _db.Employees.Where(e => e.UserId == _user.UserId).Select(e => (Guid?)e.Id).FirstOrDefaultAsync(ct);
            q = q.Where(l => l.EmployeeId == myId);
        }

        var loans = await q.OrderByDescending(l => l.StartDate).ToListAsync(ct);
        var empIds = loans.Select(l => l.EmployeeId).Distinct().ToList();
        var typeIds = loans.Where(l => l.LoanTypeId != null).Select(l => l.LoanTypeId!.Value).Distinct().ToList();
        var emps = await _db.Employees.Where(e => empIds.Contains(e.Id))
            .Select(e => new { e.Id, Name = (e.FirstNameAr ?? e.FirstName) + " " + (e.LastNameAr ?? e.LastName) }).ToDictionaryAsync(e => e.Id, e => e.Name, ct);
        var types = await _db.MasterDataItems.Where(m => typeIds.Contains(m.Id)).ToDictionaryAsync(m => m.Id, m => m.NameAr, ct);

        var rows = loans.Select(l => new LoanDto
        {
            Id = l.Id, EmployeeId = l.EmployeeId,
            EmployeeName = emps.TryGetValue(l.EmployeeId, out var n) ? n : null,
            LoanType = l.LoanTypeId is { } tid && types.TryGetValue(tid, out var tn) ? tn : null,
            Kind = l.Kind, Principal = l.Principal, InstallmentMonths = l.InstallmentMonths,
            MonthlyInstallment = l.MonthlyInstallment, Status = l.Status, StartDate = l.StartDate,
            Installments = l.Installments.OrderBy(i => i.DueMonth).Select(i => new InstallmentDto { DueMonth = i.DueMonth, Amount = i.Amount, Paid = i.Paid }).ToList(),
        }).ToList();
        return Ok(ApiResponse<List<LoanDto>>.Ok(rows));
    }
}
