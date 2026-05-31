using HR.Modules.Platform.DTOs.ObjectRegistry;
using MediatR;

namespace HR.Modules.Platform.Commands.ObjectRegistry;

public record CreateObjectDefinitionCommand : IRequest<ObjectDefinitionDto>
{
    public string Code { get; init; } = null!;
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public string Module { get; init; } = null!;
    public string TableName { get; init; } = null!;
    public bool IsSystem { get; init; }
}

public class CreateObjectDefinitionCommandHandler : IRequestHandler<CreateObjectDefinitionCommand, ObjectDefinitionDto>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;
    private readonly AutoMapper.IMapper _mapper;

    public CreateObjectDefinitionCommandHandler(HR.Infrastructure.Persistence.ApplicationDbContext context, AutoMapper.IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ObjectDefinitionDto> Handle(CreateObjectDefinitionCommand request, CancellationToken cancellationToken)
    {
        var entity = new HR.Domain.Engines.ObjectRegistry.ObjectDefinition
        {
            Code = request.Code,
            NameEn = request.NameEn,
            NameAr = request.NameAr,
            Module = request.Module,
            TableName = request.TableName,
            IsSystem = request.IsSystem
        };

        _context.ObjectDefinitions.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ObjectDefinitionDto>(entity);
    }
}
