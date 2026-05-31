using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Engines.Dashboards;

public class DashboardDefinition : TenantEntity
{
    public string Code { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string? Description { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? TemplateId { get; set; }
    public DashboardScope Scope { get; set; } = DashboardScope.Personal;
    public Guid? OwnerId { get; set; }
    public Guid? DepartmentId { get; set; }
    public bool IsDefault { get; set; }
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; } = true;
    public string? LayoutConfiguration { get; set; } // JSONB - grid settings
    public int SortOrder { get; set; }

    public DashboardCategory? Category { get; set; }
    public DashboardTemplate? Template { get; set; }
    public ICollection<DashboardWidget> Widgets { get; set; } = new List<DashboardWidget>();
    public ICollection<DashboardShare> Shares { get; set; } = new List<DashboardShare>();
}
