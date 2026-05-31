using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Engines.Reports;

public class ReportSchedule : BaseEntity
{
    public Guid ReportDefinitionId { get; set; }
    public ReportScheduleFrequency Frequency { get; set; }
    public string? CronExpression { get; set; }
    public ExportFormat ExportFormat { get; set; }
    public string Recipients { get; set; } = null!; // JSONB - list of email/userId
    public bool IsActive { get; set; } = true;
    public DateTime? LastRunAt { get; set; }
    public DateTime? NextRunAt { get; set; }

    public ReportDefinition ReportDefinition { get; set; } = null!;
}
