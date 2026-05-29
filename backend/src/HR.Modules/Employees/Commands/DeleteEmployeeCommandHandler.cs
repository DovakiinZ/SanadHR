using HR.Application.Common.Exceptions;
using HR.Infrastructure.Persistence;
using MediatR;

namespace HR.Modules.Employees.Commands;

public class DeleteEmployeeCommandHandler : IRequestHandler<DeleteEmployeeCommand, Unit>
{
    private readonly ApplicationDbContext _context;

    public DeleteEmployeeCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await _context.Employees.FindAsync(new object[] { request.Id }, cancellationToken);
        if (employee == null) throw new NotFoundException("Employee", request.Id);

        employee.IsDeleted = true;
        employee.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
