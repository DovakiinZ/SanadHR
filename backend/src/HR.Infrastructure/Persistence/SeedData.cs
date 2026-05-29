using HR.Modules.Identity.Entities;
using Microsoft.EntityFrameworkCore;

namespace HR.Infrastructure.Persistence;

public static class SeedData
{
    public static void SeedPermissions(ModelBuilder modelBuilder)
    {
        var permissions = new List<Permission>();
        var modules = new Dictionary<string, string[]>
        {
            ["Identity"] = new[] { "ViewUsers", "CreateUsers", "EditUsers", "DeleteUsers", "ViewRoles", "CreateRoles", "EditRoles", "DeleteRoles" },
            ["Employees"] = new[] { "View", "Create", "Edit", "Delete", "Export" },
            ["Tasks"] = new[] { "View", "Create", "Edit", "Delete", "Assign" },
            ["Departments"] = new[] { "View", "Create", "Edit", "Delete" },
            ["Branches"] = new[] { "View", "Create", "Edit", "Delete" },
            ["Settings"] = new[] { "View", "Edit" },
            ["Attendance"] = new[] { "View", "Create", "Edit", "Delete", "Export" },
            ["Payroll"] = new[] { "View", "Create", "Edit", "Delete", "Approve", "Export" },
            ["Expenses"] = new[] { "View", "Create", "Edit", "Delete", "Approve" },
            ["Loans"] = new[] { "View", "Create", "Edit", "Delete", "Approve" },
            ["Documents"] = new[] { "View", "Create", "Edit", "Delete" },
            ["Reports"] = new[] { "View", "Export" },
            ["Dashboards"] = new[] { "View" },
            ["Workflows"] = new[] { "View", "Create", "Edit", "Delete" },
            ["ESS"] = new[] { "View", "Create", "Edit" },
            ["Notifications"] = new[] { "View", "Create", "Edit", "Delete" }
        };

        var index = 1;
        foreach (var (module, names) in modules)
        {
            foreach (var name in names)
            {
                permissions.Add(new Permission
                {
                    Id = GenerateDeterministicGuid(module, name),
                    Module = module,
                    Name = name,
                    Description = $"{name} permission for {module}"
                });
                index++;
            }
        }

        modelBuilder.Entity<Permission>().HasData(permissions);
    }

    private static Guid GenerateDeterministicGuid(string module, string name)
    {
        // Use a deterministic GUID based on module+name for consistent seeding
        using var md5 = System.Security.Cryptography.MD5.Create();
        var bytes = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes($"{module}.{name}"));
        return new Guid(bytes);
    }
}
