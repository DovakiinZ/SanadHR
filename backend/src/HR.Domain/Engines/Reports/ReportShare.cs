using HR.Domain.Common;

namespace HR.Domain.Engines.Reports;

public class ReportShare : BaseEntity
{
    public Guid ReportDefinitionId { get; set; }
    public Guid? SharedWithUserId { get; set; }
    public Guid? SharedWithDepartmentId { get; set; }
    public Guid? SharedWithRoleId { get; set; }
    public bool CanEdit { get; set; }
    public DateTime SharedAt { get; set; }

    public ReportDefinition ReportDefinition { get; set; } = null!;
}
