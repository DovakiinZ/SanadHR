using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Engines.ObjectRegistry;

public class ObjectField : BaseEntity
{
    public Guid ObjectDefinitionId { get; set; }
    public string Code { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public FieldType FieldType { get; set; }
    public bool IsFilterable { get; set; }
    public bool IsSortable { get; set; }
    public bool IsSearchable { get; set; }

    public ObjectDefinition ObjectDefinition { get; set; } = null!;
}
