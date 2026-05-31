using AutoMapper;
using HR.Application.Common.Exceptions;
using HR.Infrastructure.Persistence;
using HR.Modules.Platform.DTOs.Metadata;
using MediatR;

namespace HR.Modules.Platform.Commands.Metadata;

public class UpdateMetadataDefinitionCommandHandler : IRequestHandler<UpdateMetadataDefinitionCommand, MetadataDefinitionDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdateMetadataDefinitionCommandHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<MetadataDefinitionDto> Handle(UpdateMetadataDefinitionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.MetadataDefinitions.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new NotFoundException("MetadataDefinition", request.Id);

        entity.NameEn = request.NameEn;
        entity.NameAr = request.NameAr;
        entity.Description = request.Description;
        entity.IsActive = request.IsActive;
        entity.SortOrder = request.SortOrder;

        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<MetadataDefinitionDto>(entity);
    }
}
