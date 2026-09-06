using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreGym.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGroups3Schema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "barcode_products",
                columns: table => new
                {
                    barcode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    product_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    product_name_ar = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    brand = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    serving_size_g = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    calories = table.Column<decimal>(type: "decimal(8,2)", nullable: false, defaultValue: 0m),
                    protein_g = table.Column<decimal>(type: "decimal(8,2)", nullable: false, defaultValue: 0m),
                    carbs_g = table.Column<decimal>(type: "decimal(8,2)", nullable: false, defaultValue: 0m),
                    fat_g = table.Column<decimal>(type: "decimal(8,2)", nullable: false, defaultValue: 0m),
                    source = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    confidence = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, defaultValueSql: "(N'high')"),
                    lookup_count = table.Column<int>(type: "int", nullable: true, defaultValueSql: "((1))"),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "(SYSDATETIMEOFFSET())"),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "(SYSDATETIMEOFFSET())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_barcode_products", x => x.barcode);
                    table.CheckConstraint("CK_barcode_products_source", "[source] IN (N'openfoodfacts', N'gemini_estimate')");
                });

            migrationBuilder.CreateTable(
                name: "daily_activity",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    activity_date = table.Column<DateTime>(type: "date", nullable: true, defaultValueSql: "(CAST(SYSUTCDATETIME() AS date))"),
                    steps = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    active_calories_burned = table.Column<decimal>(type: "decimal(8,2)", nullable: false, defaultValue: 0m),
                    heart_rate_avg = table.Column<int>(type: "int", nullable: true),
                    exercise_minutes = table.Column<int>(type: "int", nullable: true),
                    source = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, defaultValueSql: "(N'health_connect')"),
                    synced_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "(SYSDATETIMEOFFSET())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_daily_activity", x => x.id);
                    table.ForeignKey(
                        name: "FK_daily_activity_profiles",
                        column: x => x.user_id,
                        principalTable: "profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "daily_summary",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    summary_date = table.Column<DateTime>(type: "date", nullable: true, defaultValueSql: "(CAST(SYSUTCDATETIME() AS date))"),
                    steps = table.Column<int>(type: "int", nullable: true),
                    active_minutes = table.Column<int>(type: "int", nullable: true),
                    calories_burned = table.Column<int>(type: "int", nullable: true),
                    water_ml = table.Column<int>(type: "int", nullable: true),
                    sleep_hours = table.Column<decimal>(type: "decimal(4,2)", nullable: true),
                    calories_consumed = table.Column<decimal>(type: "decimal(8,2)", nullable: true),
                    protein_g = table.Column<decimal>(type: "decimal(8,2)", nullable: true),
                    carbs_g = table.Column<decimal>(type: "decimal(8,2)", nullable: true),
                    fat_g = table.Column<decimal>(type: "decimal(8,2)", nullable: true),
                    workout_done = table.Column<bool>(type: "bit", nullable: true),
                    workout_duration = table.Column<int>(type: "int", nullable: true),
                    mood = table.Column<int>(type: "int", nullable: true),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "(SYSDATETIMEOFFSET())"),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "(SYSDATETIMEOFFSET())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_daily_summary", x => x.id);
                    table.ForeignKey(
                        name: "FK_daily_summary_profiles",
                        column: x => x.user_id,
                        principalTable: "profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "food_scans",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    image_path = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    is_food = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((1))"),
                    confidence = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, defaultValueSql: "(N'medium')"),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    scanned_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "(SYSDATETIMEOFFSET())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_food_scans", x => x.id);
                    table.ForeignKey(
                        name: "FK_food_scans_profiles",
                        column: x => x.user_id,
                        principalTable: "profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "nutrition_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    food_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    food_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    meal_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    quantity = table.Column<decimal>(type: "decimal(8,2)", nullable: false, defaultValueSql: "((1))"),
                    serving_unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, defaultValueSql: "(N'g')"),
                    calories = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    protein_g = table.Column<decimal>(type: "decimal(8,2)", nullable: false, defaultValue: 0m),
                    carbs_g = table.Column<decimal>(type: "decimal(8,2)", nullable: false, defaultValue: 0m),
                    fat_g = table.Column<decimal>(type: "decimal(8,2)", nullable: false, defaultValue: 0m),
                    logged_date = table.Column<DateTime>(type: "date", nullable: true, defaultValueSql: "(CAST(SYSUTCDATETIME() AS date))"),
                    logged_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "(SYSDATETIMEOFFSET())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nutrition_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_nutrition_logs_foods",
                        column: x => x.food_id,
                        principalTable: "foods",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_nutrition_logs_profiles",
                        column: x => x.user_id,
                        principalTable: "profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "streak_activity_log",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    activity_date = table.Column<DateTime>(type: "date", nullable: false),
                    source = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "(SYSDATETIMEOFFSET())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_streak_activity_log", x => x.id);
                    table.CheckConstraint("CK_streak_activity_log_source", "[source] IN (N'workout', N'nutrition')");
                    table.ForeignKey(
                        name: "FK_streak_activity_log_profiles",
                        column: x => x.user_id,
                        principalTable: "profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_active_program",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    program_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    started_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "(SYSDATETIMEOFFSET())"),
                    current_week = table.Column<int>(type: "int", nullable: true, defaultValueSql: "((1))"),
                    current_day = table.Column<int>(type: "int", nullable: true, defaultValueSql: "((1))"),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "(SYSDATETIMEOFFSET())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_active_program", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_active_program_profiles",
                        column: x => x.user_id,
                        principalTable: "profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_active_program_training_programs",
                        column: x => x.program_id,
                        principalTable: "training_programs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_programs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    program_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    muscle_group = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((1))"),
                    started_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_programs", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_programs_profiles",
                        column: x => x.user_id,
                        principalTable: "profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "voice_food_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    audio_path = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    transcript = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    is_food = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((1))"),
                    confidence = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, defaultValueSql: "(N'medium')"),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    logged_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "(SYSDATETIMEOFFSET())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_voice_food_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_voice_food_logs_profiles",
                        column: x => x.user_id,
                        principalTable: "profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "weekly_activity",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    week_start = table.Column<DateTime>(type: "date", nullable: false),
                    day_index = table.Column<int>(type: "int", nullable: false),
                    actual_pct = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    goal_pct = table.Column<decimal>(type: "decimal(5,2)", nullable: false, defaultValue: 0m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_weekly_activity", x => x.id);
                    table.ForeignKey(
                        name: "FK_weekly_activity_profiles",
                        column: x => x.user_id,
                        principalTable: "profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "workout_sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    muscle_group = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    session_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    duration_min = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    session_date = table.Column<DateTime>(type: "date", nullable: true, defaultValueSql: "(CAST(SYSUTCDATETIME() AS date))"),
                    started_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ended_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workout_sessions", x => x.id);
                    table.ForeignKey(
                        name: "FK_workout_sessions_profiles",
                        column: x => x.user_id,
                        principalTable: "profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "barcode_scan_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    barcode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    quantity_g = table.Column<decimal>(type: "decimal(8,2)", nullable: true),
                    nutrition_log_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    scanned_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_barcode_scan_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_barcode_scan_history_barcode_products",
                        column: x => x.barcode,
                        principalTable: "barcode_products",
                        principalColumn: "barcode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_barcode_scan_history_nutrition_logs",
                        column: x => x.nutrition_log_id,
                        principalTable: "nutrition_logs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_barcode_scan_history_profiles",
                        column: x => x.user_id,
                        principalTable: "profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "food_scan_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    scan_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    name_ar = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    estimated_weight_g = table.Column<decimal>(type: "decimal(8,2)", nullable: false, defaultValue: 0m),
                    calories = table.Column<decimal>(type: "decimal(8,2)", nullable: false, defaultValue: 0m),
                    protein_g = table.Column<decimal>(type: "decimal(8,2)", nullable: false, defaultValue: 0m),
                    carbs_g = table.Column<decimal>(type: "decimal(8,2)", nullable: false, defaultValue: 0m),
                    fat_g = table.Column<decimal>(type: "decimal(8,2)", nullable: false, defaultValue: 0m),
                    nutrition_log_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "(SYSDATETIMEOFFSET())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_food_scan_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_food_scan_items_food_scans",
                        column: x => x.scan_id,
                        principalTable: "food_scans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_food_scan_items_nutrition_logs",
                        column: x => x.nutrition_log_id,
                        principalTable: "nutrition_logs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "voice_food_log_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    log_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    name_ar = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    estimated_weight_g = table.Column<decimal>(type: "decimal(8,2)", nullable: false, defaultValue: 0m),
                    calories = table.Column<decimal>(type: "decimal(8,2)", nullable: false, defaultValue: 0m),
                    protein_g = table.Column<decimal>(type: "decimal(8,2)", nullable: false, defaultValue: 0m),
                    carbs_g = table.Column<decimal>(type: "decimal(8,2)", nullable: false, defaultValue: 0m),
                    fat_g = table.Column<decimal>(type: "decimal(8,2)", nullable: false, defaultValue: 0m),
                    nutrition_log_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "(SYSDATETIMEOFFSET())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_voice_food_log_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_voice_food_log_items_nutrition_logs",
                        column: x => x.nutrition_log_id,
                        principalTable: "nutrition_logs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_voice_food_log_items_voice_food_logs",
                        column: x => x.log_id,
                        principalTable: "voice_food_logs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "exercise_progress",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    exercise_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    session_date = table.Column<DateTime>(type: "date", nullable: true, defaultValueSql: "(CAST(SYSUTCDATETIME() AS date))"),
                    best_set_weight = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    best_set_reps = table.Column<int>(type: "int", nullable: true),
                    total_volume = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    one_rm_estimate = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    session_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "(SYSDATETIMEOFFSET())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exercise_progress", x => x.id);
                    table.ForeignKey(
                        name: "FK_exercise_progress_exercises",
                        column: x => x.exercise_id,
                        principalTable: "exercises",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exercise_progress_profiles",
                        column: x => x.user_id,
                        principalTable: "profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exercise_progress_workout_sessions",
                        column: x => x.session_id,
                        principalTable: "workout_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "workout_sets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    session_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    exercise_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    set_number = table.Column<int>(type: "int", nullable: false),
                    reps = table.Column<int>(type: "int", nullable: true),
                    weight_kg = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    duration_sec = table.Column<int>(type: "int", nullable: true),
                    rest_sec = table.Column<int>(type: "int", nullable: true, defaultValueSql: "((60))"),
                    is_warmup = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    logged_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workout_sets", x => x.id);
                    table.ForeignKey(
                        name: "FK_workout_sets_profiles",
                        column: x => x.user_id,
                        principalTable: "profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_workout_sets_workout_sessions",
                        column: x => x.session_id,
                        principalTable: "workout_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_barcode_scan_history_barcode",
                table: "barcode_scan_history",
                column: "barcode");

            migrationBuilder.CreateIndex(
                name: "IX_barcode_scan_history_nutrition_log_id",
                table: "barcode_scan_history",
                column: "nutrition_log_id");

            migrationBuilder.CreateIndex(
                name: "IX_barcode_scan_history_user_id",
                table: "barcode_scan_history",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_daily_activity_user_id_activity_date",
                table: "daily_activity",
                columns: new[] { "user_id", "activity_date" });

            migrationBuilder.CreateIndex(
                name: "IX_daily_summary_user_id_summary_date",
                table: "daily_summary",
                columns: new[] { "user_id", "summary_date" });

            migrationBuilder.CreateIndex(
                name: "IX_exercise_progress_exercise_id",
                table: "exercise_progress",
                column: "exercise_id");

            migrationBuilder.CreateIndex(
                name: "IX_exercise_progress_session_id",
                table: "exercise_progress",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "IX_exercise_progress_user_id_session_date",
                table: "exercise_progress",
                columns: new[] { "user_id", "session_date" });

            migrationBuilder.CreateIndex(
                name: "IX_food_scan_items_nutrition_log_id",
                table: "food_scan_items",
                column: "nutrition_log_id");

            migrationBuilder.CreateIndex(
                name: "IX_food_scan_items_scan_id",
                table: "food_scan_items",
                column: "scan_id");

            migrationBuilder.CreateIndex(
                name: "IX_food_scans_user_id",
                table: "food_scans",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_nutrition_logs_food_id",
                table: "nutrition_logs",
                column: "food_id");

            migrationBuilder.CreateIndex(
                name: "IX_nutrition_logs_user_id_logged_date",
                table: "nutrition_logs",
                columns: new[] { "user_id", "logged_date" });

            migrationBuilder.CreateIndex(
                name: "IX_streak_activity_log_user_id_activity_date",
                table: "streak_activity_log",
                columns: new[] { "user_id", "activity_date" });

            migrationBuilder.CreateIndex(
                name: "IX_user_active_program_program_id",
                table: "user_active_program",
                column: "program_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_active_program_user_id",
                table: "user_active_program",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_programs_user_id",
                table: "user_programs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_voice_food_log_items_log_id",
                table: "voice_food_log_items",
                column: "log_id");

            migrationBuilder.CreateIndex(
                name: "IX_voice_food_log_items_nutrition_log_id",
                table: "voice_food_log_items",
                column: "nutrition_log_id");

            migrationBuilder.CreateIndex(
                name: "IX_voice_food_logs_user_id",
                table: "voice_food_logs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_weekly_activity_user_id_week_start",
                table: "weekly_activity",
                columns: new[] { "user_id", "week_start" });

            migrationBuilder.CreateIndex(
                name: "IX_workout_sessions_user_id_session_date",
                table: "workout_sessions",
                columns: new[] { "user_id", "session_date" });

            migrationBuilder.CreateIndex(
                name: "IX_workout_sets_session_id",
                table: "workout_sets",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "IX_workout_sets_user_id_exercise_name",
                table: "workout_sets",
                columns: new[] { "user_id", "exercise_name" });

            // Replicates the Supabase update_updated_at() trigger on the Group 3
            // tables that have an updated_at column. Recursive triggers are disabled
            // by default in SQL Server, so the inner UPDATE does not re-fire.
            // Each CREATE TRIGGER runs in its own batch (required by T-SQL).
            migrationBuilder.Sql(@"
CREATE TRIGGER dbo.trg_daily_summary_updated_at
ON dbo.daily_summary
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE s SET updated_at = SYSDATETIMEOFFSET()
    FROM dbo.daily_summary s INNER JOIN inserted i ON i.id = s.id;
END");

            migrationBuilder.Sql(@"
CREATE TRIGGER dbo.trg_user_active_program_updated_at
ON dbo.user_active_program
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE p SET updated_at = SYSDATETIMEOFFSET()
    FROM dbo.user_active_program p INNER JOIN inserted i ON i.id = p.id;
END");

            migrationBuilder.Sql(@"
CREATE TRIGGER dbo.trg_barcode_products_updated_at
ON dbo.barcode_products
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE b SET updated_at = SYSDATETIMEOFFSET()
    FROM dbo.barcode_products b INNER JOIN inserted i ON i.barcode = b.barcode;
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "barcode_scan_history");

            migrationBuilder.DropTable(
                name: "daily_activity");

            migrationBuilder.DropTable(
                name: "daily_summary");

            migrationBuilder.DropTable(
                name: "exercise_progress");

            migrationBuilder.DropTable(
                name: "food_scan_items");

            migrationBuilder.DropTable(
                name: "streak_activity_log");

            migrationBuilder.DropTable(
                name: "user_active_program");

            migrationBuilder.DropTable(
                name: "user_programs");

            migrationBuilder.DropTable(
                name: "voice_food_log_items");

            migrationBuilder.DropTable(
                name: "weekly_activity");

            migrationBuilder.DropTable(
                name: "workout_sets");

            migrationBuilder.DropTable(
                name: "barcode_products");

            migrationBuilder.DropTable(
                name: "food_scans");

            migrationBuilder.DropTable(
                name: "nutrition_logs");

            migrationBuilder.DropTable(
                name: "voice_food_logs");

            migrationBuilder.DropTable(
                name: "workout_sessions");
        }
    }
}
