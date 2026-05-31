using HR.Domain.Common;

namespace HR.Domain.Engines.OrgGraph;

public class EmployeeReportingLine : TenantEntity
{
    public Guid EmployeeId { get; set; }
    public Guid ManagerId { get; set; }
    public string ReportingType { get; set; } = null!; // Direct, Indirect, Functional
    public bool IsPrimary { get; set; } = true;
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;
}
