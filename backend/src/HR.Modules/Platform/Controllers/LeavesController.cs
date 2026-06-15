using HR.Api.Controllers;
using HR.Application.Common.Interfaces;
using HR.Application.Common.Models;
using HR.Infrastructure.Persistence;
using HR.Modules.Platform.DTOs.Leaves;
using HR.Modules.Platform.Services.Leaves;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Controllers;

/// <summary>HR management of approved/assigned leave records (distinct from employee leave requests):
/// list, detail, direct assignment, edit, cancel, and printing the official Leave Record document.</summary>
[Authorize]
[Route("api/leaves")]
public class LeavesController : BaseApiController
{
    private readonly ILeaveRecordService _svc;
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _user;

    public LeavesController(ILeaveRecordService svc, ApplicationDbContext db, ICurrentUserService user)
    {
        _svc = svc; _db = db; _user = user;
    }

    private bool Has(params string[] perms) => perms.Any(p => _user.Permissions.Contains(p));
    private bool CanViewAll => Has("Leaves.View", "Employees.View");
    private bool CanAssign => Has("Leaves.Assign", "Leaves.Create", "Employees.Edit", "Employees.Create");
    private bool CanEdit => Has("Leaves.Edit", "Employees.Edit");
    private bool CanCancel => Has("Leaves.Cancel", "Leaves.Edit", "Employees.Edit");

    private Task<Guid?> MyEmployeeIdAsync(CancellationToken ct) =>
        _db.Employees.Where(e => e.UserId == _user.UserId).Select(e => (Guid?)e.Id).FirstOrDefaultAsync(ct);

    private ActionResult Denied() => StatusCode(403, ApiResponse.Fail("ليس لديك صلاحية لتنفيذ هذا الإجراء"));

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<LeaveRecordListDto>>>> List([FromQuery] LeaveFilter filter, CancellationToken ct)
    {
        if (!CanViewAll)
        {
            filter.Mine = true;
            filter.MyEmployeeId = await MyEmployeeIdAsync(ct);
        }
        var rows = await _svc.ListAsync(filter, ct);
        return OkResponse(rows);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<LeaveDetailDto>>> GetById(Guid id, CancellationToken ct)
    {
        var detail = await _svc.GetDetailAsync(id, ct);
        if (detail is null) return NotFound(ApiResponse<LeaveDetailDto>.Fail("Leave record not found"));
        if (!CanViewAll && detail.Record.EmployeeId != await MyEmployeeIdAsync(ct)) return StatusCode(403, ApiResponse<LeaveDetailDto>.Fail("ليس لديك صلاحية"));
        return OkResponse(detail);
    }

    [HttpPost("assign")]
    public async Task<ActionResult<ApiResponse<int>>> Assign([FromBody] AssignLeaveRequest req, CancellationToken ct)
    {
        if (!CanAssign) return Denied();
        try
        {
            var n = await _svc.AssignAsync(req, ct);
            return CreatedResponse(n, $"تم تعيين الإجازة لـ {n} موظف");
        }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse<int>.Fail(ex.Message)); }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse>> Edit(Guid id, [FromBody] EditLeaveRequest req, CancellationToken ct)
    {
        if (!CanEdit) return Denied();
        try { await _svc.EditAsync(id, req, ct); return OkResponse("تم تعديل الإجازة"); }
        catch (KeyNotFoundException) { return NotFound(ApiResponse.Fail("Leave record not found")); }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse.Fail(ex.Message)); }
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<ApiResponse>> Cancel(Guid id, [FromBody] CancelLeaveRequest req, CancellationToken ct)
    {
        if (!CanCancel) return Denied();
        try { await _svc.CancelAsync(id, req?.Reason, ct); return OkResponse("تم إلغاء الإجازة"); }
        catch (KeyNotFoundException) { return NotFound(ApiResponse.Fail("Leave record not found")); }
        catch (InvalidOperationException ex) { return BadRequest(ApiResponse.Fail(ex.Message)); }
    }

    [HttpPost("{id:guid}/print")]
    public async Task<IActionResult> Print(Guid id, CancellationToken ct)
    {
        if (!CanViewAll)
        {
            var detail = await _svc.GetDetailAsync(id, ct);
            if (detail is null) return NotFound();
            if (detail.Record.EmployeeId != await MyEmployeeIdAsync(ct)) return StatusCode(403);
        }
        try
        {
            var (pdf, fileName) = await _svc.PrintAsync(id, ct);
            return File(pdf, "application/pdf", fileName);
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    /// <summary>Per-employee leave balances (entitled / used / remaining) by type.</summary>
    [HttpGet("/api/employees/{id:guid}/leave-balance")]
    public async Task<ActionResult<ApiResponse<List<LeaveBalanceDto>>>> EmployeeBalance(Guid id, CancellationToken ct)
    {
        if (!CanViewAll && id != await MyEmployeeIdAsync(ct)) return StatusCode(403, ApiResponse<List<LeaveBalanceDto>>.Fail("ليس لديك صلاحية"));
        var rows = await _svc.GetEmployeeBalanceAsync(id, ct);
        return OkResponse(rows);
    }
}
