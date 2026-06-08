using HR.Domain.Enums;
using HR.Modules.Employees.DTOs;
using MediatR;

namespace HR.Modules.Employees.Commands;

public record UpdateEmployeeCommand : IRequest<EmployeeDto>
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = null!;
    public string? FirstNameAr { get; init; }
    public string LastName { get; init; } = null!;
    public string? LastNameAr { get; init; }
    public string Email { get; init; } = null!;
    public string? Phone { get; init; }
    public Gender Gender { get; init; }
    public DateTime DateOfBirth { get; init; }
    public string? NationalId { get; init; }
    public Guid? NationalityId { get; init; }
    public EmployeeStatus Status { get; init; }
    public Guid? ContractTypeId { get; init; }
    public Guid? JobTitleId { get; init; }
    public Guid? DepartmentId { get; init; }
    public Guid? BranchId { get; init; }
    public Guid? ManagerId { get; init; }
    public decimal BasicSalary { get; init; }
}
