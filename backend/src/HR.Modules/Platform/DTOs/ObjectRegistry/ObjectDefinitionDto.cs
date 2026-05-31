namespace HR.Modules.Platform.DTOs.ObjectRegistry;

public class ObjectDefinitionDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string Module { get; set; } = null!;
    public string TableName { get; set; } = null!;
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; }
    public List<ObjectFieldDto> Fields { get; set; } = new();
    public List<ObjectRelationshipDto> Relationships { get; set; } = new();
}

public class ObjectFieldDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string FieldType { get; set; } = null!;
    public bool IsFilterable { get; set; }
    public bool IsSortable { get; set; }
    public bool IsSearchable { get; set; }
}

public class ObjectRelationshipDto
{
    public Guid Id { get; set; }
    public Guid SourceObjectId { get; set; }
    public Guid TargetObjectId { get; set; }
    public string RelationType { get; set; } = null!;
    public string ForeignKeyField { get; set; } = null!;
}
