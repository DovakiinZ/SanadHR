using HR.Domain.Common;

namespace HR.Domain.Engines.Reports;

public class ReportGrouping : BaseEntity
{
    public Guid ReportDefinitionId { get; set; }
    public string FieldCode { get; set; } = null!;
    public int SortOrder { get; set; }

    public ReportDefinition ReportDefinition { get; set; } = null!;
}
