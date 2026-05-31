using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Engines.Metadata;

public class MetadataField : BaseEntity
{
    public Guid MetadataDefinitionId { get; set; }
    public string Code { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public FieldType FieldType { get; set; }
    public bool IsRequired { get; set; }
    public int SortOrder { get; set; }
    public string? DefaultValue { get; set; }

    public MetadataDefinition MetadataDefinition { get; set; } = null!;
    public ICollection<MetadataOption> Options { get; set; } = new List<MetadataOption>();
}
