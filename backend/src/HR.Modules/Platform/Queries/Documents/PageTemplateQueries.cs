using AutoMapper;
using HR.Application.Common.Exceptions;
using HR.Domain.Engines.Documents;
using HR.Infrastructure.Persistence;
using HR.Modules.Platform.DTOs.Documents;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Queries.Documents;

public record GetPageTemplatesQuery : IRequest<List<PageTemplateDto>>;
public record GetPageTemplateByIdQuery(Guid Id) : IRequest<PageTemplateDto>;

public class GetPageTemplatesQueryHandler : IRequestHandler<GetPageTemplatesQuery, List<PageTemplateDto>>
{
    private readonly ApplicationDbContext _context; private readonly IMapper _mapper;
    public GetPageTemplatesQueryHandler(ApplicationDbContext context, IMapper mapper) { _context = context; _mapper = mapper; }
    public async Task<List<PageTemplateDto>> Handle(GetPageTemplatesQuery request, CancellationToken ct)
    {
        var items = await _context.Set<PageTemplate>().Where(p => p.IsActive).OrderBy(p => p.SortOrder).ThenBy(p => p.NameAr).ToListAsync(ct);
        return _mapper.Map<List<PageTemplateDto>>(items);
    }
}

public class GetPageTemplateByIdQueryHandler : IRequestHandler<GetPageTemplateByIdQuery, PageTemplateDto>
{
    private readonly ApplicationDbContext _context; private readonly IMapper _mapper;
    public GetPageTemplateByIdQueryHandler(ApplicationDbContext context, IMapper mapper) { _context = context; _mapper = mapper; }
    public async Task<PageTemplateDto> Handle(GetPageTemplateByIdQuery request, CancellationToken ct)
    {
        var entity = await _context.Set<PageTemplate>().FirstOrDefaultAsync(p => p.Id == request.Id, ct) ?? throw new NotFoundException("PageTemplate", request.Id);
        return _mapper.Map<PageTemplateDto>(entity);
    }
}
