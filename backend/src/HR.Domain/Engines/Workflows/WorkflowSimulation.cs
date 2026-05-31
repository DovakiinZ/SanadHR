using HR.Domain.Common;

namespace HR.Domain.Engines.Workflows;

public class WorkflowSimulation : TenantEntity
{
    public Guid WorkflowVersionId { get; set; }
    public string InputData { get; set; } = null!; // JSONB - simulated entity data
    public string Result { get; set; } = null!; // JSONB - simulation steps/path
    public DateTime SimulatedAt { get; set; }
    public Guid SimulatedById { get; set; }

    public WorkflowVersion WorkflowVersion { get; set; } = null!;
}
