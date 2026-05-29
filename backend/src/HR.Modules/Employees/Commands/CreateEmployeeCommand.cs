using HR.Application.Common.Models;
using HR.Domain.Enums;
using HR.Modules.Employees.DTOs;
using MediatR;

namespace HR.Modules.Employees.Commands;

public record CreateEmployeeCommand : IRequest<EmployeeDto>
{
    public string EmployeeNumber { get; init; } = null!;
    public string FirstName { get; init; } = null!;
    public string? FirstNameAr { get; init; }
    public string LastName { get; init; } = null!;
    public string? LastNameAr { get; init; }
    public string Email { get; init; } = null!;
    public string? Phone { get; init; }
    public Gender Gender { get; init; }
    public DateTime DateOfBirth { get; init; }
    public string? NationalId { get; init; }
    public string? Nationality { get; init; }
    public ContractType ContractType { get; init; }
    public DateTime HireDate { get; init; }
    public string? JobTitle { get; init; }
    public string? JobTitleAr { get; init; }
    public Guid? DepartmentId { get; init; }
    public Guid? BranchId { get; init; }
    public Guid? ManagerId { get; init; }
    public decimal BasicSalary { get; init; }
    public string? Currency { get; init; }
}
