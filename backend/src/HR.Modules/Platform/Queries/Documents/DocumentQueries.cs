using AutoMapper;
using HR.Application.Common.Models;
using HR.Domain.Engines.Documents;
using HR.Infrastructure.Persistence;
using HR.Modules.Platform.DTOs.Documents;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Queries.Documents;

public record GetDocumentTemplatesQuery : IRequest<PaginatedList<DocumentTemplateDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Module { get; init; }
    public string? Search { get; init; }
}

public record GetDocumentTemplateByIdQuery(Guid Id) : IRequest<DocumentTemplateDto>;
public record GetDocumentTemplateVersionsQuery(Guid TemplateId) : IRequest<List<DocumentTemplateVersionDto>>;
public record GetGeneratedDocumentsQuery : IRequest<PaginatedList<GeneratedDocumentDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public Guid? TemplateId { get; init; }
    public string? EntityType { get; init; }
}
public record GetCompanyBrandingQuery : IRequest<List<CompanyBrandingDto>>;

public class GetDocumentTemplatesQueryHandler : IRequestHandler<GetDocumentTemplatesQuery, PaginatedList<DocumentTemplateDto>>
{
    private readonly ApplicationDbContext _context; private readonly IMapper _mapper;
    public GetDocumentTemplatesQueryHandler(ApplicationDbContext context, IMapper mapper) { _context = context; _mapper = mapper; }
    public async Task<PaginatedList<DocumentTemplateDto>> Handle(GetDocumentTemplatesQuery request, CancellationToken ct)
    {
        var query = _context.Set<DocumentTemplate>().Include(t => t.Tokens).AsQueryable();
        if (!string.IsNullOrEmpty(request.Module)) query = query.Where(t => t.Module == request.Module);
        if (!string.IsNullOrEmpty(request.Search)) query = query.Where(t => t.NameEn.Contains(request.Search) || t.NameAr.Contains(request.Search));
        var totalCount = await query.CountAsync(ct);
        var items = await query.OrderByDescending(t => t.CreatedAt).Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).ToListAsync(ct);
        return new PaginatedList<DocumentTemplateDto> { Items = _mapper.Map<List<DocumentTemplateDto>>(items), PageNumber = request.PageNumber, PageSize = request.PageSize, TotalCount = totalCount };
    }
}

public class GetDocumentTemplateByIdQueryHandler : IRequestHandler<GetDocumentTemplateByIdQuery, DocumentTemplateDto>
{
    private readonly ApplicationDbContext _context; private readonly IMapper _mapper;
    public GetDocumentTemplateByIdQueryHandler(ApplicationDbContext context, IMapper mapper) { _context = context; _mapper = mapper; }
    public async Task<DocumentTemplateDto> Handle(GetDocumentTemplateByIdQuery request, CancellationToken ct)
    {
        var entity = await _context.Set<DocumentTemplate>().Include(t => t.Tokens).FirstOrDefaultAsync(t => t.Id == request.Id, ct) ?? throw new HR.Application.Common.Exceptions.NotFoundException("DocumentTemplate", request.Id);
        return _mapper.Map<DocumentTemplateDto>(entity);
    }
}

public class GetDocumentTemplateVersionsQueryHandler : IRequestHandler<GetDocumentTemplateVersionsQuery, List<DocumentTemplateVersionDto>>
{
    private readonly ApplicationDbContext _context; private readonly IMapper _mapper;
    public GetDocumentTemplateVersionsQueryHandler(ApplicationDbContext context, IMapper mapper) { _context = context; _mapper = mapper; }
    public async Task<List<DocumentTemplateVersionDto>> Handle(GetDocumentTemplateVersionsQuery request, CancellationToken ct)
    {
        var items = await _context.Set<DocumentTemplateVersion>().Where(v => v.DocumentTemplateId == request.TemplateId).OrderByDescending(v => v.VersionNumber).ToListAsync(ct);
        return _mapper.Map<List<DocumentTemplateVersionDto>>(items);
    }
}

public class GetGeneratedDocumentsQueryHandler : IRequestHandler<GetGeneratedDocumentsQuery, PaginatedList<GeneratedDocumentDto>>
{
    private readonly ApplicationDbContext _context; private readonly IMapper _mapper;
    public GetGeneratedDocumentsQueryHandler(ApplicationDbContext context, IMapper mapper) { _context = context; _mapper = mapper; }
    public async Task<PaginatedList<GeneratedDocumentDto>> Handle(GetGeneratedDocumentsQuery request, CancellationToken ct)
    {
        var query = _context.Set<GeneratedDocument>().AsQueryable();
        if (request.TemplateId.HasValue) query = query.Where(d => d.DocumentTemplateId == request.TemplateId);
        if (!string.IsNullOrEmpty(request.EntityType)) query = query.Where(d => d.EntityType == request.EntityType);
        var totalCount = await query.CountAsync(ct);
        var items = await query.OrderByDescending(d => d.GeneratedAt).Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).ToListAsync(ct);
        return new PaginatedList<GeneratedDocumentDto> { Items = _mapper.Map<List<GeneratedDocumentDto>>(items), PageNumber = request.PageNumber, PageSize = request.PageSize, TotalCount = totalCount };
    }
}

public class GetCompanyBrandingQueryHandler : IRequestHandler<GetCompanyBrandingQuery, List<CompanyBrandingDto>>
{
    private readonly ApplicationDbContext _context; private readonly IMapper _mapper;
    public GetCompanyBrandingQueryHandler(ApplicationDbContext context, IMapper mapper) { _context = context; _mapper = mapper; }
    public async Task<List<CompanyBrandingDto>> Handle(GetCompanyBrandingQuery request, CancellationToken ct)
    {
        var items = await _context.Set<CompanyBranding>().Where(b => b.IsActive).OrderBy(b => b.SortOrder).ToListAsync(ct);
        return _mapper.Map<List<CompanyBrandingDto>>(items);
    }
}
