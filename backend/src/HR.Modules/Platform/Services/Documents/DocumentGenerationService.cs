using HR.Application.Common.Interfaces;
using HR.Domain.Engines.Documents;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Services.Documents;

/// <summary>
/// Mapping-driven document generation. For a request lifecycle trigger it finds every active
/// <see cref="HR.Domain.Engines.Requests.RequestTemplateMapping"/> for that request type + trigger
/// and records a <see cref="GeneratedDocument"/> per template. The PDF itself is rendered on demand
/// (by template id) when viewed/downloaded, so the record captures which template fired and when.
/// Adds entities to the shared DbContext; the caller (RequestEngine) persists.
/// </summary>
public interface IDocumentGenerationService
{
    Task<int> GenerateForTriggerAsync(Guid requestInstanceId, DocumentTriggerEvent trigger, CancellationToken ct);
}

public sealed class DocumentGenerationService : IDocumentGenerationService
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _user;
    public DocumentGenerationService(ApplicationDbContext db, ICurrentUserService user) { _db = db; _user = user; }

    public async Task<int> GenerateForTriggerAsync(Guid requestInstanceId, DocumentTriggerEvent trigger, CancellationToken ct)
    {
        var instance = await _db.RequestInstances.Include(r => r.RequestType)
            .FirstOrDefaultAsync(r => r.Id == requestInstanceId, ct);
        if (instance is null) return 0;

        var templateIds = await _db.RequestTemplateMappings
            .Where(m => m.RequestTypeId == instance.RequestTypeId && m.TriggerEvent == trigger && m.IsActive)
            .OrderBy(m => m.SortOrder)
            .Select(m => m.DocumentTemplateId).ToListAsync(ct);

        // Back-compat: a FinalApproval with no mapping but a legacy single print template.
        if (templateIds.Count == 0 && trigger == DocumentTriggerEvent.FinalApproval && instance.RequestType.PrintTemplateId is { } legacy)
            templateIds.Add(legacy);

        if (templateIds.Count == 0) return 0;

        int n = 0;
        foreach (var tid in templateIds.Distinct())
        {
            if (!await _db.DocumentTemplates.AnyAsync(t => t.Id == tid, ct)) continue;
            var doc = new GeneratedDocument
            {
                DocumentTemplateId = tid,
                EntityType = "RequestInstance",
                EntityId = instance.Id,
                Status = DocumentGenerationStatus.Completed,
                OutputFormat = DocumentOutputFormat.Pdf,
                FileName = $"{instance.RequestType.Code}-{instance.RequestNumber}.pdf",
                GeneratedAt = DateTime.UtcNow,
                GeneratedById = _user.UserId,
            };
            _db.Set<GeneratedDocument>().Add(doc);
            instance.GeneratedDocumentId = doc.Id; // latest fired becomes the primary
            n++;
        }
        return n;
    }
}
