using MediatR;

namespace HR.Modules.Platform.Commands.ObjectRegistry;

public record DeleteObjectRelationshipCommand(Guid Id) : IRequest;

public class DeleteObjectRelationshipCommandHandler : IRequestHandler<DeleteObjectRelationshipCommand>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;

    public DeleteObjectRelationshipCommandHandler(HR.Infrastructure.Persistence.ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteObjectRelationshipCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.ObjectRelationships.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("ObjectRelationship", request.Id);

        _context.ObjectRelationships.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
