using HR.Application.Common.Exceptions;
using HR.Infrastructure.Persistence;
using MediatR;

namespace HR.Modules.Tasks.Commands;

public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, Unit>
{
    private readonly ApplicationDbContext _context;

    public DeleteTaskCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _context.HrTasks.FindAsync(new object[] { request.Id }, cancellationToken);
        if (task == null) throw new NotFoundException("Task", request.Id);

        task.IsDeleted = true;
        task.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
