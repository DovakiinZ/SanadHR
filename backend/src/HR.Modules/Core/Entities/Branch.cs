using HR.Domain.Common;

namespace HR.Modules.Core.Entities;

public class Branch : TenantEntity
{
    public string Name { get; set; } = null!;
    public string? NameAr { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public bool IsMainBranch { get; set; }
}
