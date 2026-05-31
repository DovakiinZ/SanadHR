using HR.Modules.Platform.DTOs.Metadata;
using MediatR;

namespace HR.Modules.Platform.Commands.Metadata;

public record UpdateMetadataOptionCommand : IRequest<MetadataOptionDto>
{
    public Guid Id { get; init; }
    public string Value { get; init; } = null!;
    public string LabelEn { get; init; } = null!;
    public string LabelAr { get; init; } = null!;
    public int SortOrder { get; init; }
    public bool IsDefault { get; init; }
}

public class UpdateMetadataOptionCommandHandler : IRequestHandler<UpdateMetadataOptionCommand, MetadataOptionDto>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;
    private readonly AutoMapper.IMapper _mapper;

    public UpdateMetadataOptionCommandHandler(HR.Infrastructure.Persistence.ApplicationDbContext context, AutoMapper.IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<MetadataOptionDto> Handle(UpdateMetadataOptionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.MetadataOptions.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("MetadataOption", request.Id);

        entity.Value = request.Value;
        entity.LabelEn = request.LabelEn;
        entity.LabelAr = request.LabelAr;
        entity.SortOrder = request.SortOrder;
        entity.IsDefault = request.IsDefault;

        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<MetadataOptionDto>(entity);
    }
}
