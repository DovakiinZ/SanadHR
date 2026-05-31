using HR.Domain.Common;

namespace HR.Domain.Engines.Permissions;

public class UserPermissionTemplate : TenantEntity
{
    public Guid UserId { get; set; }
    public Guid PermissionTemplateId { get; set; }
    public DateTime AssignedAt { get; set; }
    public string? AssignedBy { get; set; }

    public PermissionTemplate PermissionTemplate { get; set; } = null!;
}
