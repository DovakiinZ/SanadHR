using HR.Api.Controllers;
using HR.Application.Common.Interfaces;
using HR.Application.Common.Models;
using HR.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Controllers;

/// <summary>In-app (bell) notifications for the current user.</summary>
[Authorize]
[Route("api/notifications")]
public class NotificationsController : BaseApiController
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _user;

    public NotificationsController(ApplicationDbContext db, ICurrentUserService user) { _db = db; _user = user; }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<NotificationDto>>>> Get([FromQuery] bool unreadOnly, CancellationToken ct)
    {
        var q = _db.Notifications.Where(n => n.UserId == _user.UserId);
        if (unreadOnly) q = q.Where(n => !n.IsRead);
        var list = await q.OrderByDescending(n => n.CreatedAt).Take(50)
            .Select(n => new NotificationDto
            {
                Id = n.Id, TitleAr = n.TitleAr, TitleEn = n.TitleEn, BodyAr = n.BodyAr, BodyEn = n.BodyEn,
                Category = n.Category, Link = n.Link, EntityId = n.EntityId, IsRead = n.IsRead, CreatedAt = n.CreatedAt,
            }).ToListAsync(ct);
        return OkResponse(list);
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<ApiResponse<int>>> UnreadCount(CancellationToken ct)
        => OkResponse(await _db.Notifications.CountAsync(n => n.UserId == _user.UserId && !n.IsRead, ct));

    [HttpPost("{id:guid}/read")]
    public async Task<ActionResult<ApiResponse>> Read(Guid id, CancellationToken ct)
    {
        var n = await _db.Notifications.FirstOrDefaultAsync(x => x.Id == id && x.UserId == _user.UserId, ct);
        if (n is not null && !n.IsRead) { n.IsRead = true; await _db.SaveChangesAsync(ct); }
        return OkResponse("Read");
    }

    [HttpPost("read-all")]
    public async Task<ActionResult<ApiResponse>> ReadAll(CancellationToken ct)
    {
        var unread = await _db.Notifications.Where(n => n.UserId == _user.UserId && !n.IsRead).ToListAsync(ct);
        foreach (var n in unread) n.IsRead = true;
        await _db.SaveChangesAsync(ct);
        return OkResponse("All read");
    }
}

public sealed class NotificationDto
{
    public Guid Id { get; set; }
    public string TitleAr { get; set; } = "";
    public string TitleEn { get; set; } = "";
    public string? BodyAr { get; set; }
    public string? BodyEn { get; set; }
    public string? Category { get; set; }
    public string? Link { get; set; }
    public Guid? EntityId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
