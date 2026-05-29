using HR.Domain.Common;

namespace HR.Modules.Tenancy.Entities;

public class Tenant : AuditableEntity
{
    public string CompanyName { get; set; } = null!;
    public string? CompanyNameAr { get; set; }
    public string? LogoUrl { get; set; }
    public string? Domain { get; set; }
    public bool IsActive { get; set; } = true;
    public string? SubscriptionPlan { get; set; }
    public DateTime? SubscriptionExpiresAt { get; set; }
}
