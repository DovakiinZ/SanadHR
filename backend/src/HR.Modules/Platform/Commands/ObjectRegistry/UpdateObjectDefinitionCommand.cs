using HR.Modules.Platform.DTOs.ObjectRegistry;
using MediatR;

namespace HR.Modules.Platform.Commands.ObjectRegistry;

public record UpdateObjectDefinitionCommand : IRequest<ObjectDefinitionDto>
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public string Module { get; init; } = null!;
    public string TableName { get; init; } = null!;
}

public class UpdateObjectDefinitionCommandHandler : IRequestHandler<UpdateObjectDefinitionCommand, ObjectDefinitionDto>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;
    private readonly AutoMapper.IMapper _mapper;

    public UpdateObjectDefinitionCommandHandler(HR.Infrastructure.Persistence.ApplicationDbContext context, AutoMapper.IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ObjectDefinitionDto> Handle(UpdateObjectDefinitionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.ObjectDefinitions.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("ObjectDefinition", request.Id);

        entity.NameEn = request.NameEn;
        entity.NameAr = request.NameAr;
        entity.Module = request.Module;
        entity.TableName = request.TableName;

        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ObjectDefinitionDto>(entity);
    }
}
