using HR.Domain.Enums;
using HR.Modules.Platform.DTOs.ObjectRegistry;
using MediatR;

namespace HR.Modules.Platform.Commands.ObjectRegistry;

public record AddObjectRelationshipCommand : IRequest<ObjectRelationshipDto>
{
    public Guid SourceObjectId { get; init; }
    public Guid TargetObjectId { get; init; }
    public RelationType RelationType { get; init; }
    public string ForeignKeyField { get; init; } = null!;
}

public class AddObjectRelationshipCommandHandler : IRequestHandler<AddObjectRelationshipCommand, ObjectRelationshipDto>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;
    private readonly AutoMapper.IMapper _mapper;

    public AddObjectRelationshipCommandHandler(HR.Infrastructure.Persistence.ApplicationDbContext context, AutoMapper.IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ObjectRelationshipDto> Handle(AddObjectRelationshipCommand request, CancellationToken cancellationToken)
    {
        _ = await _context.ObjectDefinitions.FindAsync(new object[] { request.SourceObjectId }, cancellationToken)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("ObjectDefinition (Source)", request.SourceObjectId);
        _ = await _context.ObjectDefinitions.FindAsync(new object[] { request.TargetObjectId }, cancellationToken)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("ObjectDefinition (Target)", request.TargetObjectId);

        var entity = new HR.Domain.Engines.ObjectRegistry.ObjectRelationship
        {
            SourceObjectId = request.SourceObjectId,
            TargetObjectId = request.TargetObjectId,
            RelationType = request.RelationType,
            ForeignKeyField = request.ForeignKeyField
        };

        _context.ObjectRelationships.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ObjectRelationshipDto>(entity);
    }
}
