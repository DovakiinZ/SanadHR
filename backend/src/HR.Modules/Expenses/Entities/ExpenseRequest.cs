using HR.Domain.Common;

namespace HR.Modules.Expenses.Entities;

// TODO: Implement expense request entity
public class ExpenseRequest : TenantEntity
{
    public Guid EmployeeId { get; set; }
    public string Description { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "SAR";
}
