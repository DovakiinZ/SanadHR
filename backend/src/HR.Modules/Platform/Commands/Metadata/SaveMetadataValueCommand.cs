using HR.Modules.Platform.DTOs.Metadata;
using MediatR;

namespace HR.Modules.Platform.Commands.Metadata;

public record SaveMetadataValueCommand : IRequest<MetadataValueDto>
{
    public Guid MetadataDefinitionId { get; init; }
    public Guid EntityId { get; init; }
    public string EntityType { get; init; } = null!;
    public string? Values { get; init; }
}
