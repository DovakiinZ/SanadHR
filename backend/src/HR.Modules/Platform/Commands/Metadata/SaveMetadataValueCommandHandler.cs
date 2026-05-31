using AutoMapper;
using HR.Domain.Engines.Metadata;
using HR.Infrastructure.Persistence;
using HR.Modules.Platform.DTOs.Metadata;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Commands.Metadata;

public class SaveMetadataValueCommandHandler : IRequestHandler<SaveMetadataValueCommand, MetadataValueDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public SaveMetadataValueCommandHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<MetadataValueDto> Handle(SaveMetadataValueCommand request, CancellationToken cancellationToken)
    {
        var existing = await _context.MetadataValues
            .FirstOrDefaultAsync(v => v.MetadataDefinitionId == request.MetadataDefinitionId
                && v.EntityId == request.EntityId && v.EntityType == request.EntityType, cancellationToken);

        if (existing != null)
        {
            existing.Values = request.Values;
        }
        else
        {
            existing = new MetadataValue
            {
                MetadataDefinitionId = request.MetadataDefinitionId,
                EntityId = request.EntityId,
                EntityType = request.EntityType,
                Values = request.Values
            };
            _context.MetadataValues.Add(existing);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<MetadataValueDto>(existing);
    }
}
