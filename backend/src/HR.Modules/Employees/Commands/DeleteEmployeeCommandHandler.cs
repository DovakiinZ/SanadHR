using HR.Application.Common.Exceptions;
using HR.Application.Engines.Audit;
using HR.Infrastructure.Persistence;
using MediatR;

namespace HR.Modules.Employees.Commands;

public class DeleteEmployeeCommandHandler : IRequestHandler<DeleteEmployeeCommand, Unit>
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditEngine _audit;

    public DeleteEmployeeCommandHandler(ApplicationDbContext context, IAuditEngine audit)
    {
        _context = context;
        _audit = audit;
    }

    public async Task<Unit> Handle(DeleteEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await _context.Employees.FindAsync(new object[] { request.Id }, cancellationToken);
        if (employee == null) throw new NotFoundException("Employee", request.Id);

        employee.IsDeleted = true;
        employee.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        await _audit.LogChange("Employee", employee.Id, "Deleted",
            new { employee.EmployeeNumber, employee.FirstName, employee.LastName }, null, cancellationToken);

        return Unit.Value;
    }
}
