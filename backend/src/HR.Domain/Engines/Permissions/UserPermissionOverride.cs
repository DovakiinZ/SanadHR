using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Engines.Permissions;

public class UserPermissionOverride : TenantEntity
{
    public Guid UserId { get; set; }
    public string PermissionCode { get; set; } = null!;
    public bool IsGranted { get; set; }
    public ScopeType Scope { get; set; }
}
