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
        var departments = await _context.Departments
            .Include(d => d.SubDepartments)
            .Select(d => new DepartmentDto
            {
                Id = d.Id,
                Name = d.Name,
                NameAr = d.NameAr,
                Description = d.Description,
                ParentDepartmentId = d.ParentDepartmentId,
                ManagerId = d.ManagerId
            })
            .ToListAsync(ct);

        return OkResponse(departments);
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("Departments.View")]
    public async Task<ActionResult<ApiResponse<DepartmentDto>>> GetById(Guid id, CancellationToken ct)
    {
        var department = await _context.Departments
            .Where(d => d.Id == id)
            .Select(d => new DepartmentDto
            {
                Id = d.Id,
                Name = d.Name,
                NameAr = d.NameAr,
                Description = d.Description,
                ParentDepartmentId = d.ParentDepartmentId,
                ManagerId = d.ManagerId
            })
            .FirstOrDefaultAsync(ct);

        if (department == null) throw new NotFoundException("Department", id);
        return OkResponse(department);
    }

    [HttpPost]
    [RequirePermission("Departments.Create")]
    public async Task<ActionResult<ApiResponse<DepartmentDto>>> Create([FromBody] CreateDepartmentRequest request, CancellationToken ct)
    {
        var department = new Department
        {
            Name = request.Name,
            NameAr = request.NameAr,
            Description = request.Description,
            ParentDepartmentId = request.ParentDepartmentId,
            ManagerId = request.ManagerId
        };

        _context.Departments.Add(department);
        await _context.SaveChangesAsync(ct);

        return CreatedResponse(new DepartmentDto
        {
            Id = department.Id, Name = department.Name, NameAr = department.NameAr,
            Description = department.Description, ParentDepartmentId = department.ParentDepartmentId,
            ManagerId = department.ManagerId
        });
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("Departments.Edit")]
    public async Task<ActionResult<ApiResponse<DepartmentDto>>> Update(Guid id, [FromBody] CreateDepartmentRequest request, CancellationToken ct)
    {
        var department = await _context.Departments.FindAsync(new object[] { id }, ct);
        if (department == null) throw new NotFoundException("Department", id);

        department.Name = request.Name;
        department.NameAr = request.NameAr;
        department.Description = request.Description;
        department.ParentDepartmentId = request.ParentDepartmentId;
        department.ManagerId = request.ManagerId;

        await _context.SaveChangesAsync(ct);

        return OkResponse(new DepartmentDto
        {
            Id = department.Id, Name = department.Name, NameAr = department.NameAr,
            Description = department.Description, ParentDepartmentId = department.ParentDepartmentId,
            ManagerId = department.ManagerId
        });
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("Departments.Delete")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id, CancellationToken ct)
    {
        var department = await _context.Departments.FindAsync(new object[] { id }, ct);
        if (department == null) throw new NotFoundException("Department", id);

        department.IsDeleted = true;
        department.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        return OkResponse("Department deleted");
    }
}

public class DepartmentDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? NameAr { get; set; }
    public string? Description { get; set; }
    public Guid? ParentDepartmentId { get; set; }
    public Guid? ManagerId { get; set; }
}

public class CreateDepartmentRequest
{
    public string Name { get; set; } = null!;
    public string? NameAr { get; set; }
    public string? Description { get; set; }
    public Guid? ParentDepartmentId { get; set; }
    public Guid? ManagerId { get; set; }
}
