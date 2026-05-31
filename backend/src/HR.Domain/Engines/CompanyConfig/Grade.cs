using HR.Domain.Common;

namespace HR.Domain.Engines.CompanyConfig;

public class Grade : TenantEntity
{
    public string Code { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public int Level { get; set; }
    public decimal? MinSalary { get; set; }
    public decimal? MaxSalary { get; set; }
    public string? Benefits { get; set; } // JSONB
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}
