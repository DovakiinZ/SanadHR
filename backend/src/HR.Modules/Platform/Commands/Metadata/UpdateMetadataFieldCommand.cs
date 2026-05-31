using HR.Domain.Enums;
using HR.Modules.Platform.DTOs.Metadata;
using MediatR;

namespace HR.Modules.Platform.Commands.Metadata;

public record UpdateMetadataFieldCommand : IRequest<MetadataFieldDto>
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public FieldType FieldType { get; init; }
    public bool IsRequired { get; init; }
    public int SortOrder { get; init; }
    public string? DefaultValue { get; init; }
}

public class UpdateMetadataFieldCommandHandler : IRequestHandler<UpdateMetadataFieldCommand, MetadataFieldDto>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;
    private readonly AutoMapper.IMapper _mapper;

    public UpdateMetadataFieldCommandHandler(HR.Infrastructure.Persistence.ApplicationDbContext context, AutoMapper.IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<MetadataFieldDto> Handle(UpdateMetadataFieldCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.MetadataFields.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("MetadataField", request.Id);

        entity.NameEn = request.NameEn;
        entity.NameAr = request.NameAr;
        entity.FieldType = request.FieldType;
        entity.IsRequired = request.IsRequired;
        entity.SortOrder = request.SortOrder;
        entity.DefaultValue = request.DefaultValue;

        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<MetadataFieldDto>(entity);
    }
}
