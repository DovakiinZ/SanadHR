using HR.Modules.Platform.DTOs.Metadata;
using MediatR;

namespace HR.Modules.Platform.Commands.Metadata;

public record CreateMetadataDefinitionCommand : IRequest<MetadataDefinitionDto>
{
    public string Code { get; init; } = null!;
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public string? Description { get; init; }
    public string Module { get; init; } = null!;
    public bool IsSystem { get; init; }
    public int SortOrder { get; init; }
}
