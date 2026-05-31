using HR.Domain.Common;

namespace HR.Domain.Engines.CompanyConfig;

public class CostCenter : TenantEntity
{
    public string Code { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public Guid? ParentCostCenterId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? BranchId { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }

    public CostCenter? ParentCostCenter { get; set; }
    public ICollection<CostCenter> ChildCostCenters { get; set; } = new List<CostCenter>();
}
