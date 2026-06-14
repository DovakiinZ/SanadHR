using HR.Domain.Common;

namespace HR.Domain.Engines.Loans;

/// <summary>
/// A loan or salary advance. Created when a Loan/Salary-Advance request is approved. Generates a
/// monthly installment (deduction) schedule that payroll consumes. LoanType references master-data.
/// </summary>
public class Loan : TenantEntity
{
    public Guid EmployeeId { get; set; }
    public Guid? LoanTypeId { get; set; }
    public string Kind { get; set; } = "Loan";        // Loan | Advance
    public decimal Principal { get; set; }
    public int InstallmentMonths { get; set; } = 1;
    public decimal MonthlyInstallment { get; set; }
    public string Status { get; set; } = "Active";     // Active | Settled | Cancelled
    public Guid? RequestInstanceId { get; set; }
    public DateTime StartDate { get; set; } = DateTime.UtcNow;

    public ICollection<LoanInstallment> Installments { get; set; } = new List<LoanInstallment>();
}
