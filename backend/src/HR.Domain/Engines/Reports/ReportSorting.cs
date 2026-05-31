using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Engines.Reports;

public class ReportSorting : BaseEntity
{
    public Guid ReportDefinitionId { get; set; }
    public string FieldCode { get; set; } = null!;
    public SortDirection Direction { get; set; }
    public int SortOrder { get; set; }

    public ReportDefinition ReportDefinition { get; set; } = null!;
}
