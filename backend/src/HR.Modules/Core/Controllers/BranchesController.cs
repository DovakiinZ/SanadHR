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
[Route("api/branches")]
public class BranchesController : BaseApiController
{
    private readonly ApplicationDbContext _context;

    public BranchesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [RequirePermission("Branches.View")]
    public async Task<ActionResult<ApiResponse<List<BranchDto>>>> GetAll(CancellationToken ct)
    {
        var branches = await _context.Branch
            .Select(b => Project(b))
            .ToListAsync(ct);

        return OkResponse(branches);
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("Branches.View")]
    public async Task<ActionResult<ApiResponse<BranchDto>>> GetById(Guid id, CancellationToken ct)
    {
        var branch = await _context.Branch
            .Where(b => b.Id == id)
            .Select(b => Project(b))
            .FirstOrDefaultAsync(ct);

        if (branch == null) throw new NotFoundException("Branch", id);
        return OkResponse(branch);
    }

    [HttpPost]
    [RequirePermission("Branches.Create")]
    public async Task<ActionResult<ApiResponse<BranchDto>>> Create([FromBody] CreateBranchRequest request, CancellationToken ct)
    {
        var branch = new Branch
        {
            Name = request.Name, NameAr = request.NameAr, Code = request.Code,
            City = request.City, Address = request.Address, Phone = request.Phone,
            IsMainBranch = request.IsMainBranch,
            Latitude = request.Latitude, Longitude = request.Longitude,
            GeofenceRadiusMeters = request.GeofenceRadiusMeters,
            IsActive = request.IsActive,
        };

        _context.Branch.Add(branch);
        await _context.SaveChangesAsync(ct);

        return CreatedResponse(Map(branch));
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("Branches.Edit")]
    public async Task<ActionResult<ApiResponse<BranchDto>>> Update(Guid id, [FromBody] CreateBranchRequest request, CancellationToken ct)
    {
        var branch = await _context.Branch.FindAsync(new object[] { id }, ct);
        if (branch == null) throw new NotFoundException("Branch", id);

        branch.Name = request.Name;
        branch.NameAr = request.NameAr;
        branch.Code = request.Code;
        branch.City = request.City;
        branch.Address = request.Address;
        branch.Phone = request.Phone;
        branch.IsMainBranch = request.IsMainBranch;
        branch.Latitude = request.Latitude;
        branch.Longitude = request.Longitude;
        branch.GeofenceRadiusMeters = request.GeofenceRadiusMeters;
        branch.IsActive = request.IsActive;

        await _context.SaveChangesAsync(ct);

        return OkResponse(Map(branch));
    }

    // EF-translatable projection (used inside IQueryable.Select).
    private static BranchDto Project(Branch b) => new()
    {
        Id = b.Id, Name = b.Name, NameAr = b.NameAr, Code = b.Code,
        City = b.City, Address = b.Address, Phone = b.Phone, IsMainBranch = b.IsMainBranch,
        Latitude = b.Latitude, Longitude = b.Longitude, GeofenceRadiusMeters = b.GeofenceRadiusMeters,
        IsActive = b.IsActive,
    };

    private static BranchDto Map(Branch b) => Project(b);

    [HttpDelete("{id:guid}")]
    [RequirePermission("Branches.Delete")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id, CancellationToken ct)
    {
        var branch = await _context.Branch.FindAsync(new object[] { id }, ct);
        if (branch == null) throw new NotFoundException("Branch", id);

        branch.IsDeleted = true;
        branch.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        return OkResponse("Branch deleted");
    }
}

public class BranchDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? NameAr { get; set; }
    public string? Code { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public bool IsMainBranch { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int? GeofenceRadiusMeters { get; set; }
    public bool IsActive { get; set; }
}

public class CreateBranchRequest
{
    public string Name { get; set; } = null!;
    public string? NameAr { get; set; }
    public string? Code { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public bool IsMainBranch { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int? GeofenceRadiusMeters { get; set; }
    public bool IsActive { get; set; } = true;
}
