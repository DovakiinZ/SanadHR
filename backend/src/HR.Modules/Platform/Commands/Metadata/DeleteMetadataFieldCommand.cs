using MediatR;

namespace HR.Modules.Platform.Commands.Metadata;

public record DeleteMetadataFieldCommand(Guid Id) : IRequest;

public class DeleteMetadataFieldCommandHandler : IRequestHandler<DeleteMetadataFieldCommand>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;

    public DeleteMetadataFieldCommandHandler(HR.Infrastructure.Persistence.ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteMetadataFieldCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.MetadataFields.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("MetadataField", request.Id);

        _context.MetadataFields.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
