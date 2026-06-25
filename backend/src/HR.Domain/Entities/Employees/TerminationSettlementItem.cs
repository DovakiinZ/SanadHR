using HR.Domain.Common;

namespace HR.Modules.Employees.Entities;

/// <summary>One line of a <see cref="TerminationSettlement"/> breakdown, tagged with the Saudi Labor
/// Law article it derives from (e.g. "Art. 84", "Art. 77").</summary>
public class TerminationSettlementItem : TenantEntity
{
    public Guid TerminationSettlementId { get; set; }

    public string LabelEn { get; set; } = null!;
    public string LabelAr { get; set; } = null!;
    public string ArticleRef { get; set; } = null!;
    public decimal Amount { get; set; }
}
