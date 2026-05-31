using HR.Domain.Common;

namespace HR.Domain.Engines.Metadata;

public class MetadataDefinition : TenantEntity
{
    public string Code { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string? Description { get; set; }
    public string Module { get; set; } = null!;
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }

    public ICollection<MetadataField> Fields { get; set; } = new List<MetadataField>();
    public ICollection<MetadataValue> Values { get; set; } = new List<MetadataValue>();
}
