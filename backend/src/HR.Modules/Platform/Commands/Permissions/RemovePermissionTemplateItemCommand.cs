using MediatR;

namespace HR.Modules.Platform.Commands.Permissions;

public record RemovePermissionTemplateItemCommand(Guid Id) : IRequest;

public class RemovePermissionTemplateItemCommandHandler : IRequestHandler<RemovePermissionTemplateItemCommand>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;

    public RemovePermissionTemplateItemCommandHandler(HR.Infrastructure.Persistence.ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(RemovePermissionTemplateItemCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.PermissionTemplateItems.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("PermissionTemplateItem", request.Id);

        _context.PermissionTemplateItems.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
