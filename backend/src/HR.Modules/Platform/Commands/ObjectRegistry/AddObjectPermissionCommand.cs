using HR.Domain.Enums;
using MediatR;

namespace HR.Modules.Platform.Commands.ObjectRegistry;

public record AddObjectPermissionCommand : IRequest<Guid>
{
    public Guid ObjectDefinitionId { get; init; }
    public PermissionType PermissionType { get; init; }
    public string PermissionCode { get; init; } = null!;
}

public class AddObjectPermissionCommandHandler : IRequestHandler<AddObjectPermissionCommand, Guid>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;

    public AddObjectPermissionCommandHandler(HR.Infrastructure.Persistence.ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(AddObjectPermissionCommand request, CancellationToken cancellationToken)
    {
        _ = await _context.ObjectDefinitions.FindAsync(new object[] { request.ObjectDefinitionId }, cancellationToken)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("ObjectDefinition", request.ObjectDefinitionId);

        var entity = new HR.Domain.Engines.ObjectRegistry.ObjectPermission
        {
            ObjectDefinitionId = request.ObjectDefinitionId,
            PermissionType = request.PermissionType,
            PermissionCode = request.PermissionCode
        };

        _context.ObjectPermissions.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
