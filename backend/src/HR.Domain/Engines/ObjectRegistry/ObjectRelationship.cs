using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Engines.ObjectRegistry;

public class ObjectRelationship : BaseEntity
{
    public Guid SourceObjectId { get; set; }
    public Guid TargetObjectId { get; set; }
    public RelationType RelationType { get; set; }
    public string ForeignKeyField { get; set; } = null!;

    public ObjectDefinition SourceObject { get; set; } = null!;
    public ObjectDefinition TargetObject { get; set; } = null!;
}
