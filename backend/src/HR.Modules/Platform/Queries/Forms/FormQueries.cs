using AutoMapper;
using HR.Application.Common.Models;
using HR.Infrastructure.Persistence;
using HR.Modules.Platform.DTOs.Forms;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Queries.Forms;

public record GetFormDefinitionsQuery : IRequest<PaginatedList<FormDefinitionDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Module { get; init; }
}

public record GetFormDefinitionByIdQuery(Guid Id) : IRequest<FormDefinitionDto>;

public record GetFormSubmissionsQuery : IRequest<PaginatedList<FormSubmissionDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public Guid? FormDefinitionId { get; init; }
}

// Handlers

public class GetFormDefinitionsQueryHandler : IRequestHandler<GetFormDefinitionsQuery, PaginatedList<FormDefinitionDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetFormDefinitionsQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<FormDefinitionDto>> Handle(GetFormDefinitionsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.FormDefinitions
            .Include(f => f.Fields.OrderBy(ff => ff.SortOrder))
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.Module))
            query = query.Where(f => f.Module == request.Module);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(f => f.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedList<FormDefinitionDto>
        {
            Items = _mapper.Map<List<FormDefinitionDto>>(items),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}

public class GetFormDefinitionByIdQueryHandler : IRequestHandler<GetFormDefinitionByIdQuery, FormDefinitionDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetFormDefinitionByIdQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<FormDefinitionDto> Handle(GetFormDefinitionByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.FormDefinitions
            .Include(f => f.Fields.OrderBy(ff => ff.SortOrder))
            .FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken)
            ?? throw new Application.Common.Exceptions.NotFoundException("FormDefinition", request.Id);

        return _mapper.Map<FormDefinitionDto>(entity);
    }
}

public class GetFormSubmissionsQueryHandler : IRequestHandler<GetFormSubmissionsQuery, PaginatedList<FormSubmissionDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetFormSubmissionsQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<FormSubmissionDto>> Handle(GetFormSubmissionsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.FormSubmissions
            .Include(s => s.Values)
            .AsQueryable();

        if (request.FormDefinitionId.HasValue)
            query = query.Where(s => s.FormDefinitionId == request.FormDefinitionId);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(s => s.SubmittedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedList<FormSubmissionDto>
        {
            Items = _mapper.Map<List<FormSubmissionDto>>(items),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}
