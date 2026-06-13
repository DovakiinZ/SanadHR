using AutoMapper;
using HR.Application.Common.Exceptions;
using HR.Domain.Engines.Documents;
using HR.Infrastructure.Persistence;
using HR.Modules.Platform.DTOs.Documents;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Commands.Documents;

public record CreatePageTemplateCommand : IRequest<PageTemplateDto>
{
    public string Code { get; init; } = null!;
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public string? Description { get; init; }
    public string? HeaderConfig { get; init; }
    public string? FooterConfig { get; init; }
    public string? Margins { get; init; }
    public string? Watermark { get; init; }
}

public record UpdatePageTemplateCommand : IRequest<PageTemplateDto>
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public string? Description { get; init; }
    public string? HeaderConfig { get; init; }
    public string? FooterConfig { get; init; }
    public string? Margins { get; init; }
    public string? Watermark { get; init; }
}

public record DeletePageTemplateCommand(Guid Id) : IRequest;

public class CreatePageTemplateCommandHandler : IRequestHandler<CreatePageTemplateCommand, PageTemplateDto>
{
    private readonly ApplicationDbContext _context; private readonly IMapper _mapper;
    public CreatePageTemplateCommandHandler(ApplicationDbContext context, IMapper mapper) { _context = context; _mapper = mapper; }
    public async Task<PageTemplateDto> Handle(CreatePageTemplateCommand request, CancellationToken ct)
    {
        var entity = new PageTemplate
        {
            Code = request.Code, NameEn = request.NameEn, NameAr = request.NameAr, Description = request.Description,
            HeaderConfig = request.HeaderConfig, FooterConfig = request.FooterConfig, Margins = request.Margins, Watermark = request.Watermark,
        };
        _context.Set<PageTemplate>().Add(entity); await _context.SaveChangesAsync(ct);
        return _mapper.Map<PageTemplateDto>(entity);
    }
}

public class UpdatePageTemplateCommandHandler : IRequestHandler<UpdatePageTemplateCommand, PageTemplateDto>
{
    private readonly ApplicationDbContext _context; private readonly IMapper _mapper;
    public UpdatePageTemplateCommandHandler(ApplicationDbContext context, IMapper mapper) { _context = context; _mapper = mapper; }
    public async Task<PageTemplateDto> Handle(UpdatePageTemplateCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<PageTemplate>().FindAsync(new object[] { request.Id }, ct) ?? throw new NotFoundException("PageTemplate", request.Id);
        entity.NameEn = request.NameEn; entity.NameAr = request.NameAr; entity.Description = request.Description;
        entity.HeaderConfig = request.HeaderConfig; entity.FooterConfig = request.FooterConfig; entity.Margins = request.Margins; entity.Watermark = request.Watermark;
        await _context.SaveChangesAsync(ct);
        return _mapper.Map<PageTemplateDto>(entity);
    }
}

public class DeletePageTemplateCommandHandler : IRequestHandler<DeletePageTemplateCommand>
{
    private readonly ApplicationDbContext _context;
    public DeletePageTemplateCommandHandler(ApplicationDbContext context) { _context = context; }
    public async Task Handle(DeletePageTemplateCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<PageTemplate>().FindAsync(new object[] { request.Id }, ct) ?? throw new NotFoundException("PageTemplate", request.Id);
        if (entity.IsSystem) throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure("pageTemplate", "لا يمكن حذف قالب صفحة نظام.") });
        if (await _context.Set<DocumentTemplate>().AnyAsync(d => d.PageTemplateId == entity.Id, ct))
            throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure("pageTemplate", "هذا القالب مستخدم في مستندات — لا يمكن حذفه.") });
        entity.IsDeleted = true; entity.DeletedAt = DateTime.UtcNow; await _context.SaveChangesAsync(ct);
    }
}
