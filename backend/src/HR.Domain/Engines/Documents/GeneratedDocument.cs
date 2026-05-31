using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Engines.Documents;

public class GeneratedDocument : TenantEntity
{
    public Guid DocumentTemplateId { get; set; }
    public string EntityType { get; set; } = null!;
    public Guid EntityId { get; set; }
    public DocumentGenerationStatus Status { get; set; } = DocumentGenerationStatus.Pending;
    public DocumentOutputFormat OutputFormat { get; set; }
    public string? FileUrl { get; set; }
    public string? FileName { get; set; }
    public long? FileSizeBytes { get; set; }
    public string? TokenValues { get; set; } // JSONB - resolved token values snapshot
    public string? ErrorMessage { get; set; }
    public DateTime? GeneratedAt { get; set; }
    public Guid? GeneratedById { get; set; }
    public Guid? WorkflowInstanceId { get; set; }

    public DocumentTemplate DocumentTemplate { get; set; } = null!;
}
