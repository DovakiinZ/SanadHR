using HR.Api.Controllers;
using HR.Api.Filters;
using HR.Application.Common.Exceptions;
using HR.Application.Common.Models;
using HR.Infrastructure.Persistence;
using HR.Modules.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Core.Controllers;

[Authorize]
[Route("api/departments")]
public class DepartmentsController : BaseApiController
{
    private readonly ApplicationDbContext _context;

    public DepartmentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [RequirePermission("Departments.View")]
    public async Task<ActionResult<ApiResponse<List<DepartmentDto>>>> GetAll(CancellationToken ct)
    {
        var rows = await (
            from d in _context.Departments
            join px in _context.Departments on d.ParentDepartmentId equals (Guid?)px.Id into pj
            from p in pj.DefaultIfEmpty()
            join mx in _context.Employees on d.ManagerId equals (Guid?)mx.Id into mj
            from m in mj.DefaultIfEmpty()
            join dmx in _context.Employees on d.DeputyManagerId equals (Guid?)dmx.Id into dmj
            from dm in dmj.DefaultIfEmpty()
            join bx in _context.Branch on d.BranchId equals (Guid?)bx.Id into bj
            from b in bj.DefaultIfEmpty()
            join ccx in _context.MasterDataItems on d.CostCenterId equals (Guid?)ccx.Id into ccj
            from cc in ccj.DefaultIfEmpty()
            select new
            {
                D = d,
                ParentName = p != null ? (p.NameAr ?? p.Name) : null,
                MgrF = m != null ? (m.FirstNameAr ?? m.FirstName) : null,
                MgrL = m != null ? (m.LastNameAr ?? m.LastName) : null,
                DepF = dm != null ? (dm.FirstNameAr ?? dm.FirstName) : null,
                DepL = dm != null ? (dm.LastNameAr ?? dm.LastName) : null,
                BranchName = b != null ? (b.NameAr ?? b.Name) : null,
                CostCenterName = cc != null ? (cc.NameAr ?? cc.NameEn) : null
            }).ToListAsync(ct);

        var result = rows.Select(r => Map(r.D, r.ParentName, JoinName(r.MgrF, r.MgrL), JoinName(r.DepF, r.DepL), r.BranchName, r.CostCenterName)).ToList();
        return OkResponse(result);
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("Departments.View")]
    public async Task<ActionResult<ApiResponse<DepartmentDto>>> GetById(Guid id, CancellationToken ct)
    {
        var department = await _context.Departments.FirstOrDefaultAsync(d => d.Id == id, ct);
        if (department == null) throw new NotFoundException("Department", id);
        return OkResponse(Map(department, null, null, null, null, null));
    }

    [HttpPost]
    [RequirePermission("Departments.Create")]
    public async Task<ActionResult<ApiResponse<DepartmentDto>>> Create([FromBody] CreateDepartmentRequest request, CancellationToken ct)
    {
        var department = new Department
        {
            Name = request.Name,
            NameAr = request.NameAr,
            Code = request.Code,
            Description = request.Description,
            ParentDepartmentId = request.ParentDepartmentId,
            ManagerId = request.ManagerId,
            DeputyManagerId = request.DeputyManagerId,
            BranchId = request.BranchId,
            CostCenterId = request.CostCenterId,
            IsActive = request.IsActive
        };

        _context.Departments.Add(department);
        await _context.SaveChangesAsync(ct);

        return CreatedResponse(Map(department, null, null, null, null, null));
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("Departments.Edit")]
    public async Task<ActionResult<ApiResponse<DepartmentDto>>> Update(Guid id, [FromBody] CreateDepartmentRequest request, CancellationToken ct)
    {
        var department = await _context.Departments.FindAsync(new object[] { id }, ct);
        if (department == null) throw new NotFoundException("Department", id);

        if (request.ParentDepartmentId == id)
            throw new ConflictException("لا يمكن أن يكون القسم تابعاً لنفسه");

        department.Name = request.Name;
        department.NameAr = request.NameAr;
        department.Code = request.Code;
        department.Description = request.Description;
        department.ParentDepartmentId = request.ParentDepartmentId;
        department.ManagerId = request.ManagerId;
        department.DeputyManagerId = request.DeputyManagerId;
        department.BranchId = request.BranchId;
        department.CostCenterId = request.CostCenterId;
        department.IsActive = request.IsActive;

        await _context.SaveChangesAsync(ct);

        return OkResponse(Map(department, null, null, null, null, null));
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("Departments.Delete")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id, CancellationToken ct)
    {
        var department = await _context.Departments.FindAsync(new object[] { id }, ct);
        if (department == null) throw new NotFoundException("Department", id);

        var hasChildren = await _context.Departments.AnyAsync(d => d.ParentDepartmentId == id, ct);
        if (hasChildren) throw new ConflictException("لا يمكن حذف قسم يحتوي على أقسام فرعية");

        department.IsDeleted = true;
        department.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        return OkResponse("Department deleted");
    }

    private static string? JoinName(string? first, string? last)
    {
        var name = $"{first} {last}".Trim();
        return string.IsNullOrWhiteSpace(name) ? null : name;
    }

    private static DepartmentDto Map(Department d, string? parentName, string? managerName, string? deputyName, string? branchName, string? costCenterName) => new()
    {
        Id = d.Id,
        Name = d.Name,
        NameAr = d.NameAr,
        Code = d.Code,
        Description = d.Description,
        ParentDepartmentId = d.ParentDepartmentId,
        ParentDepartmentName = parentName,
        ManagerId = d.ManagerId,
        ManagerName = managerName,
        DeputyManagerId = d.DeputyManagerId,
        DeputyManagerName = deputyName,
        BranchId = d.BranchId,
        BranchName = branchName,
        CostCenterId = d.CostCenterId,
        CostCenterName = costCenterName,
        IsActive = d.IsActive
    };
}

public class DepartmentDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? NameAr { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
    public Guid? ParentDepartmentId { get; set; }
    public string? ParentDepartmentName { get; set; }
    public Guid? ManagerId { get; set; }
    public string? ManagerName { get; set; }
    public Guid? DeputyManagerId { get; set; }
    public string? DeputyManagerName { get; set; }
    public Guid? BranchId { get; set; }
    public string? BranchName { get; set; }
    public Guid? CostCenterId { get; set; }
    public string? CostCenterName { get; set; }
    public bool IsActive { get; set; }
}

public class CreateDepartmentRequest
{
    public string Name { get; set; } = null!;
    public string? NameAr { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
    public Guid? ParentDepartmentId { get; set; }
    public Guid? ManagerId { get; set; }
    public Guid? DeputyManagerId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? CostCenterId { get; set; }
    public bool IsActive { get; set; } = true;
}
