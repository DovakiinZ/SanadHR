using HR.Domain.Engines.Timeline;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HR.Infrastructure.Persistence.Configurations.Engines;

public class TimelineEventConfiguration : IEntityTypeConfiguration<TimelineEvent>
{
    public void Configure(EntityTypeBuilder<TimelineEvent> builder)
    {
        builder.ToTable("engine_timeline_events");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Category).HasMaxLength(100).IsRequired();
        builder.Property(x => x.EntityType).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Action).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ActorName).HasMaxLength(200);
        builder.Property(x => x.Metadata).HasColumnType("jsonb");
        builder.HasIndex(x => new { x.EntityType, x.EntityId });
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => x.OccurredAt);
    }
}

public class TimelineSubscriptionConfiguration : IEntityTypeConfiguration<TimelineSubscription>
{
    public void Configure(EntityTypeBuilder<TimelineSubscription> builder)
    {
        builder.ToTable("engine_timeline_subscriptions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EntityType).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => new { x.UserId, x.EntityType, x.EntityId }).IsUnique();
        builder.HasIndex(x => x.TenantId);
    }
}
