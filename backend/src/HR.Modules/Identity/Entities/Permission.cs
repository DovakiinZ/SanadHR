using HR.Domain.Common;

namespace HR.Modules.Identity.Entities;

public class Permission : BaseEntity
{
    public string Module { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
