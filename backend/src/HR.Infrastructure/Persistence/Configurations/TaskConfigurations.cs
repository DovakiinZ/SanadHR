using HR.Modules.Tasks.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HR.Infrastructure.Persistence.Configurations;

public class HrTaskConfiguration : IEntityTypeConfiguration<HrTask>
{
    public void Configure(EntityTypeBuilder<HrTask> builder)
    {
        builder.ToTable("hr_tasks");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Category).HasMaxLength(100);
        builder.Property(x => x.Tags).HasColumnType("jsonb");
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => x.AssigneeId);
        builder.HasIndex(x => x.Status);

        builder.HasMany(x => x.Checklists).WithOne(x => x.Task).HasForeignKey(x => x.TaskId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Comments).WithOne(x => x.Task).HasForeignKey(x => x.TaskId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Activities).WithOne(x => x.Task).HasForeignKey(x => x.TaskId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class HrTaskChecklistConfiguration : IEntityTypeConfiguration<HrTaskChecklist>
{
    public void Configure(EntityTypeBuilder<HrTaskChecklist> builder)
    {
        builder.ToTable("hr_task_checklists");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(500).IsRequired();
    }
}

public class HrTaskCommentConfiguration : IEntityTypeConfiguration<HrTaskComment>
{
    public void Configure(EntityTypeBuilder<HrTaskComment> builder)
    {
        builder.ToTable("hr_task_comments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UserName).HasMaxLength(200);
        builder.Property(x => x.Content).IsRequired();
    }
}

public class HrTaskActivityConfiguration : IEntityTypeConfiguration<HrTaskActivity>
{
    public void Configure(EntityTypeBuilder<HrTaskActivity> builder)
    {
        builder.ToTable("hr_task_activities");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Action).HasMaxLength(100).IsRequired();
        builder.Property(x => x.UserName).HasMaxLength(200);
    }
}
