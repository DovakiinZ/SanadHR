using HR.Domain.Common;

namespace HR.Modules.Reports.Entities;

// TODO: Implement report definition entity
public class ReportDefinition : TenantEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string ReportType { get; set; } = null!;
}
