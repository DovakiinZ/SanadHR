using HR.Domain.Common;

namespace HR.Modules.Payroll.Entities;

// TODO: Implement payroll run entity
public class PayrollRun : TenantEntity
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string Status { get; set; } = "Draft";
    public decimal TotalAmount { get; set; }
}
