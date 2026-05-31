using HR.Domain.Enums;
using MediatR;

namespace HR.Modules.Platform.Commands.Permissions;

public record SetUserPermissionScopeCommand : IRequest<Guid>
{
    public Guid UserId { get; init; }
    public ScopeType ScopeType { get; init; }
    public Guid ScopeValue { get; init; }
}

public class SetUserPermissionScopeCommandHandler : IRequestHandler<SetUserPermissionScopeCommand, Guid>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;

    public SetUserPermissionScopeCommandHandler(HR.Infrastructure.Persistence.ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(SetUserPermissionScopeCommand request, CancellationToken cancellationToken)
    {
        var entity = new HR.Domain.Engines.Permissions.UserPermissionScope
        {
            UserId = request.UserId,
            ScopeType = request.ScopeType,
            ScopeValue = request.ScopeValue
        };

        _context.UserPermissionScopes.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
