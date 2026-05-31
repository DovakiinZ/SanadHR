using HR.Domain.Engines.OrgGraph;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HR.Infrastructure.Persistence.Configurations.Engines;

public class OrgNodeConfiguration : IEntityTypeConfiguration<OrgNode>
{
    public void Configure(EntityTypeBuilder<OrgNode> builder)
    {
        builder.ToTable("engine_org_nodes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.NodeType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.NameEn).HasMaxLength(200).IsRequired();
        builder.Property(x => x.NameAr).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Metadata).HasColumnType("jsonb");
        builder.HasOne(x => x.ParentNode).WithMany(x => x.ChildNodes).HasForeignKey(x => x.ParentNodeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(x => x.OutgoingEdges).WithOne(x => x.SourceNode).HasForeignKey(x => x.SourceNodeId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.IncomingEdges).WithOne(x => x.TargetNode).HasForeignKey(x => x.TargetNodeId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.TenantId, x.NodeType, x.EntityId });
    }
}

public class OrgEdgeConfiguration : IEntityTypeConfiguration<OrgEdge>
{
    public void Configure(EntityTypeBuilder<OrgEdge> builder)
    {
        builder.ToTable("engine_org_edges");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RelationType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Label).HasMaxLength(200);
    }
}

public class OrgGraphLayoutConfiguration : IEntityTypeConfiguration<OrgGraphLayout>
{
    public void Configure(EntityTypeBuilder<OrgGraphLayout> builder)
    {
        builder.ToTable("engine_org_graph_layouts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(100).IsRequired();
        builder.Property(x => x.NameEn).HasMaxLength(200).IsRequired();
        builder.Property(x => x.NameAr).HasMaxLength(200).IsRequired();
        builder.Property(x => x.GraphType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.LayoutData).HasColumnType("jsonb").IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
    }
}

public class EmployeeReportingLineConfiguration : IEntityTypeConfiguration<EmployeeReportingLine>
{
    public void Configure(EntityTypeBuilder<EmployeeReportingLine> builder)
    {
        builder.ToTable("engine_employee_reporting_lines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ReportingType).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.EmployeeId, x.ManagerId, x.ReportingType }).IsUnique();
    }
}
