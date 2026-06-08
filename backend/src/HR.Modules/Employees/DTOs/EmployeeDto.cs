namespace HR.Modules.Employees.DTOs;

public class EmployeeDto
{
    public Guid Id { get; set; }
    public string EmployeeNumber { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string? FirstNameAr { get; set; }
    public string LastName { get; set; } = null!;
    public string? LastNameAr { get; set; }
    public string FullName => $"{FirstName} {LastName}";
    public string? FullNameAr => FirstNameAr != null && LastNameAr != null ? $"{FirstNameAr} {LastNameAr}" : null;
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }
    public string Gender { get; set; } = null!;
    public string GenderAr { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }
    public string? NationalId { get; set; }

    // Nationality (master data reference)
    public Guid? NationalityId { get; set; }
    public string? Nationality { get; set; }
    public string? NationalityAr { get; set; }

    public string Status { get; set; } = null!;
    public string StatusAr { get; set; } = null!;

    // Contract type (master data reference)
    public Guid? ContractTypeId { get; set; }
    public string? ContractType { get; set; }
    public string? ContractTypeAr { get; set; }

    public DateTime HireDate { get; set; }
    public DateTime? TerminationDate { get; set; }

    // Job title (master data reference)
    public Guid? JobTitleId { get; set; }
    public string? JobTitle { get; set; }
    public string? JobTitleAr { get; set; }

    // Organization assignment (Core entities)
    public Guid? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public Guid? BranchId { get; set; }
    public string? BranchName { get; set; }
    public Guid? ManagerId { get; set; }
    public string? ManagerName { get; set; }

    public decimal BasicSalary { get; set; }
    public string? Currency { get; set; }
    public string? PhotoUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}
