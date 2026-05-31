using HR.Domain.Enums;
using HR.Modules.Platform.DTOs.ObjectRegistry;
using MediatR;

namespace HR.Modules.Platform.Commands.ObjectRegistry;

public record AddObjectFieldCommand : IRequest<ObjectFieldDto>
{
    public Guid ObjectDefinitionId { get; init; }
    public string Code { get; init; } = null!;
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public FieldType FieldType { get; init; }
    public bool IsFilterable { get; init; }
    public bool IsSortable { get; init; }
    public bool IsSearchable { get; init; }
}

public class AddObjectFieldCommandHandler : IRequestHandler<AddObjectFieldCommand, ObjectFieldDto>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;
    private readonly AutoMapper.IMapper _mapper;

    public AddObjectFieldCommandHandler(HR.Infrastructure.Persistence.ApplicationDbContext context, AutoMapper.IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ObjectFieldDto> Handle(AddObjectFieldCommand request, CancellationToken cancellationToken)
    {
        _ = await _context.ObjectDefinitions.FindAsync(new object[] { request.ObjectDefinitionId }, cancellationToken)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("ObjectDefinition", request.ObjectDefinitionId);

        var entity = new HR.Domain.Engines.ObjectRegistry.ObjectField
        {
            ObjectDefinitionId = request.ObjectDefinitionId,
            Code = request.Code,
            NameEn = request.NameEn,
            NameAr = request.NameAr,
            FieldType = request.FieldType,
            IsFilterable = request.IsFilterable,
            IsSortable = request.IsSortable,
            IsSearchable = request.IsSearchable
        };

        _context.ObjectFields.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ObjectFieldDto>(entity);
    }
}
