using HR.Domain.Enums;
using HR.Modules.Platform.DTOs.Metadata;
using MediatR;

namespace HR.Modules.Platform.Commands.Metadata;

public record AddMetadataFieldCommand : IRequest<MetadataFieldDto>
{
    public Guid MetadataDefinitionId { get; init; }
    public string Code { get; init; } = null!;
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public FieldType FieldType { get; init; }
    public bool IsRequired { get; init; }
    public int SortOrder { get; init; }
    public string? DefaultValue { get; init; }
}

public class AddMetadataFieldCommandHandler : IRequestHandler<AddMetadataFieldCommand, MetadataFieldDto>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;
    private readonly AutoMapper.IMapper _mapper;

    public AddMetadataFieldCommandHandler(HR.Infrastructure.Persistence.ApplicationDbContext context, AutoMapper.IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<MetadataFieldDto> Handle(AddMetadataFieldCommand request, CancellationToken cancellationToken)
    {
        var definition = await _context.MetadataDefinitions.FindAsync(new object[] { request.MetadataDefinitionId }, cancellationToken)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("MetadataDefinition", request.MetadataDefinitionId);

        var entity = new HR.Domain.Engines.Metadata.MetadataField
        {
            MetadataDefinitionId = request.MetadataDefinitionId,
            Code = request.Code,
            NameEn = request.NameEn,
            NameAr = request.NameAr,
            FieldType = request.FieldType,
            IsRequired = request.IsRequired,
            SortOrder = request.SortOrder,
            DefaultValue = request.DefaultValue
        };

        _context.MetadataFields.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<MetadataFieldDto>(entity);
    }
}
