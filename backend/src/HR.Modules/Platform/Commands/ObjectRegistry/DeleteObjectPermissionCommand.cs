using MediatR;

namespace HR.Modules.Platform.Commands.ObjectRegistry;

public record DeleteObjectPermissionCommand(Guid Id) : IRequest;

public class DeleteObjectPermissionCommandHandler : IRequestHandler<DeleteObjectPermissionCommand>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;

    public DeleteObjectPermissionCommandHandler(HR.Infrastructure.Persistence.ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteObjectPermissionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.ObjectPermissions.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("ObjectPermission", request.Id);

        _context.ObjectPermissions.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
