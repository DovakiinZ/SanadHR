using HR.Modules.Employees.DTOs;
using MediatR;

namespace HR.Modules.Employees.Queries;

public record GetEmployeeByIdQuery(Guid Id) : IRequest<EmployeeDto>;
