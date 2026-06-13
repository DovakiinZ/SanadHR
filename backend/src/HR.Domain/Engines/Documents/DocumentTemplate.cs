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

    /// <summary>The visual block document — ordered list of blocks (title/text/table/image/qr/
    /// signature/stamp/divider/spacer/token). JSONB. This is the source of truth for new templates;
    /// the renderer prefers it over the legacy <see cref="BodyTemplate"/> HTML.</summary>
    public string? LayoutJson { get; set; }

    public string? BodyTemplate { get; set; } // Legacy HTML template with tokens (fallback)
    public string? HeaderTemplate { get; set; }
    public string? FooterTemplate { get; set; }
    public string? StyleSheet { get; set; } // CSS
    public bool UseBranding { get; set; } = true;

    /// <summary>The page template (chrome) this document inherits. Null → built-in default chrome.</summary>
    public Guid? PageTemplateId { get; set; }

    public string? PageSettings { get; set; } // JSONB - margins, orientation, size
    public int Version { get; set; } = 1;

    /// <summary>Shipped library template — editable via duplicate, but cannot be deleted.</summary>
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<DocumentTemplateToken> Tokens { get; set; } = new List<DocumentTemplateToken>();
    public ICollection<DocumentTemplateVersion> Versions { get; set; } = new List<DocumentTemplateVersion>();
    public ICollection<GeneratedDocument> GeneratedDocuments { get; set; } = new List<GeneratedDocument>();
}
