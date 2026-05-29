using HR.Api.Controllers;
using HR.Api.Filters;
using HR.Application.Common.Exceptions;
using HR.Application.Common.Models;
using HR.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Identity.Controllers;

[Authorize]
[Route("api/users")]
public class UsersController : BaseApiController
{
    private readonly ApplicationDbContext _context;

    public UsersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [RequirePermission("Identity.ViewUsers")]
    public async Task<ActionResult<ApiResponse<List<UserInfo>>>> GetAll(CancellationToken ct)
    {
        var users = await _context.Users
            .Select(u => new UserInfo { Id = u.Id, Email = u.Email, FullName = u.FullName })
            .ToListAsync(ct);

        return OkResponse(users);
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("Identity.ViewUsers")]
    public async Task<ActionResult<ApiResponse<UserInfo>>> GetById(Guid id, CancellationToken ct)
    {
        var user = await _context.Users
            .Where(u => u.Id == id)
            .Select(u => new UserInfo { Id = u.Id, Email = u.Email, FullName = u.FullName })
            .FirstOrDefaultAsync(ct);

        if (user == null) throw new NotFoundException("User", id);

        return OkResponse(user);
    }
}
