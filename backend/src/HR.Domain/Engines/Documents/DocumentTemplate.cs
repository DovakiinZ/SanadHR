using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Engines.Documents;

public class DocumentTemplate : TenantEntity
{
    public string Code { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string? Description { get; set; }
    public string Module { get; set; } = null!;
    public DocumentTemplateStatus Status { get; set; } = DocumentTemplateStatus.Draft;
    public DocumentOutputFormat OutputFormat { get; set; } = DocumentOutputFormat.Pdf;
    public string BodyTemplate { get; set; } = null!; // HTML template with tokens
    public string? HeaderTemplate { get; set; }
    public string? FooterTemplate { get; set; }
    public string? StyleSheet { get; set; } // CSS
    public bool UseBranding { get; set; } = true;
    public string? PageSettings { get; set; } // JSONB - margins, orientation, size
    public int Version { get; set; } = 1;
    public bool IsActive { get; set; } = true;

    public ICollection<DocumentTemplateToken> Tokens { get; set; } = new List<DocumentTemplateToken>();
    public ICollection<DocumentTemplateVersion> Versions { get; set; } = new List<DocumentTemplateVersion>();
    public ICollection<GeneratedDocument> GeneratedDocuments { get; set; } = new List<GeneratedDocument>();
}
