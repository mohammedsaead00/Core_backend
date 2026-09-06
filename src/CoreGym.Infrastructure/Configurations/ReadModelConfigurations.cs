using CoreGym.Domain.ReadModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGym.Infrastructure.Configurations;

/// <summary>
/// Keyless query-only mappings for the three SQL Server views. The view DDL
/// itself is created by the AddPhase1Views migration (inferred definitions —
/// see MIGRATION_PROGRESS.md).
/// </summary>
public class PersonalRecordConfiguration : IEntityTypeConfiguration<PersonalRecord>
{
    public void Configure(EntityTypeBuilder<PersonalRecord> builder)
    {
        builder.ToView("personal_records");
        builder.HasNoKey();

        builder.Property(x => x.UserId).HasColumnName("user_id");
        builder.Property(x => x.ExerciseName).HasColumnName("exercise_name");
        builder.Property(x => x.BestWeightKg).HasColumnName("best_weight_kg");
        builder.Property(x => x.BestReps).HasColumnName("best_reps");
        builder.Property(x => x.BestSetVolume).HasColumnName("best_set_volume");
        builder.Property(x => x.EstimatedOneRm).HasColumnName("estimated_one_rm");
        builder.Property(x => x.LastLoggedAt).HasColumnName("last_logged_at");
    }
}

public class WeeklyProgressConfiguration : IEntityTypeConfiguration<WeeklyProgress>
{
    public void Configure(EntityTypeBuilder<WeeklyProgress> builder)
    {
        builder.ToView("weekly_progress");
        builder.HasNoKey();

        builder.Property(x => x.UserId).HasColumnName("user_id");
        builder.Property(x => x.WeekStart).HasColumnName("week_start");
        builder.Property(x => x.DaysLogged).HasColumnName("days_logged");
        builder.Property(x => x.DaysGoalMet).HasColumnName("days_goal_met");
        builder.Property(x => x.AvgActualPct).HasColumnName("avg_actual_pct");
        builder.Property(x => x.AvgGoalPct).HasColumnName("avg_goal_pct");
    }
}

public class WeightProgressConfiguration : IEntityTypeConfiguration<WeightProgress>
{
    public void Configure(EntityTypeBuilder<WeightProgress> builder)
    {
        builder.ToView("weight_progress");
        builder.HasNoKey();

        builder.Property(x => x.UserId).HasColumnName("user_id");
        builder.Property(x => x.MeasuredDate).HasColumnName("measured_date");
        builder.Property(x => x.WeightKg).HasColumnName("weight_kg");
        builder.Property(x => x.WeightChangeKg).HasColumnName("weight_change_kg");
    }
}
