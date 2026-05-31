using HR.Domain.Common;

namespace HR.Domain.Engines.CompanyConfig;

public class Position : TenantEntity
{
    public string Code { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public Guid? DepartmentId { get; set; }
    public Guid? ParentPositionId { get; set; }
    public string? JobDescription { get; set; }
    public int? MinGrade { get; set; }
    public int? MaxGrade { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }

    public Position? ParentPosition { get; set; }
    public ICollection<Position> ChildPositions { get; set; } = new List<Position>();
}
