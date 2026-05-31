using MediatR;

namespace HR.Modules.Platform.Commands.Metadata;

public record DeleteMetadataOptionCommand(Guid Id) : IRequest;

public class DeleteMetadataOptionCommandHandler : IRequestHandler<DeleteMetadataOptionCommand>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;

    public DeleteMetadataOptionCommandHandler(HR.Infrastructure.Persistence.ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteMetadataOptionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.MetadataOptions.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("MetadataOption", request.Id);

        _context.MetadataOptions.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
