using HR.Domain.Engines.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HR.Infrastructure.Persistence.Configurations.Engines;

public class NotificationRuleConfiguration : IEntityTypeConfiguration<NotificationRule>
{
    public void Configure(EntityTypeBuilder<NotificationRule> builder)
    {
        builder.ToTable("notification_rules");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200);
        builder.Property(x => x.Event).HasMaxLength(50);
        builder.Property(x => x.DocumentType).HasMaxLength(50);
        builder.HasIndex(x => new { x.TenantId, x.Event, x.IsActive });
    }
}

public class NotificationDispatchConfiguration : IEntityTypeConfiguration<NotificationDispatch>
{
    public void Configure(EntityTypeBuilder<NotificationDispatch> builder)
    {
        builder.ToTable("notification_dispatches");
        builder.HasKey(x => x.Id);
        // One notification per rule + source entity + recipient.
        builder.HasIndex(x => new { x.RuleId, x.SourceEntityId, x.UserId }).IsUnique();
    }
}
