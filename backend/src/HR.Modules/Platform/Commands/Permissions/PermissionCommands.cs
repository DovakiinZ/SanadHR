using AutoMapper;
using HR.Application.Common.Exceptions;
using HR.Domain.Engines.Permissions;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using HR.Modules.Platform.DTOs.Permissions;
using MediatR;

namespace HR.Modules.Platform.Commands.Permissions;

public record CreatePermissionTemplateCommand : IRequest<PermissionTemplateDto>
{
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public string? Description { get; init; }
    public List<PermissionTemplateItemInput> Items { get; init; } = new();
}

public class PermissionTemplateItemInput
{
    public string PermissionCode { get; set; } = null!;
    public ScopeType Scope { get; set; }
}

public record UpdatePermissionTemplateCommand : IRequest<PermissionTemplateDto>
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public string? Description { get; init; }
}

public record DeletePermissionTemplateCommand(Guid Id) : IRequest;

// Handlers

public class CreatePermissionTemplateCommandHandler : IRequestHandler<CreatePermissionTemplateCommand, PermissionTemplateDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreatePermissionTemplateCommandHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PermissionTemplateDto> Handle(CreatePermissionTemplateCommand request, CancellationToken cancellationToken)
    {
        var entity = new PermissionTemplate
        {
            NameEn = request.NameEn,
            NameAr = request.NameAr,
            Description = request.Description
        };

        foreach (var item in request.Items)
        {
            entity.Items.Add(new PermissionTemplateItem
            {
                PermissionCode = item.PermissionCode,
                Scope = item.Scope
            });
        }

        _context.PermissionTemplates.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<PermissionTemplateDto>(entity);
    }
}

public class UpdatePermissionTemplateCommandHandler : IRequestHandler<UpdatePermissionTemplateCommand, PermissionTemplateDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdatePermissionTemplateCommandHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PermissionTemplateDto> Handle(UpdatePermissionTemplateCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.PermissionTemplates.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new NotFoundException("PermissionTemplate", request.Id);

        entity.NameEn = request.NameEn;
        entity.NameAr = request.NameAr;
        entity.Description = request.Description;

        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<PermissionTemplateDto>(entity);
    }
}

public class DeletePermissionTemplateCommandHandler : IRequestHandler<DeletePermissionTemplateCommand>
{
    private readonly ApplicationDbContext _context;

    public DeletePermissionTemplateCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeletePermissionTemplateCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.PermissionTemplates.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new NotFoundException("PermissionTemplate", request.Id);

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
