using HR.Domain.Common;

namespace HR.Domain.Engines.Metadata;

public class MetadataValue : TenantEntity
{
    public Guid MetadataDefinitionId { get; set; }
    public Guid EntityId { get; set; }
    public string EntityType { get; set; } = null!;
    public string? Values { get; set; } // JSONB

    public MetadataDefinition MetadataDefinition { get; set; } = null!;
}
