using HR.Domain.Common;

namespace HR.Domain.Engines.OrgGraph;

public class OrgGraphLayout : TenantEntity
{
    public string Code { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string GraphType { get; set; } = null!; // Department, Employee, Full
    public string LayoutData { get; set; } = null!; // JSONB - full layout positions
    public bool IsDefault { get; set; }
    public Guid? OwnerId { get; set; }
}
