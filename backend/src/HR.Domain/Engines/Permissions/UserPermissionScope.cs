using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Engines.Permissions;

public class UserPermissionScope : BaseEntity
{
    public Guid UserId { get; set; }
    public ScopeType ScopeType { get; set; }
    public Guid ScopeValue { get; set; }
}
