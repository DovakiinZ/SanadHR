using HR.Api.Controllers;
using HR.Application.Common.Interfaces;
using HR.Application.Common.Models;
using HR.Domain.Engines.Files;
using HR.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Core.Controllers;

[Authorize]
[Route("api/files")]
public class FilesController : BaseApiController
{
    private const long MaxBytes = 5 * 1024 * 1024; // 5 MB
    private static readonly HashSet<string> ImageTypes =
        new(StringComparer.OrdinalIgnoreCase) { "image/jpeg", "image/png", "image/webp", "image/gif" };

    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _user;

    public FilesController(ApplicationDbContext context, ICurrentUserService user)
    {
        _context = context;
        _user = user;
    }

    /// <summary>Upload a file (multipart). Returns the capability URL to render/download it.</summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<FileUploadResult>>> Upload(
        IFormFile file, [FromQuery] string? category, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<FileUploadResult>.Fail("لم يتم إرفاق ملف"));
        if (file.Length > MaxBytes)
            return BadRequest(ApiResponse<FileUploadResult>.Fail("حجم الملف يتجاوز 5 ميجابايت"));
        if (string.Equals(category, "photo", StringComparison.OrdinalIgnoreCase) && !ImageTypes.Contains(file.ContentType))
            return BadRequest(ApiResponse<FileUploadResult>.Fail("يجب أن تكون الصورة بصيغة JPG أو PNG أو WEBP"));

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms, ct);

        var stored = new StoredFile
        {
            TenantId = _user.TenantId,
            FileName = Path.GetFileName(file.FileName),
            ContentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
            Data = ms.ToArray(),
            SizeBytes = file.Length,
            Category = category,
        };

        _context.Files.Add(stored);
        await _context.SaveChangesAsync(ct);

        return CreatedResponse(new FileUploadResult
        {
            Id = stored.Id,
            Url = $"/api/files/{stored.Id}",
            FileName = stored.FileName,
            ContentType = stored.ContentType,
            SizeBytes = stored.SizeBytes,
        });
    }

    /// <summary>Serve a file by its unguessable id. Anonymous so &lt;img src&gt; works.</summary>
    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var file = await _context.Files.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (file == null) return NotFound();
        Response.Headers["Cache-Control"] = "private, max-age=86400";
        return File(file.Data, file.ContentType);
    }
}

public class FileUploadResult
{
    public Guid Id { get; set; }
    public string Url { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long SizeBytes { get; set; }
}
