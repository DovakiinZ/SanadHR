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

    /// <summary>When false (default), employees who have left the organization (Terminated/Resigned)
    /// are hidden. The toggle on the directory flips this to surface former employees in search.</summary>
    public bool IncludeTerminated { get; init; }
}
