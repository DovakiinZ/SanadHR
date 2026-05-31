using HR.Modules.Platform.DTOs.Metadata;
using MediatR;

namespace HR.Modules.Platform.Commands.Metadata;

public record AddMetadataOptionCommand : IRequest<MetadataOptionDto>
{
    public Guid MetadataFieldId { get; init; }
    public string Value { get; init; } = null!;
    public string LabelEn { get; init; } = null!;
    public string LabelAr { get; init; } = null!;
    public int SortOrder { get; init; }
    public bool IsDefault { get; init; }
}

public class AddMetadataOptionCommandHandler : IRequestHandler<AddMetadataOptionCommand, MetadataOptionDto>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;
    private readonly AutoMapper.IMapper _mapper;

    public AddMetadataOptionCommandHandler(HR.Infrastructure.Persistence.ApplicationDbContext context, AutoMapper.IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<MetadataOptionDto> Handle(AddMetadataOptionCommand request, CancellationToken cancellationToken)
    {
        var field = await _context.MetadataFields.FindAsync(new object[] { request.MetadataFieldId }, cancellationToken)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("MetadataField", request.MetadataFieldId);

        var entity = new HR.Domain.Engines.Metadata.MetadataOption
        {
            MetadataFieldId = request.MetadataFieldId,
            Value = request.Value,
            LabelEn = request.LabelEn,
            LabelAr = request.LabelAr,
            SortOrder = request.SortOrder,
            IsDefault = request.IsDefault
        };

        _context.MetadataOptions.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<MetadataOptionDto>(entity);
    }
}
