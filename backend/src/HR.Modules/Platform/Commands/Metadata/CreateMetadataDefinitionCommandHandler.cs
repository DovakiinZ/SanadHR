using AutoMapper;
using HR.Domain.Engines.Metadata;
using HR.Infrastructure.Persistence;
using HR.Modules.Platform.DTOs.Metadata;
using MediatR;

namespace HR.Modules.Platform.Commands.Metadata;

public class CreateMetadataDefinitionCommandHandler : IRequestHandler<CreateMetadataDefinitionCommand, MetadataDefinitionDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateMetadataDefinitionCommandHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<MetadataDefinitionDto> Handle(CreateMetadataDefinitionCommand request, CancellationToken cancellationToken)
    {
        var entity = new MetadataDefinition
        {
            Code = request.Code,
            NameEn = request.NameEn,
            NameAr = request.NameAr,
            Description = request.Description,
            Module = request.Module,
            IsSystem = request.IsSystem,
            SortOrder = request.SortOrder
        };

        _context.MetadataDefinitions.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<MetadataDefinitionDto>(entity);
    }
}
