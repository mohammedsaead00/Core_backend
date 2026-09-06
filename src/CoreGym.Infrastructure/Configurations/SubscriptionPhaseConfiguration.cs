using CoreGym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGym.Infrastructure.Configurations;

public class SubscriptionPhaseConfiguration : IEntityTypeConfiguration<SubscriptionPhase>
{
    public void Configure(EntityTypeBuilder<SubscriptionPhase> builder)
    {
        builder.ToTable("subscription_phases");
        builder.HasKey(ph => ph.Id).HasName("PK_subscription_phases");

        builder.Property(ph => ph.Id).HasColumnName("id");
        builder.Property(ph => ph.SubscriptionId).HasColumnName("subscription_id");
        builder.Property(ph => ph.PhaseNumber).HasColumnName("phase_number");
        builder.Property(ph => ph.Title).HasColumnName("title").IsRequired().HasMaxLength(200);
        builder.Property(ph => ph.Type).HasColumnName("type").HasMaxLength(50);
        builder.Property(ph => ph.Description).HasColumnName("description");
        builder.Property(ph => ph.DurationWeeks).HasColumnName("duration_weeks");
        builder.Property(ph => ph.Status).HasColumnName("status").HasMaxLength(50).HasDefaultValueSql("(N'upcoming')");
        builder.Property(ph => ph.StartedAt).HasColumnName("started_at");
        builder.Property(ph => ph.CompletedAt).HasColumnName("completed_at");
        builder.Property(ph => ph.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(SYSDATETIMEOFFSET())");

        builder.HasOne(ph => ph.Subscription)
            .WithMany()
            .HasForeignKey(ph => ph.SubscriptionId)
            .HasConstraintName("FK_subscription_phases_subscriptions")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(ph => ph.SubscriptionId).HasDatabaseName("IX_subscription_phases_subscription_id");
    }
}
