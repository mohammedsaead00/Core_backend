using CoreGym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGym.Infrastructure.Configurations;

public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.ToTable("subscription_plans");
        builder.HasKey(sp => sp.Id).HasName("PK_subscription_plans");

        builder.Property(sp => sp.Id).HasColumnName("id");
        builder.Property(sp => sp.CoachId).HasColumnName("coach_id");
        builder.Property(sp => sp.Name).HasColumnName("name").IsRequired().HasMaxLength(200);
        builder.Property(sp => sp.PriceUsd).HasColumnName("price_usd").HasColumnType("decimal(10,2)");
        builder.Property(sp => sp.DurationDays).HasColumnName("duration_days");
        builder.Property(sp => sp.MaxClients).HasColumnName("max_clients");
        builder.Property(sp => sp.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(SYSDATETIMEOFFSET())");

        // NOTE: unlike subscriptions.coach_id, this is the AUTH USER id
        // (original RLS: coach_manage_own_plans USING (coach_id = auth.uid())).
        builder.HasOne(sp => sp.Coach)
            .WithMany()
            .HasForeignKey(sp => sp.CoachId)
            .HasConstraintName("FK_subscription_plans_profiles")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(sp => sp.CoachId).HasDatabaseName("IX_subscription_plans_coach_id");
    }
}
