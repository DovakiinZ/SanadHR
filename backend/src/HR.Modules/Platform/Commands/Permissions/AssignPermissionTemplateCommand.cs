using HR.Modules.Platform.DTOs.Permissions;
using MediatR;

namespace HR.Modules.Platform.Commands.Permissions;

public record AssignPermissionTemplateCommand : IRequest<UserPermissionTemplateDto>
{
    public Guid UserId { get; init; }
    public Guid PermissionTemplateId { get; init; }
}

public class AssignPermissionTemplateCommandHandler : IRequestHandler<AssignPermissionTemplateCommand, UserPermissionTemplateDto>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;
    private readonly AutoMapper.IMapper _mapper;

    public AssignPermissionTemplateCommandHandler(HR.Infrastructure.Persistence.ApplicationDbContext context, AutoMapper.IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<UserPermissionTemplateDto> Handle(AssignPermissionTemplateCommand request, CancellationToken cancellationToken)
    {
        _ = await _context.PermissionTemplates.FindAsync(new object[] { request.PermissionTemplateId }, cancellationToken)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("PermissionTemplate", request.PermissionTemplateId);

        var entity = new HR.Domain.Engines.Permissions.UserPermissionTemplate
        {
            UserId = request.UserId,
            PermissionTemplateId = request.PermissionTemplateId,
            AssignedAt = DateTime.UtcNow
        };

        _context.UserPermissionTemplates.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UserPermissionTemplateDto>(entity);
    }
}
