using HR.Domain.Common;

namespace HR.Domain.Engines.Reports;

public class ReportRelationship : BaseEntity
{
    public Guid ReportDefinitionId { get; set; }
    public Guid SourceObjectId { get; set; }
    public Guid TargetObjectId { get; set; }
    public string JoinField { get; set; } = null!;
    public string JoinType { get; set; } = "Inner"; // Inner, Left, Right
    public int SortOrder { get; set; }

    public ReportDefinition ReportDefinition { get; set; } = null!;
}
