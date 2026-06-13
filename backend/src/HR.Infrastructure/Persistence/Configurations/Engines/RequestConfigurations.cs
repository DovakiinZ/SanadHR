using HR.Domain.Engines.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HR.Infrastructure.Persistence.Configurations.Engines;

public class RequestTypeConfiguration : IEntityTypeConfiguration<RequestType>
{
    public void Configure(EntityTypeBuilder<RequestType> builder)
    {
        builder.ToTable("engine_request_types");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(100).IsRequired();
        builder.Property(x => x.NameEn).HasMaxLength(200).IsRequired();
        builder.Property(x => x.NameAr).HasMaxLength(200).IsRequired();
        builder.Property(x => x.DescriptionEn).HasMaxLength(1000);
        builder.Property(x => x.DescriptionAr).HasMaxLength(1000);
        builder.Property(x => x.Icon).HasMaxLength(100);
        builder.Property(x => x.Color).HasMaxLength(20);
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        builder.HasOne(x => x.ImpactMapping).WithOne(x => x.RequestType).HasForeignKey<RequestImpactMapping>(x => x.RequestTypeId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Permissions).WithOne(x => x.RequestType).HasForeignKey(x => x.RequestTypeId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class RequestImpactMappingConfiguration : IEntityTypeConfiguration<RequestImpactMapping>
{
    public void Configure(EntityTypeBuilder<RequestImpactMapping> builder)
    {
        builder.ToTable("engine_request_impact_mappings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ExtraJson).HasColumnType("jsonb");
        builder.HasIndex(x => x.RequestTypeId).IsUnique();
    }
}

public class RequestTemplateMappingConfiguration : IEntityTypeConfiguration<RequestTemplateMapping>
{
    public void Configure(EntityTypeBuilder<RequestTemplateMapping> builder)
    {
        builder.ToTable("engine_request_template_mappings");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.TenantId, x.RequestTypeId, x.TriggerEvent });
        builder.HasOne(x => x.RequestType).WithMany().HasForeignKey(x => x.RequestTypeId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<HR.Domain.Engines.Documents.DocumentTemplate>().WithMany().HasForeignKey(x => x.DocumentTemplateId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class RequestPermissionConfiguration : IEntityTypeConfiguration<RequestPermission>
{
    public void Configure(EntityTypeBuilder<RequestPermission> builder)
    {
        builder.ToTable("engine_request_permissions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PermissionCode).HasMaxLength(150);
    }
}

public class RequestInstanceConfiguration : IEntityTypeConfiguration<RequestInstance>
{
    public void Configure(EntityTypeBuilder<RequestInstance> builder)
    {
        builder.ToTable("engine_request_instances");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RequestNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.DecisionNote).HasMaxLength(2000);
        builder.Property(x => x.DaysCount).HasColumnType("numeric(7,2)");
        builder.HasIndex(x => new { x.TenantId, x.RequestNumber }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.EmployeeId });
        builder.HasIndex(x => new { x.TenantId, x.Status });
        builder.HasOne(x => x.RequestType).WithMany().HasForeignKey(x => x.RequestTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(x => x.Approvals).WithOne(x => x.RequestInstance).HasForeignKey(x => x.RequestInstanceId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.History).WithOne(x => x.RequestInstance).HasForeignKey(x => x.RequestInstanceId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class RequestApprovalConfiguration : IEntityTypeConfiguration<RequestApproval>
{
    public void Configure(EntityTypeBuilder<RequestApproval> builder)
    {
        builder.ToTable("engine_request_approvals");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.StepNameEn).HasMaxLength(200).IsRequired();
        builder.Property(x => x.StepNameAr).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Comment).HasMaxLength(2000);
        builder.HasIndex(x => x.AssignedToUserId);
        builder.HasIndex(x => new { x.RequestInstanceId, x.StepOrder });
    }
}

public class RequestStatusHistoryConfiguration : IEntityTypeConfiguration<RequestStatusHistory>
{
    public void Configure(EntityTypeBuilder<RequestStatusHistory> builder)
    {
        builder.ToTable("engine_request_status_history");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.NoteEn).HasMaxLength(1000);
        builder.Property(x => x.NoteAr).HasMaxLength(1000);
        builder.HasIndex(x => x.RequestInstanceId);
    }
}
