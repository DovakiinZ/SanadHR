using HR.Application.Common.Interfaces;
using HR.Domain.Common;
using HR.Domain.Engines.Audit;
using HR.Domain.Engines.Automation;
using HR.Domain.Engines.Dashboards;
using HR.Domain.Engines.Forms;
using HR.Domain.Engines.Metadata;
using HR.Domain.Engines.ObjectRegistry;
using HR.Domain.Engines.Permissions;
using HR.Domain.Engines.Timeline;
using HR.Domain.Engines.Tokens;
using HR.Domain.Engines.Workflows;
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

    // ===== Engine DbSets =====

    // Metadata Engine
    public DbSet<MetadataDefinition> MetadataDefinitions => Set<MetadataDefinition>();
    public DbSet<MetadataField> MetadataFields => Set<MetadataField>();
    public DbSet<MetadataOption> MetadataOptions => Set<MetadataOption>();
    public DbSet<MetadataValue> MetadataValues => Set<MetadataValue>();

    // Object Registry Engine
    public DbSet<ObjectDefinition> ObjectDefinitions => Set<ObjectDefinition>();
    public DbSet<ObjectField> ObjectFields => Set<ObjectField>();
    public DbSet<ObjectRelationship> ObjectRelationships => Set<ObjectRelationship>();
    public DbSet<ObjectPermission> ObjectPermissions => Set<ObjectPermission>();

    // Permission Engine
    public DbSet<PermissionTemplate> PermissionTemplates => Set<PermissionTemplate>();
    public DbSet<PermissionTemplateItem> PermissionTemplateItems => Set<PermissionTemplateItem>();
    public DbSet<UserPermissionOverride> UserPermissionOverrides => Set<UserPermissionOverride>();
    public DbSet<UserPermissionScope> UserPermissionScopes => Set<UserPermissionScope>();
    public DbSet<UserPermissionTemplate> UserPermissionTemplates => Set<UserPermissionTemplate>();

    // Forms Engine
    public DbSet<FormDefinition> FormDefinitions => Set<FormDefinition>();
    public DbSet<FormField> FormFields => Set<FormField>();
    public DbSet<FormSubmission> FormSubmissions => Set<FormSubmission>();
    public DbSet<FormSubmissionValue> FormSubmissionValues => Set<FormSubmissionValue>();

    // Workflow Engine
    public DbSet<WorkflowDefinition> WorkflowDefinitions => Set<WorkflowDefinition>();
    public DbSet<WorkflowVersion> WorkflowVersions => Set<WorkflowVersion>();
    public DbSet<WorkflowNode> WorkflowNodes => Set<WorkflowNode>();
    public DbSet<WorkflowEdge> WorkflowEdges => Set<WorkflowEdge>();
    public DbSet<WorkflowCondition> WorkflowConditions => Set<WorkflowCondition>();
    public DbSet<WorkflowApproverRule> WorkflowApproverRules => Set<WorkflowApproverRule>();
    public DbSet<WorkflowInstance> WorkflowInstances => Set<WorkflowInstance>();
    public DbSet<WorkflowInstanceStep> WorkflowInstanceSteps => Set<WorkflowInstanceStep>();

    // Automation Engine
    public DbSet<AutomationRule> AutomationRules => Set<AutomationRule>();
    public DbSet<AutomationTrigger> AutomationTriggers => Set<AutomationTrigger>();
    public DbSet<AutomationCondition> AutomationConditions => Set<AutomationCondition>();
    public DbSet<AutomationAction> AutomationActions => Set<AutomationAction>();
    public DbSet<AutomationExecutionLog> AutomationExecutionLogs => Set<AutomationExecutionLog>();

    // Audit Engine
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();
    public DbSet<AuditConfiguration> AuditConfigurations => Set<AuditConfiguration>();

    // Timeline Engine
    public DbSet<TimelineEvent> TimelineEvents => Set<TimelineEvent>();
    public DbSet<TimelineSubscription> TimelineSubscriptions => Set<TimelineSubscription>();

    // Token Engine
    public DbSet<TokenDefinition> TokenDefinitions => Set<TokenDefinition>();
    public DbSet<TokenCategory> TokenCategories => Set<TokenCategory>();

    // Dashboard Engine
    public DbSet<DashboardDefinition> DashboardDefinitions => Set<DashboardDefinition>();
    public DbSet<DashboardWidget> DashboardWidgets => Set<DashboardWidget>();
    public DbSet<WidgetFilter> WidgetFilters => Set<WidgetFilter>();
    public DbSet<WidgetLayout> WidgetLayouts => Set<WidgetLayout>();

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
