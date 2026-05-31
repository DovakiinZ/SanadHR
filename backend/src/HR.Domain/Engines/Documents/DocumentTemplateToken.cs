using HR.Domain.Common;

namespace HR.Domain.Engines.Documents;

public class DocumentTemplateToken : BaseEntity
{
    public Guid DocumentTemplateId { get; set; }
    public string TokenCode { get; set; } = null!; // e.g., employee.name
    public string? DefaultValue { get; set; }
    public bool IsRequired { get; set; }
    public int SortOrder { get; set; }

    public DocumentTemplate DocumentTemplate { get; set; } = null!;
}
