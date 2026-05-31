using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Engines.Permissions;

public class PermissionTemplateItem : BaseEntity
{
    public Guid PermissionTemplateId { get; set; }
    public string PermissionCode { get; set; } = null!;
    public ScopeType Scope { get; set; }

    public PermissionTemplate PermissionTemplate { get; set; } = null!;
}
