using HR.Domain.Enums;
using HR.Modules.Platform.DTOs.ObjectRegistry;
using MediatR;

namespace HR.Modules.Platform.Commands.ObjectRegistry;

public record UpdateObjectFieldCommand : IRequest<ObjectFieldDto>
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public FieldType FieldType { get; init; }
    public bool IsFilterable { get; init; }
    public bool IsSortable { get; init; }
    public bool IsSearchable { get; init; }
}

public class UpdateObjectFieldCommandHandler : IRequestHandler<UpdateObjectFieldCommand, ObjectFieldDto>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;
    private readonly AutoMapper.IMapper _mapper;

    public UpdateObjectFieldCommandHandler(HR.Infrastructure.Persistence.ApplicationDbContext context, AutoMapper.IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ObjectFieldDto> Handle(UpdateObjectFieldCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.ObjectFields.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("ObjectField", request.Id);

        entity.NameEn = request.NameEn;
        entity.NameAr = request.NameAr;
        entity.FieldType = request.FieldType;
        entity.IsFilterable = request.IsFilterable;
        entity.IsSortable = request.IsSortable;
        entity.IsSearchable = request.IsSearchable;

        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ObjectFieldDto>(entity);
    }
}
