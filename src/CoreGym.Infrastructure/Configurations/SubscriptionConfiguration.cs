using CoreGym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGym.Infrastructure.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        // Declares the updated_at trigger so EF avoids the OUTPUT clause on SaveChanges.
        builder.ToTable("subscriptions", t =>
        {
            // NEW constraint (original column is unconstrained text): values confirmed
            // from the Dart SubscriptionStatus enum and stripe-webhook writes.
            t.HasCheckConstraint("CK_subscriptions_status",
                "[status] IN (N'pending', N'active', N'cancelled', N'expired')");
            t.HasTrigger("trg_subscriptions_updated_at");
        });
        builder.HasKey(s => s.Id).HasName("PK_subscriptions");

        builder.Property(s => s.Id).HasColumnName("id");
        builder.Property(s => s.ClientId).HasColumnName("client_id");
        builder.Property(s => s.CoachId).HasColumnName("coach_id");
        builder.Property(s => s.Status).HasColumnName("status").IsRequired().HasMaxLength(20).HasDefaultValueSql("(N'active')");
        builder.Property(s => s.Tier).HasColumnName("tier").HasMaxLength(50).HasDefaultValueSql("(N'basic')");
        builder.Property(s => s.StartDate).HasColumnName("start_date").HasColumnType("date").HasDefaultValueSql("(CAST(SYSUTCDATETIME() AS date))");
        builder.Property(s => s.EndDate).HasColumnName("end_date").HasColumnType("date");
        builder.Property(s => s.StripeSubId).HasColumnName("stripe_sub_id").HasMaxLength(100);
        builder.Property(s => s.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(SYSDATETIMEOFFSET())");
        builder.Property(s => s.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("(SYSDATETIMEOFFSET())");
        builder.Property(s => s.PlanId).HasColumnName("plan_id");
        builder.Property(s => s.PaymentStatus).HasColumnName("payment_status").HasMaxLength(50);
        builder.Property(s => s.StartedAt).HasColumnName("started_at");
        builder.Property(s => s.ExpiresAt).HasColumnName("expires_at");
        builder.Property(s => s.Goals).HasColumnName("goals");
        builder.Property(s => s.Notes).HasColumnName("notes");

        builder.HasOne(s => s.Client)
            .WithMany()
            .HasForeignKey(s => s.ClientId)
            .HasConstraintName("FK_subscriptions_profiles")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Coach)
            .WithMany()
            .HasForeignKey(s => s.CoachId)
            .HasConstraintName("FK_subscriptions_coaches")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Plan)
            .WithMany()
            .HasForeignKey(s => s.PlanId)
            .HasConstraintName("FK_subscriptions_subscription_plans")
            .OnDelete(DeleteBehavior.Restrict);

        // Hot path: the Authorization Service looks up active subscriptions by
        // client + coach; coaches list their subscriptions by coach_id.
        builder.HasIndex(s => new { s.ClientId, s.CoachId })
            .HasDatabaseName("IX_subscriptions_client_id_coach_id");
        builder.HasIndex(s => s.CoachId).HasDatabaseName("IX_subscriptions_coach_id");
        builder.HasIndex(s => s.PlanId).HasDatabaseName("IX_subscriptions_plan_id");
    }
}
