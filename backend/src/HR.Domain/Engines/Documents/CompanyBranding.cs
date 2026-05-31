using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Engines.Documents;

public class CompanyBranding : TenantEntity
{
    public BrandingElementType ElementType { get; set; }
    public string? ImageUrl { get; set; }
    public string? Content { get; set; } // HTML/text content for headers/footers
    public string? Configuration { get; set; } // JSONB - position, size, etc.
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}
