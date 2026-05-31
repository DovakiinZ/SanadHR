using MediatR;

namespace HR.Modules.Platform.Commands.Permissions;

public record DeleteUserPermissionOverrideCommand(Guid Id) : IRequest;

public class DeleteUserPermissionOverrideCommandHandler : IRequestHandler<DeleteUserPermissionOverrideCommand>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;

    public DeleteUserPermissionOverrideCommandHandler(HR.Infrastructure.Persistence.ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteUserPermissionOverrideCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.UserPermissionOverrides.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("UserPermissionOverride", request.Id);

        _context.UserPermissionOverrides.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
