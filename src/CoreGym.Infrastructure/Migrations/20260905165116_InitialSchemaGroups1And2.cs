using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreGym.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchemaGroups1And2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "exercises",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    name_ar = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    muscle_group = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    secondary_muscles = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    equipment = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    instructions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    instructions_ar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tips = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "(SYSDATETIMEOFFSET())"),
                    image_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    youtube_video_id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    gif_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exercises", x => x.id);
                    table.CheckConstraint("CK_exercises_secondary_muscles_json", "[secondary_muscles] IS NULL OR ISJSON([secondary_muscles]) = 1");
                });

            migrationBuilder.CreateTable(
                name: "profiles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true, defaultValueSql: "(N'')"),
                    email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true, defaultValueSql: "(N'')"),
                    gender = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    age = table.Column<int>(type: "int", nullable: true),
                    weight_kg = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    height_cm = table.Column<decimal>(type: "decimal(5,1)", nullable: true),
                    fitness_goal = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    avatar_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "(SYSDATETIMEOFFSET())"),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "(SYSDATETIMEOFFSET())"),
                    role = table.Column<string>(type: "nvarchar(20)", nullable: false, defaultValueSql: "(N'client')"),
                    full_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_profiles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "training_programs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    name_ar = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    description_ar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    level = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    goal = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    days_per_week = table.Column<int>(type: "int", nullable: true),
                    duration_weeks = table.Column<int>(type: "int", nullable: true),
                    split_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((1))"),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "(SYSDATETIMEOFFSET())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_training_programs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "body_measurements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    weight_kg = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    body_fat_pct = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    muscle_mass = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    chest_cm = table.Column<decimal>(type: "decimal(5,1)", nullable: true),
                    waist_cm = table.Column<decimal>(type: "decimal(5,1)", nullable: true),
                    hips_cm = table.Column<decimal>(type: "decimal(5,1)", nullable: true),
                    arms_cm = table.Column<decimal>(type: "decimal(5,1)", nullable: true),
                    thighs_cm = table.Column<decimal>(type: "decimal(5,1)", nullable: true),
                    measured_date = table.Column<DateTime>(type: "date", nullable: true, defaultValueSql: "(CAST(SYSUTCDATETIME() AS date))"),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "(SYSDATETIMEOFFSET())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_body_measurements", x => x.id);
                    table.ForeignKey(
                        name: "FK_body_measurements_profiles",
                        column: x => x.user_id,
                        principalTable: "profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "foods",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    name_ar = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    calories = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    protein_g = table.Column<decimal>(type: "decimal(8,2)", nullable: false, defaultValue: 0m),
                    carbs_g = table.Column<decimal>(type: "decimal(8,2)", nullable: false, defaultValue: 0m),
                    fat_g = table.Column<decimal>(type: "decimal(8,2)", nullable: false, defaultValue: 0m),
                    fiber_g = table.Column<decimal>(type: "decimal(8,2)", nullable: false, defaultValue: 0m),
                    serving_size = table.Column<decimal>(type: "decimal(6,2)", nullable: true, defaultValueSql: "((100))"),
                    serving_unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, defaultValueSql: "(N'g')"),
                    is_custom = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "(SYSDATETIMEOFFSET())"),
                    category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, defaultValueSql: "(N'other')"),
                    image_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_foods", x => x.id);
                    table.ForeignKey(
                        name: "FK_foods_profiles",
                        column: x => x.created_by,
                        principalTable: "profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "onboarding",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    age = table.Column<int>(type: "int", nullable: true),
                    gender = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    height_cm = table.Column<decimal>(type: "decimal(5,1)", nullable: true),
                    weight_kg = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    goal = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    activity_level = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    target_weight = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    weekly_workouts = table.Column<int>(type: "int", nullable: true),
                    completed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "(SYSDATETIMEOFFSET())"),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "(SYSDATETIMEOFFSET())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_onboarding", x => x.id);
                    table.ForeignKey(
                        name: "FK_onboarding_profiles",
                        column: x => x.user_id,
                        principalTable: "profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_goals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    daily_calories = table.Column<int>(type: "int", nullable: true, defaultValueSql: "((2000))"),
                    daily_protein_g = table.Column<int>(type: "int", nullable: true, defaultValueSql: "((150))"),
                    daily_carbs_g = table.Column<int>(type: "int", nullable: true, defaultValueSql: "((250))"),
                    daily_fat_g = table.Column<int>(type: "int", nullable: true, defaultValueSql: "((65))"),
                    daily_water_ml = table.Column<int>(type: "int", nullable: true, defaultValueSql: "((2500))"),
                    daily_steps = table.Column<int>(type: "int", nullable: true, defaultValueSql: "((10000))"),
                    daily_sleep_hours = table.Column<decimal>(type: "decimal(4,2)", nullable: true, defaultValueSql: "((8))"),
                    weekly_workouts = table.Column<int>(type: "int", nullable: true, defaultValueSql: "((4))"),
                    target_weight_kg = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "(SYSDATETIMEOFFSET())"),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "(SYSDATETIMEOFFSET())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_goals", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_goals_profiles",
                        column: x => x.user_id,
                        principalTable: "profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_streaks",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    current_streak = table.Column<int>(type: "int", nullable: false, defaultValueSql: "((0))"),
                    longest_streak = table.Column<int>(type: "int", nullable: false, defaultValueSql: "((0))"),
                    last_active_date = table.Column<DateTime>(type: "date", nullable: true),
                    freeze_available = table.Column<int>(type: "int", nullable: true, defaultValueSql: "((1))"),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "(SYSDATETIMEOFFSET())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_streaks", x => x.user_id);
                    table.ForeignKey(
                        name: "FK_user_streaks_profiles",
                        column: x => x.user_id,
                        principalTable: "profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "program_days",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    program_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    day_number = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    name_ar = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    muscle_groups = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_program_days", x => x.id);
                    table.CheckConstraint("CK_program_days_muscle_groups_json", "[muscle_groups] IS NULL OR ISJSON([muscle_groups]) = 1");
                    table.ForeignKey(
                        name: "FK_program_days_training_programs",
                        column: x => x.program_id,
                        principalTable: "training_programs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "program_day_exercises",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    program_day_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    exercise_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    order_index = table.Column<int>(type: "int", nullable: false),
                    sets = table.Column<int>(type: "int", nullable: true),
                    reps_min = table.Column<int>(type: "int", nullable: true),
                    reps_max = table.Column<int>(type: "int", nullable: true),
                    rest_seconds = table.Column<int>(type: "int", nullable: true, defaultValueSql: "((90))"),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    notes_ar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    is_main_lift = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_program_day_exercises", x => x.id);
                    table.ForeignKey(
                        name: "FK_program_day_exercises_exercises",
                        column: x => x.exercise_id,
                        principalTable: "exercises",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_program_day_exercises_program_days",
                        column: x => x.program_day_id,
                        principalTable: "program_days",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_body_measurements_user_id_measured_date",
                table: "body_measurements",
                columns: new[] { "user_id", "measured_date" });

            migrationBuilder.CreateIndex(
                name: "IX_foods_created_by",
                table: "foods",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_onboarding_user_id",
                table: "onboarding",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_program_day_exercises_exercise_id",
                table: "program_day_exercises",
                column: "exercise_id");

            migrationBuilder.CreateIndex(
                name: "IX_program_day_exercises_program_day_id_order_index",
                table: "program_day_exercises",
                columns: new[] { "program_day_id", "order_index" });

            migrationBuilder.CreateIndex(
                name: "IX_program_days_program_id",
                table: "program_days",
                column: "program_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_goals_user_id",
                table: "user_goals",
                column: "user_id");

            // Replicates the Supabase update_updated_at() trigger on every
            // Group 1-2 table with an updated_at column. Recursive triggers are
            // disabled by default in SQL Server, so the inner UPDATE does not re-fire.
            // Each CREATE TRIGGER runs in its own batch (required by T-SQL).
            migrationBuilder.Sql(@"
CREATE TRIGGER dbo.trg_profiles_updated_at
ON dbo.profiles
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE p SET updated_at = SYSDATETIMEOFFSET()
    FROM dbo.profiles p INNER JOIN inserted i ON i.id = p.id;
END");

            migrationBuilder.Sql(@"
CREATE TRIGGER dbo.trg_onboarding_updated_at
ON dbo.onboarding
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE o SET updated_at = SYSDATETIMEOFFSET()
    FROM dbo.onboarding o INNER JOIN inserted i ON i.id = o.id;
END");

            migrationBuilder.Sql(@"
CREATE TRIGGER dbo.trg_user_goals_updated_at
ON dbo.user_goals
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE g SET updated_at = SYSDATETIMEOFFSET()
    FROM dbo.user_goals g INNER JOIN inserted i ON i.id = g.id;
END");

            migrationBuilder.Sql(@"
CREATE TRIGGER dbo.trg_user_streaks_updated_at
ON dbo.user_streaks
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE s SET updated_at = SYSDATETIMEOFFSET()
    FROM dbo.user_streaks s INNER JOIN inserted i ON i.user_id = s.user_id;
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "body_measurements");

            migrationBuilder.DropTable(
                name: "foods");

            migrationBuilder.DropTable(
                name: "onboarding");

            migrationBuilder.DropTable(
                name: "program_day_exercises");

            migrationBuilder.DropTable(
                name: "user_goals");

            migrationBuilder.DropTable(
                name: "user_streaks");

            migrationBuilder.DropTable(
                name: "exercises");

            migrationBuilder.DropTable(
                name: "program_days");

            migrationBuilder.DropTable(
                name: "profiles");

            migrationBuilder.DropTable(
                name: "training_programs");
        }
    }
}
