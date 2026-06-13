using AutoMapper;
using HR.Application.Common.Exceptions;
using HR.Domain.Engines.Documents;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using HR.Modules.Platform.DTOs.Documents;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Commands.Documents;

public record CreateDocumentTemplateCommand : IRequest<DocumentTemplateDto>
{
    public string Code { get; init; } = null!;
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public string? Description { get; init; }
    public string Module { get; init; } = "Requests";
    public DocumentOutputFormat OutputFormat { get; init; }
    public string? LayoutJson { get; init; }
    public string? BodyTemplate { get; init; }
    public string? HeaderTemplate { get; init; }
    public string? FooterTemplate { get; init; }
    public string? StyleSheet { get; init; }
    public bool UseBranding { get; init; } = true;
    public Guid? PageTemplateId { get; init; }
    public string? PageSettings { get; init; }
}

public record UpdateDocumentTemplateCommand : IRequest<DocumentTemplateDto>
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public string? Description { get; init; }
    public string? LayoutJson { get; init; }
    public string? BodyTemplate { get; init; }
    public string? HeaderTemplate { get; init; }
    public string? FooterTemplate { get; init; }
    public string? StyleSheet { get; init; }
    public bool UseBranding { get; init; }
    public Guid? PageTemplateId { get; init; }
    public string? PageSettings { get; init; }
}

public record DeleteDocumentTemplateCommand(Guid Id) : IRequest;
public record PublishDocumentTemplateCommand(Guid Id) : IRequest<DocumentTemplateDto>;
public record DuplicateDocumentTemplateCommand(Guid Id) : IRequest<DocumentTemplateDto>;

public record GenerateDocumentCommand : IRequest<GeneratedDocumentDto>
{
    public Guid DocumentTemplateId { get; init; }
    public string EntityType { get; init; } = null!;
    public Guid EntityId { get; init; }
    public string? TokenValues { get; init; }
}

public record AddDocumentTokenCommand : IRequest<DocumentTemplateTokenDto>
{
    public Guid DocumentTemplateId { get; init; }
    public string TokenCode { get; init; } = null!;
    public string? DefaultValue { get; init; }
    public bool IsRequired { get; init; }
}

public record DeleteDocumentTokenCommand(Guid Id) : IRequest;

public record SaveCompanyBrandingCommand : IRequest<CompanyBrandingDto>
{
    public BrandingElementType ElementType { get; init; }
    public string? ImageUrl { get; init; }
    public string? Content { get; init; }
    public string? Configuration { get; init; }
}

// === Handlers ===

public class CreateDocumentTemplateCommandHandler : IRequestHandler<CreateDocumentTemplateCommand, DocumentTemplateDto>
{
    private readonly ApplicationDbContext _context; private readonly IMapper _mapper;
    public CreateDocumentTemplateCommandHandler(ApplicationDbContext context, IMapper mapper) { _context = context; _mapper = mapper; }
    public async Task<DocumentTemplateDto> Handle(CreateDocumentTemplateCommand request, CancellationToken ct)
    {
        var entity = new DocumentTemplate { Code = request.Code, NameEn = request.NameEn, NameAr = request.NameAr, Description = request.Description, Module = request.Module, OutputFormat = request.OutputFormat, LayoutJson = request.LayoutJson, BodyTemplate = request.BodyTemplate, HeaderTemplate = request.HeaderTemplate, FooterTemplate = request.FooterTemplate, StyleSheet = request.StyleSheet, UseBranding = request.UseBranding, PageTemplateId = request.PageTemplateId, PageSettings = request.PageSettings };
        _context.Set<DocumentTemplate>().Add(entity); await _context.SaveChangesAsync(ct);
        return _mapper.Map<DocumentTemplateDto>(entity);
    }
}

public class UpdateDocumentTemplateCommandHandler : IRequestHandler<UpdateDocumentTemplateCommand, DocumentTemplateDto>
{
    private readonly ApplicationDbContext _context; private readonly IMapper _mapper;
    public UpdateDocumentTemplateCommandHandler(ApplicationDbContext context, IMapper mapper) { _context = context; _mapper = mapper; }
    public async Task<DocumentTemplateDto> Handle(UpdateDocumentTemplateCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<DocumentTemplate>().FindAsync(new object[] { request.Id }, ct) ?? throw new NotFoundException("DocumentTemplate", request.Id);
        // Save version before update (snapshot the layout, falling back to legacy HTML; column is required).
        _context.Set<DocumentTemplateVersion>().Add(new DocumentTemplateVersion { DocumentTemplateId = entity.Id, VersionNumber = entity.Version, BodyTemplate = entity.LayoutJson ?? entity.BodyTemplate ?? "", HeaderTemplate = entity.HeaderTemplate, FooterTemplate = entity.FooterTemplate, CreatedAt = DateTime.UtcNow });
        entity.NameEn = request.NameEn; entity.NameAr = request.NameAr; entity.Description = request.Description; entity.LayoutJson = request.LayoutJson; entity.BodyTemplate = request.BodyTemplate; entity.HeaderTemplate = request.HeaderTemplate; entity.FooterTemplate = request.FooterTemplate; entity.StyleSheet = request.StyleSheet; entity.UseBranding = request.UseBranding; entity.PageTemplateId = request.PageTemplateId; entity.PageSettings = request.PageSettings; entity.Version++;
        await _context.SaveChangesAsync(ct);
        return _mapper.Map<DocumentTemplateDto>(entity);
    }
}

public class DeleteDocumentTemplateCommandHandler : IRequestHandler<DeleteDocumentTemplateCommand>
{
    private readonly ApplicationDbContext _context;
    public DeleteDocumentTemplateCommandHandler(ApplicationDbContext context) { _context = context; }
    public async Task Handle(DeleteDocumentTemplateCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<DocumentTemplate>().FindAsync(new object[] { request.Id }, ct) ?? throw new NotFoundException("DocumentTemplate", request.Id);
        if (entity.IsSystem) throw new HR.Application.Common.Exceptions.ValidationException(new[] { new FluentValidation.Results.ValidationFailure("template", "لا يمكن حذف قالب نظام — يمكنك نسخه وتعديل النسخة.") });
        entity.IsDeleted = true; entity.DeletedAt = DateTime.UtcNow; await _context.SaveChangesAsync(ct);
    }
}

public class DuplicateDocumentTemplateCommandHandler : IRequestHandler<DuplicateDocumentTemplateCommand, DocumentTemplateDto>
{
    private readonly ApplicationDbContext _context; private readonly IMapper _mapper;
    public DuplicateDocumentTemplateCommandHandler(ApplicationDbContext context, IMapper mapper) { _context = context; _mapper = mapper; }
    public async Task<DocumentTemplateDto> Handle(DuplicateDocumentTemplateCommand request, CancellationToken ct)
    {
        var src = await _context.Set<DocumentTemplate>().FindAsync(new object[] { request.Id }, ct) ?? throw new NotFoundException("DocumentTemplate", request.Id);
        var baseCode = $"{src.Code}_COPY";
        var code = baseCode; var i = 1;
        while (await _context.Set<DocumentTemplate>().AnyAsync(d => d.Code == code, ct)) code = $"{baseCode}{++i}";
        var copy = new DocumentTemplate
        {
            Code = code, NameEn = src.NameEn + " (Copy)", NameAr = src.NameAr + " (نسخة)", Description = src.Description,
            Module = src.Module, OutputFormat = src.OutputFormat, Status = DocumentTemplateStatus.Draft,
            LayoutJson = src.LayoutJson, BodyTemplate = src.BodyTemplate, HeaderTemplate = src.HeaderTemplate, FooterTemplate = src.FooterTemplate,
            StyleSheet = src.StyleSheet, UseBranding = src.UseBranding, PageTemplateId = src.PageTemplateId, PageSettings = src.PageSettings,
            IsSystem = false, IsActive = true,
        };
        _context.Set<DocumentTemplate>().Add(copy); await _context.SaveChangesAsync(ct);
        return _mapper.Map<DocumentTemplateDto>(copy);
    }
}

public class PublishDocumentTemplateCommandHandler : IRequestHandler<PublishDocumentTemplateCommand, DocumentTemplateDto>
{
    private readonly ApplicationDbContext _context; private readonly IMapper _mapper;
    public PublishDocumentTemplateCommandHandler(ApplicationDbContext context, IMapper mapper) { _context = context; _mapper = mapper; }
    public async Task<DocumentTemplateDto> Handle(PublishDocumentTemplateCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<DocumentTemplate>().FindAsync(new object[] { request.Id }, ct) ?? throw new NotFoundException("DocumentTemplate", request.Id);
        entity.Status = DocumentTemplateStatus.Published; await _context.SaveChangesAsync(ct);
        return _mapper.Map<DocumentTemplateDto>(entity);
    }
}

public class GenerateDocumentCommandHandler : IRequestHandler<GenerateDocumentCommand, GeneratedDocumentDto>
{
    private readonly ApplicationDbContext _context; private readonly IMapper _mapper;
    public GenerateDocumentCommandHandler(ApplicationDbContext context, IMapper mapper) { _context = context; _mapper = mapper; }
    public async Task<GeneratedDocumentDto> Handle(GenerateDocumentCommand request, CancellationToken ct)
    {
        var entity = new GeneratedDocument { DocumentTemplateId = request.DocumentTemplateId, EntityType = request.EntityType, EntityId = request.EntityId, Status = DocumentGenerationStatus.Pending, OutputFormat = DocumentOutputFormat.Pdf, TokenValues = request.TokenValues };
        _context.Set<GeneratedDocument>().Add(entity); await _context.SaveChangesAsync(ct);
        return _mapper.Map<GeneratedDocumentDto>(entity);
    }
}

public class AddDocumentTokenCommandHandler : IRequestHandler<AddDocumentTokenCommand, DocumentTemplateTokenDto>
{
    private readonly ApplicationDbContext _context; private readonly IMapper _mapper;
    public AddDocumentTokenCommandHandler(ApplicationDbContext context, IMapper mapper) { _context = context; _mapper = mapper; }
    public async Task<DocumentTemplateTokenDto> Handle(AddDocumentTokenCommand request, CancellationToken ct)
    {
        var entity = new DocumentTemplateToken { DocumentTemplateId = request.DocumentTemplateId, TokenCode = request.TokenCode, DefaultValue = request.DefaultValue, IsRequired = request.IsRequired };
        _context.Set<DocumentTemplateToken>().Add(entity); await _context.SaveChangesAsync(ct);
        return _mapper.Map<DocumentTemplateTokenDto>(entity);
    }
}

public class DeleteDocumentTokenCommandHandler : IRequestHandler<DeleteDocumentTokenCommand>
{
    private readonly ApplicationDbContext _context;
    public DeleteDocumentTokenCommandHandler(ApplicationDbContext context) { _context = context; }
    public async Task Handle(DeleteDocumentTokenCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<DocumentTemplateToken>().FindAsync(new object[] { request.Id }, ct) ?? throw new NotFoundException("DocumentTemplateToken", request.Id);
        _context.Set<DocumentTemplateToken>().Remove(entity); await _context.SaveChangesAsync(ct);
    }
}

public class SaveCompanyBrandingCommandHandler : IRequestHandler<SaveCompanyBrandingCommand, CompanyBrandingDto>
{
    private readonly ApplicationDbContext _context; private readonly IMapper _mapper;
    public SaveCompanyBrandingCommandHandler(ApplicationDbContext context, IMapper mapper) { _context = context; _mapper = mapper; }
    public async Task<CompanyBrandingDto> Handle(SaveCompanyBrandingCommand request, CancellationToken ct)
    {
        var existing = await _context.Set<CompanyBranding>().FirstOrDefaultAsync(b => b.ElementType == request.ElementType, ct);
        if (existing != null) { existing.ImageUrl = request.ImageUrl; existing.Content = request.Content; existing.Configuration = request.Configuration; }
        else { existing = new CompanyBranding { ElementType = request.ElementType, ImageUrl = request.ImageUrl, Content = request.Content, Configuration = request.Configuration }; _context.Set<CompanyBranding>().Add(existing); }
        await _context.SaveChangesAsync(ct);
        return _mapper.Map<CompanyBrandingDto>(existing);
    }
}
