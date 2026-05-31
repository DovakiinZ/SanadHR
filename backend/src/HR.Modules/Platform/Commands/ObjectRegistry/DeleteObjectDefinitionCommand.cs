using MediatR;

namespace HR.Modules.Platform.Commands.ObjectRegistry;

public record DeleteObjectDefinitionCommand(Guid Id) : IRequest;

public class DeleteObjectDefinitionCommandHandler : IRequestHandler<DeleteObjectDefinitionCommand>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;

    public DeleteObjectDefinitionCommandHandler(HR.Infrastructure.Persistence.ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteObjectDefinitionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.ObjectDefinitions.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("ObjectDefinition", request.Id);

        if (entity.IsSystem)
            throw new InvalidOperationException("Cannot delete system object definitions");

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
