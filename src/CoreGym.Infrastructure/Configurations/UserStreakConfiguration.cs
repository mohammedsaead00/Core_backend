using CoreGym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGym.Infrastructure.Configurations;

public class UserStreakConfiguration : IEntityTypeConfiguration<UserStreak>
{
    public void Configure(EntityTypeBuilder<UserStreak> builder)
    {
        // Declares the updated_at trigger so EF avoids the OUTPUT clause on SaveChanges.
        builder.ToTable("user_streaks", t =>
        {
            t.HasTrigger("trg_user_streaks_updated_at");
        });
        builder.HasKey(s => s.UserId).HasName("PK_user_streaks");

        builder.Property(s => s.UserId).HasColumnName("user_id");
        builder.Property(s => s.CurrentStreak).HasColumnName("current_streak").HasDefaultValueSql("((0))");
        builder.Property(s => s.LongestStreak).HasColumnName("longest_streak").HasDefaultValueSql("((0))");
        builder.Property(s => s.LastActiveDate).HasColumnName("last_active_date").HasColumnType("date");
        builder.Property(s => s.FreezeAvailable).HasColumnName("freeze_available").HasDefaultValueSql("((1))");
        builder.Property(s => s.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("(SYSDATETIMEOFFSET())");

        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .HasConstraintName("FK_user_streaks_profiles")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
