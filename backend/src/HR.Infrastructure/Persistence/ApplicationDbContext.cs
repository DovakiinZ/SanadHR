using HR.Application.Common.Interfaces;
using HR.Domain.Common;
using HR.Modules.Core.Entities;
using HR.Modules.Employees.Entities;
using HR.Modules.Identity.Entities;
using HR.Modules.Settings.Entities;
using HR.Modules.Tasks.Entities;
using HR.Modules.Tenancy.Entities;
using Microsoft.EntityFrameworkCore;

namespace HR.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentUserService _currentUser;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentUserService currentUser)
        : base(options)
    {
        _currentUser = currentUser;
    }

    // Tenancy
    public DbSet<Tenant> Tenants => Set<Tenant>();

    // Identity
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserPermission> UserPermissions => Set<UserPermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // Core
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Branch> Branch => Set<Branch>();

    // Employees
    public DbSet<Employee> Employees => Set<Employee>();

    // Tasks
    public DbSet<HrTask> HrTasks => Set<HrTask>();
    public DbSet<HrTaskChecklist> HrTaskChecklists => Set<HrTaskChecklist>();
    public DbSet<HrTaskComment> HrTaskComments => Set<HrTaskComment>();
    public DbSet<HrTaskActivity> HrTaskActivities => Set<HrTaskActivity>();

    // Settings
    public DbSet<CompanySettings> CompanySettings => Set<CompanySettings>();

    // Audit
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Seed permissions
        SeedData.SeedPermissions(modelBuilder);

        // Global query filters for tenant isolation and soft delete
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(TenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(
                    CreateTenantFilter(entityType.ClrType));
            }
            else if (typeof(AuditableEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(
                    CreateSoftDeleteFilter(entityType.ClrType));
            }
        }
    }

    private LambdaExpression CreateTenantFilter(Type entityType)
    {
        var parameter = System.Linq.Expressions.Expression.Parameter(entityType, "e");
        var tenantIdProp = System.Linq.Expressions.Expression.Property(parameter, nameof(TenantEntity.TenantId));
        var currentTenantId = System.Linq.Expressions.Expression.Property(
            System.Linq.Expressions.Expression.Constant(this), nameof(_currentUser.TenantId));
        // Actually we need to access _currentUser.TenantId
        var currentUserField = System.Linq.Expressions.Expression.Field(
            System.Linq.Expressions.Expression.Constant(this), "_currentUser");
        var tenantIdValue = System.Linq.Expressions.Expression.Property(currentUserField, nameof(ICurrentUserService.TenantId));

        var isDeletedProp = System.Linq.Expressions.Expression.Property(parameter, nameof(AuditableEntity.IsDeleted));

        var tenantFilter = System.Linq.Expressions.Expression.Equal(tenantIdProp, tenantIdValue);
        var softDeleteFilter = System.Linq.Expressions.Expression.Equal(
            isDeletedProp, System.Linq.Expressions.Expression.Constant(false));
        var combined = System.Linq.Expressions.Expression.AndAlso(tenantFilter, softDeleteFilter);

        return System.Linq.Expressions.Expression.Lambda(combined, parameter);
    }

    private LambdaExpression CreateSoftDeleteFilter(Type entityType)
    {
        var parameter = System.Linq.Expressions.Expression.Parameter(entityType, "e");
        var isDeletedProp = System.Linq.Expressions.Expression.Property(parameter, nameof(AuditableEntity.IsDeleted));
        var filter = System.Linq.Expressions.Expression.Equal(
            isDeletedProp, System.Linq.Expressions.Expression.Constant(false));
        return System.Linq.Expressions.Expression.Lambda(filter, parameter);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.CreatedBy = _currentUser.Email;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedBy = _currentUser.Email;
                    break;
            }

            if (entry.Entity is TenantEntity tenantEntity && entry.State == EntityState.Added)
            {
                if (tenantEntity.TenantId == Guid.Empty)
                    tenantEntity.TenantId = _currentUser.TenantId;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
