using HR.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Commands.Permissions;

public record SetUserPermissionOverrideCommand : IRequest<Guid>
{
    public Guid UserId { get; init; }
    public string PermissionCode { get; init; } = null!;
    public bool IsGranted { get; init; }
    public ScopeType Scope { get; init; }
}

public class SetUserPermissionOverrideCommandHandler : IRequestHandler<SetUserPermissionOverrideCommand, Guid>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;

    public SetUserPermissionOverrideCommandHandler(HR.Infrastructure.Persistence.ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(SetUserPermissionOverrideCommand request, CancellationToken cancellationToken)
    {
        // Upsert: update existing or create new
        var existing = await _context.UserPermissionOverrides
            .FirstOrDefaultAsync(x => x.UserId == request.UserId && x.PermissionCode == request.PermissionCode, cancellationToken);

        if (existing != null)
        {
            existing.IsGranted = request.IsGranted;
            existing.Scope = request.Scope;
        }
        else
        {
            existing = new HR.Domain.Engines.Permissions.UserPermissionOverride
            {
                UserId = request.UserId,
                PermissionCode = request.PermissionCode,
                IsGranted = request.IsGranted,
                Scope = request.Scope
            };
            _context.UserPermissionOverrides.Add(existing);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return existing.Id;
    }
}
