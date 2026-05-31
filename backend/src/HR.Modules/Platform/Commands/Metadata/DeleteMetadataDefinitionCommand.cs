using HR.Application.Common.Exceptions;
using HR.Infrastructure.Persistence;
using MediatR;

namespace HR.Modules.Platform.Commands.Metadata;

public record DeleteMetadataDefinitionCommand(Guid Id) : IRequest;

public class DeleteMetadataDefinitionCommandHandler : IRequestHandler<DeleteMetadataDefinitionCommand>
{
    private readonly ApplicationDbContext _context;

    public DeleteMetadataDefinitionCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteMetadataDefinitionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.MetadataDefinitions.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new NotFoundException("MetadataDefinition", request.Id);

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
