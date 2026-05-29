using HR.Application.Common.Models;
using HR.Modules.Employees.DTOs;
using MediatR;

namespace HR.Modules.Employees.Queries;

public record GetEmployeesQuery : IRequest<PaginatedList<EmployeeDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Search { get; init; }
    public string? Status { get; init; }
    public Guid? DepartmentId { get; init; }
    public Guid? BranchId { get; init; }
}
