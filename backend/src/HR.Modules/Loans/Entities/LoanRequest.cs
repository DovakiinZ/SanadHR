using HR.Domain.Common;

namespace HR.Modules.Loans.Entities;

// TODO: Implement loan request entity
public class LoanRequest : TenantEntity
{
    public Guid EmployeeId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "SAR";
    public int InstallmentMonths { get; set; }
}
