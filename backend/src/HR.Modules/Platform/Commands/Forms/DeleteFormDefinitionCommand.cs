using MediatR;

namespace HR.Modules.Platform.Commands.Forms;

public record DeleteFormDefinitionCommand(Guid Id) : IRequest;

public class DeleteFormDefinitionCommandHandler : IRequestHandler<DeleteFormDefinitionCommand>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;

    public DeleteFormDefinitionCommandHandler(HR.Infrastructure.Persistence.ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteFormDefinitionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.FormDefinitions.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("FormDefinition", request.Id);

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
