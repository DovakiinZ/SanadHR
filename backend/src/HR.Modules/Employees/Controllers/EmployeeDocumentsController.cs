using HR.Application.Common.Interfaces;
using HR.Application.Common.Models;
using HR.Domain.Engines.Documents;
using HR.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Employees.Controllers;

/// <summary>
/// Personnel documents attached to an employee (ID, Iqama, passport, contract, certificate, medical
/// report, custom). Files are uploaded via <c>POST /api/files</c> first; the returned URL is stored
/// here together with type, expiry and notes. Read is allowed to staff with Employees.View (or the
/// employee for their own record); write requires Employees.Edit.
/// </summary>
[Authorize]
[ApiController]
[Route("api/employees/{employeeId:guid}/documents")]
public class EmployeeDocumentsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _user;
    public EmployeeDocumentsController(ApplicationDbContext db, ICurrentUserService user) { _db = db; _user = user; }

    public sealed class EmployeeDocumentDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string Type { get; set; } = "Custom";
        public string Title { get; set; } = null!;
        public string? DocumentNumber { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? Notes { get; set; }
        public string FileUrl { get; set; } = null!;
        public string? FileName { get; set; }
        public string? ContentType { get; set; }
        public long SizeBytes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public sealed class EmployeeDocumentInput
    {
        public string Type { get; set; } = "Custom";
        public string Title { get; set; } = null!;
        public string? DocumentNumber { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? Notes { get; set; }
        public string FileUrl { get; set; } = null!;
        public string? FileName { get; set; }
        public string? ContentType { get; set; }
        public long SizeBytes { get; set; }
    }

    private bool CanWrite => _user.Permissions.Contains("Employees.Edit") || _user.Permissions.Contains("Employees.Create");

    private async Task<bool> CanReadAsync(Guid employeeId, CancellationToken ct)
    {
        if (_user.Permissions.Contains("Employees.View") || _user.Permissions.Contains("Employees.Edit")) return true;
        // Otherwise only the employee themselves may read their own documents.
        var ownId = await _db.Employees.Where(e => e.UserId == _user.UserId).Select(e => (Guid?)e.Id).FirstOrDefaultAsync(ct);
        return ownId == employeeId;
    }

    private static EmployeeDocumentDto ToDto(EmployeeDocument d) => new()
    {
        Id = d.Id, EmployeeId = d.EmployeeId, Type = d.Type, Title = d.Title,
        DocumentNumber = d.DocumentNumber, IssueDate = d.IssueDate, ExpiryDate = d.ExpiryDate,
        Notes = d.Notes, FileUrl = d.FileUrl, FileName = d.FileName, ContentType = d.ContentType,
        SizeBytes = d.SizeBytes, CreatedAt = d.CreatedAt,
    };

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<EmployeeDocumentDto>>>> List(Guid employeeId, CancellationToken ct)
    {
        if (!await CanReadAsync(employeeId, ct)) return Forbid();
        var rows = await _db.EmployeeDocuments.AsNoTracking()
            .Where(d => d.EmployeeId == employeeId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(ct);
        return Ok(ApiResponse<List<EmployeeDocumentDto>>.Ok(rows.Select(ToDto).ToList()));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<EmployeeDocumentDto>>> Create(Guid employeeId, [FromBody] EmployeeDocumentInput input, CancellationToken ct)
    {
        if (!CanWrite) return Forbid();
        if (string.IsNullOrWhiteSpace(input.Title)) return BadRequest(ApiResponse<EmployeeDocumentDto>.Fail("اسم المستند مطلوب"));
        if (string.IsNullOrWhiteSpace(input.FileUrl)) return BadRequest(ApiResponse<EmployeeDocumentDto>.Fail("ملف المستند مطلوب"));
        var exists = await _db.Employees.AnyAsync(e => e.Id == employeeId, ct);
        if (!exists) return NotFound(ApiResponse<EmployeeDocumentDto>.Fail("الموظف غير موجود"));

        var doc = new EmployeeDocument
        {
            EmployeeId = employeeId,
            Type = string.IsNullOrWhiteSpace(input.Type) ? "Custom" : input.Type.Trim(),
            Title = input.Title.Trim(),
            DocumentNumber = input.DocumentNumber?.Trim(),
            IssueDate = input.IssueDate,
            ExpiryDate = input.ExpiryDate,
            Notes = input.Notes?.Trim(),
            FileUrl = input.FileUrl.Trim(),
            FileName = input.FileName,
            ContentType = input.ContentType,
            SizeBytes = input.SizeBytes,
        };
        _db.EmployeeDocuments.Add(doc);
        await _db.SaveChangesAsync(ct);
        return Ok(ApiResponse<EmployeeDocumentDto>.Ok(ToDto(doc)));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<EmployeeDocumentDto>>> Update(Guid employeeId, Guid id, [FromBody] EmployeeDocumentInput input, CancellationToken ct)
    {
        if (!CanWrite) return Forbid();
        var doc = await _db.EmployeeDocuments.FirstOrDefaultAsync(d => d.Id == id && d.EmployeeId == employeeId, ct);
        if (doc is null) return NotFound(ApiResponse<EmployeeDocumentDto>.Fail("المستند غير موجود"));
        if (string.IsNullOrWhiteSpace(input.Title)) return BadRequest(ApiResponse<EmployeeDocumentDto>.Fail("اسم المستند مطلوب"));

        doc.Type = string.IsNullOrWhiteSpace(input.Type) ? doc.Type : input.Type.Trim();
        doc.Title = input.Title.Trim();
        doc.DocumentNumber = input.DocumentNumber?.Trim();
        doc.IssueDate = input.IssueDate;
        doc.ExpiryDate = input.ExpiryDate;
        doc.Notes = input.Notes?.Trim();
        // Replace the file only when a new URL is supplied; otherwise keep the existing one.
        if (!string.IsNullOrWhiteSpace(input.FileUrl))
        {
            doc.FileUrl = input.FileUrl.Trim();
            doc.FileName = input.FileName;
            doc.ContentType = input.ContentType;
            doc.SizeBytes = input.SizeBytes;
        }
        await _db.SaveChangesAsync(ct);
        return Ok(ApiResponse<EmployeeDocumentDto>.Ok(ToDto(doc)));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid employeeId, Guid id, CancellationToken ct)
    {
        if (!CanWrite) return Forbid();
        var doc = await _db.EmployeeDocuments.FirstOrDefaultAsync(d => d.Id == id && d.EmployeeId == employeeId, ct);
        if (doc is null) return NotFound(ApiResponse.Fail("المستند غير موجود"));
        // Soft delete (global query filter hides IsDeleted rows).
        doc.IsDeleted = true;
        doc.DeletedAt = DateTime.UtcNow;
        doc.DeletedBy = _user.Email;
        await _db.SaveChangesAsync(ct);
        return Ok(ApiResponse.Ok("تم حذف المستند"));
    }
}
