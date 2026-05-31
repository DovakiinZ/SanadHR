using AutoMapper;
using HR.Application.Common.Exceptions;
using HR.Infrastructure.Persistence;
using HR.Modules.Platform.DTOs.ObjectRegistry;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Queries.ObjectRegistry;

public record GetObjectDefinitionsQuery : IRequest<List<ObjectDefinitionDto>>;

public record GetObjectDefinitionByCodeQuery(string Code) : IRequest<ObjectDefinitionDto>;

// Handlers

public class GetObjectDefinitionsQueryHandler : IRequestHandler<GetObjectDefinitionsQuery, List<ObjectDefinitionDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetObjectDefinitionsQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<ObjectDefinitionDto>> Handle(GetObjectDefinitionsQuery request, CancellationToken cancellationToken)
    {
        var objects = await _context.ObjectDefinitions
            .Where(o => o.IsActive)
            .OrderBy(o => o.Module).ThenBy(o => o.Code)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<ObjectDefinitionDto>>(objects);
    }
}

public class GetObjectDefinitionByCodeQueryHandler : IRequestHandler<GetObjectDefinitionByCodeQuery, ObjectDefinitionDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetObjectDefinitionByCodeQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ObjectDefinitionDto> Handle(GetObjectDefinitionByCodeQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.ObjectDefinitions
            .Include(o => o.Fields)
            .Include(o => o.SourceRelationships)
            .Include(o => o.Permissions)
            .FirstOrDefaultAsync(o => o.Code == request.Code, cancellationToken)
            ?? throw new NotFoundException("ObjectDefinition", request.Code);

        return _mapper.Map<ObjectDefinitionDto>(entity);
    }
}
