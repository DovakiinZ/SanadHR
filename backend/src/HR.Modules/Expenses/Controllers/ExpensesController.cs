using HR.Application.Common.Interfaces;
using HR.Application.Common.Models;
using HR.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Expenses.Controllers;

/// <summary>Expense records created when Expense Claim requests are approved.</summary>
[Authorize]
[ApiController]
[Route("api/expenses")]
public class ExpensesController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _user;
    public ExpensesController(ApplicationDbContext db, ICurrentUserService user) { _db = db; _user = user; }

    public sealed class ExpenseDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? Category { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "SAR";
        public string? Description { get; set; }
        public string? ReceiptUrl { get; set; }
        public string Status { get; set; } = null!;
        public DateTime DecidedAt { get; set; }
    }

    /// <summary>List expenses. scope=all (with permission) → everyone; otherwise the caller's own.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ExpenseDto>>>> Get([FromQuery] string? scope, [FromQuery] Guid? employeeId, CancellationToken ct)
    {
        var canSeeAll = _user.Permissions.Contains("Expenses.View") || _user.Permissions.Contains("Payroll.View") || _user.Permissions.Contains("Employees.View");
        var q = _db.Expenses.AsNoTracking().AsQueryable();
        if (employeeId is { } eid && canSeeAll) q = q.Where(e => e.EmployeeId == eid);
        else if (scope == "all" && canSeeAll) { /* all */ }
        else
        {
            var myId = await _db.Employees.Where(e => e.UserId == _user.UserId).Select(e => (Guid?)e.Id).FirstOrDefaultAsync(ct);
            q = q.Where(e => e.EmployeeId == myId);
        }

        var rows = await (from x in q
                          join emp in _db.Employees on x.EmployeeId equals emp.Id into ej
                          from emp in ej.DefaultIfEmpty()
                          join cat in _db.MasterDataItems on x.ExpenseCategoryId equals cat.Id into cj
                          from cat in cj.DefaultIfEmpty()
                          orderby x.DecidedAt descending
                          select new ExpenseDto
                          {
                              Id = x.Id, EmployeeId = x.EmployeeId,
                              EmployeeName = emp != null ? ((emp.FirstNameAr ?? emp.FirstName) + " " + (emp.LastNameAr ?? emp.LastName)) : null,
                              Category = cat != null ? cat.NameAr : null,
                              Amount = x.Amount, Currency = x.Currency, Description = x.Description,
                              ReceiptUrl = x.ReceiptUrl, Status = x.Status, DecidedAt = x.DecidedAt,
                          }).ToListAsync(ct);
        return Ok(ApiResponse<List<ExpenseDto>>.Ok(rows));
    }
}
