using MediatR;

namespace HR.Modules.Platform.Commands.Forms;

public record DeleteFormFieldCommand(Guid Id) : IRequest;

public class DeleteFormFieldCommandHandler : IRequestHandler<DeleteFormFieldCommand>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;

    public DeleteFormFieldCommandHandler(HR.Infrastructure.Persistence.ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteFormFieldCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.FormFields.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("FormField", request.Id);

        _context.FormFields.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
