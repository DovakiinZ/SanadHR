using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Modules.Employees.Entities;

public class Employee : TenantEntity
{
    public string EmployeeNumber { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string? FirstNameAr { get; set; }
    public string LastName { get; set; } = null!;
    public string? LastNameAr { get; set; }
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }
    public Gender Gender { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? NationalId { get; set; }
    public string? Nationality { get; set; }
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;
    public ContractType ContractType { get; set; } = ContractType.FullTime;
    public DateTime HireDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public string? JobTitle { get; set; }
    public string? JobTitleAr { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? ManagerId { get; set; }
    public decimal BasicSalary { get; set; }
    public string? Currency { get; set; } = "SAR";
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? Iban { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? PhotoUrl { get; set; }
    public string? Notes { get; set; }
    public Guid? UserId { get; set; }
}
