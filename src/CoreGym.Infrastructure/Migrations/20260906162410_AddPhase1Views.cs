using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreGym.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPhase1Views : Migration
    {
        // The three read-only views from the original Supabase project. The
        // inventory lists them but not their SQL, so these definitions are
        // INFERRED from the underlying tables (2026-09-06) and should be
        // validated against prod via pg_get_viewdef and adjusted if needed.
        // Like Postgres, no row-level filtering lives inside the views —
        // caller filtering is the Authorization Service's job.
        // Each CREATE VIEW runs in its own batch (required by T-SQL).

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE VIEW dbo.personal_records
AS
SELECT
    s.user_id,
    s.exercise_name,
    MAX(s.weight_kg) AS best_weight_kg,
    MAX(s.reps) AS best_reps,
    MAX(CAST(s.weight_kg * s.reps AS decimal(18,2))) AS best_set_volume,
    MAX(CAST(s.weight_kg * (1 + ISNULL(s.reps, 0) / 30.0) AS decimal(18,2))) AS estimated_one_rm,
    MAX(s.logged_at) AS last_logged_at
FROM dbo.workout_sets s
WHERE s.is_warmup = 0
GROUP BY s.user_id, s.exercise_name");

            migrationBuilder.Sql(@"
CREATE VIEW dbo.weekly_progress
AS
SELECT
    wa.user_id,
    wa.week_start,
    COUNT(*) AS days_logged,
    SUM(CASE WHEN wa.actual_pct >= wa.goal_pct THEN 1 ELSE 0 END) AS days_goal_met,
    AVG(wa.actual_pct) AS avg_actual_pct,
    AVG(wa.goal_pct) AS avg_goal_pct
FROM dbo.weekly_activity wa
GROUP BY wa.user_id, wa.week_start");

            migrationBuilder.Sql(@"
CREATE VIEW dbo.weight_progress
AS
SELECT
    bm.user_id,
    bm.measured_date,
    bm.weight_kg,
    bm.weight_kg - LAG(bm.weight_kg) OVER (
        PARTITION BY bm.user_id
        ORDER BY bm.measured_date, bm.created_at) AS weight_change_kg
FROM dbo.body_measurements bm
WHERE bm.weight_kg IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS dbo.weight_progress");
            migrationBuilder.Sql("DROP VIEW IF EXISTS dbo.weekly_progress");
            migrationBuilder.Sql("DROP VIEW IF EXISTS dbo.personal_records");
        }
    }
}
