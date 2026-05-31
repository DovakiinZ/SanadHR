using HR.Domain.Enums;
using HR.Modules.Platform.DTOs.Permissions;
using MediatR;

namespace HR.Modules.Platform.Commands.Permissions;

public record UpdatePermissionTemplateItemCommand : IRequest<PermissionTemplateItemDto>
{
    public Guid Id { get; init; }
    public ScopeType Scope { get; init; }
}

public class UpdatePermissionTemplateItemCommandHandler : IRequestHandler<UpdatePermissionTemplateItemCommand, PermissionTemplateItemDto>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;
    private readonly AutoMapper.IMapper _mapper;

    public UpdatePermissionTemplateItemCommandHandler(HR.Infrastructure.Persistence.ApplicationDbContext context, AutoMapper.IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PermissionTemplateItemDto> Handle(UpdatePermissionTemplateItemCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.PermissionTemplateItems.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("PermissionTemplateItem", request.Id);

        entity.Scope = request.Scope;

        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<PermissionTemplateItemDto>(entity);
    }
}
