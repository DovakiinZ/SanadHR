using MediatR;

namespace HR.Modules.Platform.Commands.Permissions;

public record DeleteUserPermissionScopeCommand(Guid Id) : IRequest;

public class DeleteUserPermissionScopeCommandHandler : IRequestHandler<DeleteUserPermissionScopeCommand>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;

    public DeleteUserPermissionScopeCommandHandler(HR.Infrastructure.Persistence.ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteUserPermissionScopeCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.UserPermissionScopes.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("UserPermissionScope", request.Id);

        _context.UserPermissionScopes.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
