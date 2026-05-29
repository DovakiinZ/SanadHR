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
            .Select(b => new BranchDto
            {
                Id = b.Id, Name = b.Name, NameAr = b.NameAr,
                City = b.City, Address = b.Address, Phone = b.Phone, IsMainBranch = b.IsMainBranch
            })
            .ToListAsync(ct);

        return OkResponse(branches);
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("Branches.View")]
    public async Task<ActionResult<ApiResponse<BranchDto>>> GetById(Guid id, CancellationToken ct)
    {
        var branch = await _context.Branch
            .Where(b => b.Id == id)
            .Select(b => new BranchDto
            {
                Id = b.Id, Name = b.Name, NameAr = b.NameAr,
                City = b.City, Address = b.Address, Phone = b.Phone, IsMainBranch = b.IsMainBranch
            })
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
            Name = request.Name, NameAr = request.NameAr,
            City = request.City, Address = request.Address,
            Phone = request.Phone, IsMainBranch = request.IsMainBranch
        };

        _context.Branch.Add(branch);
        await _context.SaveChangesAsync(ct);

        return CreatedResponse(new BranchDto
        {
            Id = branch.Id, Name = branch.Name, NameAr = branch.NameAr,
            City = branch.City, Address = branch.Address, Phone = branch.Phone, IsMainBranch = branch.IsMainBranch
        });
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("Branches.Edit")]
    public async Task<ActionResult<ApiResponse<BranchDto>>> Update(Guid id, [FromBody] CreateBranchRequest request, CancellationToken ct)
    {
        var branch = await _context.Branch.FindAsync(new object[] { id }, ct);
        if (branch == null) throw new NotFoundException("Branch", id);

        branch.Name = request.Name;
        branch.NameAr = request.NameAr;
        branch.City = request.City;
        branch.Address = request.Address;
        branch.Phone = request.Phone;
        branch.IsMainBranch = request.IsMainBranch;

        await _context.SaveChangesAsync(ct);

        return OkResponse(new BranchDto
        {
            Id = branch.Id, Name = branch.Name, NameAr = branch.NameAr,
            City = branch.City, Address = branch.Address, Phone = branch.Phone, IsMainBranch = branch.IsMainBranch
        });
    }

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
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public bool IsMainBranch { get; set; }
}

public class CreateBranchRequest
{
    public string Name { get; set; } = null!;
    public string? NameAr { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public bool IsMainBranch { get; set; }
}
