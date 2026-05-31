using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Engines.ObjectRegistry;

public class ObjectPermission : BaseEntity
{
    public Guid ObjectDefinitionId { get; set; }
    public PermissionType PermissionType { get; set; }
    public string PermissionCode { get; set; } = null!;

    public ObjectDefinition ObjectDefinition { get; set; } = null!;
}
