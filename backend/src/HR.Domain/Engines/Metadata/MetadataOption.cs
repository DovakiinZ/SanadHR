using HR.Domain.Common;

namespace HR.Domain.Engines.Metadata;

public class MetadataOption : BaseEntity
{
    public Guid MetadataFieldId { get; set; }
    public string Value { get; set; } = null!;
    public string LabelEn { get; set; } = null!;
    public string LabelAr { get; set; } = null!;
    public int SortOrder { get; set; }
    public bool IsDefault { get; set; }

    public MetadataField MetadataField { get; set; } = null!;
}
