using HR.Domain.Common;

namespace HR.Modules.ESS.Entities;

// TODO: Implement self-service request entity
public class SelfServiceRequest : TenantEntity
{
    public Guid EmployeeId { get; set; }
    public string RequestType { get; set; } = null!;
    public string? Details { get; set; }
}
