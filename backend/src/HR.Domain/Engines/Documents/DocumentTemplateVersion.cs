using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Engines.Documents;

public class DocumentTemplateVersion : BaseEntity
{
    public Guid DocumentTemplateId { get; set; }
    public int VersionNumber { get; set; }
    public string BodyTemplate { get; set; } = null!;
    public string? HeaderTemplate { get; set; }
    public string? FooterTemplate { get; set; }
    public string? ChangeNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }

    public DocumentTemplate DocumentTemplate { get; set; } = null!;
}
