namespace HR.Modules.Platform.DTOs.Metadata;

public class MetadataDefinitionDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string? Description { get; set; }
    public string Module { get; set; } = null!;
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public List<MetadataFieldDto> Fields { get; set; } = new();
}

public class MetadataFieldDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string FieldType { get; set; } = null!;
    public bool IsRequired { get; set; }
    public int SortOrder { get; set; }
    public string? DefaultValue { get; set; }
    public List<MetadataOptionDto> Options { get; set; } = new();
}

public class MetadataOptionDto
{
    public Guid Id { get; set; }
    public string Value { get; set; } = null!;
    public string LabelEn { get; set; } = null!;
    public string LabelAr { get; set; } = null!;
    public int SortOrder { get; set; }
    public bool IsDefault { get; set; }
}

public class MetadataValueDto
{
    public Guid Id { get; set; }
    public Guid MetadataDefinitionId { get; set; }
    public Guid EntityId { get; set; }
    public string EntityType { get; set; } = null!;
    public string? Values { get; set; }
}
