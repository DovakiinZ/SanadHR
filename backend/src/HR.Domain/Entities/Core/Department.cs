using HR.Domain.Common;

namespace HR.Modules.Core.Entities;

public class Department : TenantEntity
{
    public string Name { get; set; } = null!;
    public string? NameAr { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
    public Guid? ParentDepartmentId { get; set; }
    public Department? ParentDepartment { get; set; }
    public Guid? ManagerId { get; set; }
    public Guid? DeputyManagerId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? CostCenterId { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Department> SubDepartments { get; set; } = new List<Department>();
}
