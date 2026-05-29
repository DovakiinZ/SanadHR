using MediatR;

namespace HR.Modules.Employees.Commands;

public record DeleteEmployeeCommand(Guid Id) : IRequest<Unit>;
