using HR.Domain.Enums;
using HR.Modules.Platform.DTOs.Permissions;
using MediatR;

namespace HR.Modules.Platform.Commands.Permissions;

public record AddPermissionTemplateItemCommand : IRequest<PermissionTemplateItemDto>
{
    public Guid PermissionTemplateId { get; init; }
    public string PermissionCode { get; init; } = null!;
    public ScopeType Scope { get; init; }
}

public class AddPermissionTemplateItemCommandHandler : IRequestHandler<AddPermissionTemplateItemCommand, PermissionTemplateItemDto>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;
    private readonly AutoMapper.IMapper _mapper;

    public AddPermissionTemplateItemCommandHandler(HR.Infrastructure.Persistence.ApplicationDbContext context, AutoMapper.IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PermissionTemplateItemDto> Handle(AddPermissionTemplateItemCommand request, CancellationToken cancellationToken)
    {
        _ = await _context.PermissionTemplates.FindAsync(new object[] { request.PermissionTemplateId }, cancellationToken)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("PermissionTemplate", request.PermissionTemplateId);

        var entity = new HR.Domain.Engines.Permissions.PermissionTemplateItem
        {
            PermissionTemplateId = request.PermissionTemplateId,
            PermissionCode = request.PermissionCode,
            Scope = request.Scope
        };

        _context.PermissionTemplateItems.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<PermissionTemplateItemDto>(entity);
    }
}
