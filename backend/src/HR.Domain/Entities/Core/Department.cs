using HR.Domain.Common;

namespace HR.Modules.Core.Entities;

public class Department : TenantEntity
{
    public string Name { get; set; } = null!;
    public string? NameAr { get; set; }
    public string? Description { get; set; }
    public Guid? ParentDepartmentId { get; set; }
    public Department? ParentDepartment { get; set; }
    public Guid? ManagerId { get; set; }
    public ICollection<Department> SubDepartments { get; set; } = new List<Department>();
}
