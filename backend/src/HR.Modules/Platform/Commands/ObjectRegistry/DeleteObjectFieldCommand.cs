using MediatR;

namespace HR.Modules.Platform.Commands.ObjectRegistry;

public record DeleteObjectFieldCommand(Guid Id) : IRequest;

public class DeleteObjectFieldCommandHandler : IRequestHandler<DeleteObjectFieldCommand>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;

    public DeleteObjectFieldCommandHandler(HR.Infrastructure.Persistence.ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteObjectFieldCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.ObjectFields.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("ObjectField", request.Id);

        _context.ObjectFields.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
