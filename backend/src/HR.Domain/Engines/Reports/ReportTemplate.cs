using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Engines.Reports;

public class ReportTemplate : TenantEntity
{
    public string Code { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string? Description { get; set; }
    public ReportType ReportType { get; set; }
    public Guid PrimaryObjectId { get; set; }
    public string Configuration { get; set; } = null!; // JSONB - full template config
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}
