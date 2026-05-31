using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Engines.Reports;

public class ReportFilter : BaseEntity
{
    public Guid ReportDefinitionId { get; set; }
    public string FieldCode { get; set; } = null!;
    public ReportFilterOperator Operator { get; set; }
    public string? Value { get; set; }
    public string? ValueTo { get; set; } // For Between operator
    public string? LogicalOperator { get; set; } // AND / OR
    public int SortOrder { get; set; }
    public bool IsParameter { get; set; } // Runtime parameter

    public ReportDefinition ReportDefinition { get; set; } = null!;
}
