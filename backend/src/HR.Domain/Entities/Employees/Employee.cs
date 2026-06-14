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
    // Governed master-data references (MasterDataItem ids) — no free text.
    public Guid? NationalityId { get; set; }
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;
    public Guid? ContractTypeId { get; set; }
    public Guid? EmploymentTypeId { get; set; }
    public DateTime HireDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public Guid? JobTitleId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? ManagerId { get; set; }

    // Contact & emergency
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }

    // Compensation & payment
    public decimal BasicSalary { get; set; }
    public string? Currency { get; set; } = "SAR";
    public Guid? PaymentMethodId { get; set; }   // master-data PaymentMethod
    public Guid? BankId { get; set; }             // master-data Bank
    public string? BankName { get; set; }         // legacy free-text (kept; BankId is canonical)
    public string? BankAccountNumber { get; set; }
    public string? Iban { get; set; }
    public string? SalaryCardNumber { get; set; }
    public string? CardProvider { get; set; }

    // Attendance & assignment
    public Guid? WorkLocationId { get; set; }     // master-data WorkLocation (attendance location)
    public Guid? LeavePolicyId { get; set; }      // master-data LeavePolicy
    public Guid? PayrollGroupId { get; set; }     // master-data PayrollGroup

    public string? Address { get; set; }
    public string? City { get; set; }
    public string? PhotoUrl { get; set; }
    public string? Notes { get; set; }
    public Guid? UserId { get; set; }

    // Per-employee allowance overrides (values for AllowanceType master-data items).
    public ICollection<EmployeeAllowance> Allowances { get; set; } = new List<EmployeeAllowance>();
    // Per-employee salary additions (AdditionType) and deductions (DeductionType).
    public ICollection<EmployeeAddition> Additions { get; set; } = new List<EmployeeAddition>();
    public ICollection<EmployeeDeduction> Deductions { get; set; } = new List<EmployeeDeduction>();
}
