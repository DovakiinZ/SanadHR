using HR.Domain.Common;

namespace HR.Domain.Engines.Permissions;

public class PermissionTemplate : TenantEntity
{
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsSystem { get; set; }

    public ICollection<PermissionTemplateItem> Items { get; set; } = new List<PermissionTemplateItem>();
}
