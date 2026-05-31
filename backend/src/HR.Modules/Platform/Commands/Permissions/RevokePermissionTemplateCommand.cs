using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Commands.Permissions;

public record RevokePermissionTemplateCommand(Guid UserId, Guid PermissionTemplateId) : IRequest;

public class RevokePermissionTemplateCommandHandler : IRequestHandler<RevokePermissionTemplateCommand>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;

    public RevokePermissionTemplateCommandHandler(HR.Infrastructure.Persistence.ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(RevokePermissionTemplateCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.UserPermissionTemplates
            .FirstOrDefaultAsync(x => x.UserId == request.UserId && x.PermissionTemplateId == request.PermissionTemplateId, cancellationToken)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("UserPermissionTemplate", $"{request.UserId}/{request.PermissionTemplateId}");

        _context.UserPermissionTemplates.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
