using HR.Domain.Common;

namespace HR.Domain.Engines.ObjectRegistry;

public class ObjectDefinition : TenantEntity
{
    public string Code { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string Module { get; set; } = null!;
    public string TableName { get; set; } = null!;
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<ObjectField> Fields { get; set; } = new List<ObjectField>();
    public ICollection<ObjectRelationship> SourceRelationships { get; set; } = new List<ObjectRelationship>();
    public ICollection<ObjectRelationship> TargetRelationships { get; set; } = new List<ObjectRelationship>();
    public ICollection<ObjectPermission> Permissions { get; set; } = new List<ObjectPermission>();
}
