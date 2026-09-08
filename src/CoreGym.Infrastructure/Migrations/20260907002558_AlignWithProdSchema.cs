using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreGym.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AlignWithProdSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_barcode_scan_history_barcode_products",
                table: "barcode_scan_history");

            migrationBuilder.DropForeignKey(
                name: "FK_notifications_subscription_plans",
                table: "notifications");

            migrationBuilder.DropIndex(
                name: "IX_user_goals_user_id",
                table: "user_goals");

            migrationBuilder.DropIndex(
                name: "IX_user_active_program_user_id",
                table: "user_active_program");

            migrationBuilder.DropIndex(
                name: "IX_stripe_customers_stripe_customer_id",
                table: "stripe_customers");

            migrationBuilder.DropIndex(
                name: "IX_stripe_customers_user_id",
                table: "stripe_customers");

            migrationBuilder.DropIndex(
                name: "IX_onboarding_user_id",
                table: "onboarding");

            migrationBuilder.DropIndex(
                name: "IX_notifications_plan_id",
                table: "notifications");

            migrationBuilder.DropCheckConstraint(
                name: "CK_messages_type",
                table: "messages");

            migrationBuilder.DropIndex(
                name: "IX_coaches_user_id",
                table: "coaches");

            migrationBuilder.DropCheckConstraint(
                name: "CK_barcode_products_source",
                table: "barcode_products");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "logged_at",
                table: "workout_sets",
                type: "datetimeoffset",
                nullable: false,
                defaultValueSql: "(SYSDATETIMEOFFSET())",
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "started_at",
                table: "workout_sessions",
                type: "datetimeoffset",
                nullable: false,
                defaultValueSql: "(SYSDATETIMEOFFSET())",
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "session_date",
                table: "workout_sessions",
                type: "date",
                nullable: false,
                defaultValueSql: "(CAST(SYSUTCDATETIME() AS date))",
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true,
                oldDefaultValueSql: "(CAST(SYSUTCDATETIME() AS date))");

            migrationBuilder.AlterColumn<int>(
                name: "goal_pct",
                table: "weekly_activity",
                type: "int",
                nullable: true,
                defaultValue: 0,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<int>(
                name: "actual_pct",
                table: "weekly_activity",
                type: "int",
                nullable: true,
                defaultValue: 0,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "logged_at",
                table: "voice_food_logs",
                type: "datetimeoffset",
                nullable: false,
                defaultValueSql: "(SYSDATETIMEOFFSET())",
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "freeze_available",
                table: "user_streaks",
                type: "int",
                nullable: false,
                defaultValueSql: "((1))",
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true,
                oldDefaultValueSql: "((1))");

            migrationBuilder.AlterColumn<string>(
                name: "tier",
                table: "subscriptions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValueSql: "(N'basic')",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true,
                oldDefaultValueSql: "(N'basic')");

            migrationBuilder.AlterColumn<DateTime>(
                name: "start_date",
                table: "subscriptions",
                type: "date",
                nullable: false,
                defaultValueSql: "(CAST(SYSUTCDATETIME() AS date))",
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true,
                oldDefaultValueSql: "(CAST(SYSUTCDATETIME() AS date))");

            migrationBuilder.AlterColumn<string>(
                name: "tier",
                table: "payment_intents",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValueSql: "(N'standard')",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true,
                oldDefaultValueSql: "(N'standard')");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "payment_intents",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValueSql: "(N'pending')",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true,
                oldDefaultValueSql: "(N'pending')");

            migrationBuilder.AlterColumn<string>(
                name: "currency",
                table: "payment_intents",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValueSql: "(N'usd')",
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldNullable: true,
                oldDefaultValueSql: "(N'usd')");

            migrationBuilder.AlterColumn<DateTime>(
                name: "logged_date",
                table: "nutrition_logs",
                type: "date",
                nullable: false,
                defaultValueSql: "(CAST(SYSUTCDATETIME() AS date))",
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true,
                oldDefaultValueSql: "(CAST(SYSUTCDATETIME() AS date))");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "scanned_at",
                table: "food_scans",
                type: "datetimeoffset",
                nullable: false,
                defaultValueSql: "(SYSDATETIMEOFFSET())",
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "workout_duration",
                table: "daily_summary",
                type: "int",
                nullable: true,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "workout_done",
                table: "daily_summary",
                type: "bit",
                nullable: true,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "water_ml",
                table: "daily_summary",
                type: "int",
                nullable: true,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "steps",
                table: "daily_summary",
                type: "int",
                nullable: true,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "protein_g",
                table: "daily_summary",
                type: "decimal(8,2)",
                nullable: true,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(8,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "fat_g",
                table: "daily_summary",
                type: "decimal(8,2)",
                nullable: true,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(8,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "carbs_g",
                table: "daily_summary",
                type: "decimal(8,2)",
                nullable: true,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(8,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "calories_consumed",
                table: "daily_summary",
                type: "decimal(8,2)",
                nullable: true,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(8,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "calories_burned",
                table: "daily_summary",
                type: "int",
                nullable: true,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "active_minutes",
                table: "daily_summary",
                type: "int",
                nullable: true,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "synced_at",
                table: "daily_activity",
                type: "datetimeoffset",
                nullable: false,
                defaultValueSql: "(SYSDATETIMEOFFSET())",
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "heart_rate_avg",
                table: "daily_activity",
                type: "decimal(6,2)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "exercise_minutes",
                table: "daily_activity",
                type: "decimal(6,2)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "activity_date",
                table: "daily_activity",
                type: "date",
                nullable: false,
                defaultValueSql: "(CAST(SYSUTCDATETIME() AS date))",
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true,
                oldDefaultValueSql: "(CAST(SYSUTCDATETIME() AS date))");

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                table: "coaches",
                type: "bit",
                nullable: false,
                defaultValueSql: "((1))",
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true,
                oldDefaultValueSql: "((1))");

            migrationBuilder.AlterColumn<string>(
                name: "bio",
                table: "coaches",
                type: "nvarchar(max)",
                nullable: false,
                defaultValueSql: "(N'')",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldDefaultValueSql: "(N'')");

            migrationBuilder.AlterColumn<string>(
                name: "specialization",
                table: "coach_onboarding",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "price_premium",
                table: "coach_onboarding",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "price_monthly",
                table: "coach_onboarding",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "max_clients",
                table: "coach_onboarding",
                type: "int",
                nullable: false,
                defaultValueSql: "((10))",
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true,
                oldDefaultValueSql: "((10))");

            migrationBuilder.AlterColumn<string>(
                name: "languages",
                table: "coach_onboarding",
                type: "nvarchar(max)",
                nullable: false,
                defaultValueSql: "(N'[\"Arabic\",\"English\"]')",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldDefaultValueSql: "(N'[\"Arabic\",\"English\"]')");

            migrationBuilder.AlterColumn<string>(
                name: "display_name",
                table: "coach_onboarding",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValueSql: "(N'')",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "certifications",
                table: "coach_onboarding",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "bio",
                table: "coach_onboarding",
                type: "nvarchar(max)",
                nullable: false,
                defaultValueSql: "(N'')",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "assigned_at",
                table: "client_assignments",
                type: "datetimeoffset",
                nullable: false,
                defaultValueSql: "(SYSDATETIMEOFFSET())",
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "measured_date",
                table: "body_measurements",
                type: "date",
                nullable: false,
                defaultValueSql: "(CAST(SYSUTCDATETIME() AS date))",
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true,
                oldDefaultValueSql: "(CAST(SYSUTCDATETIME() AS date))");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "scanned_at",
                table: "barcode_scan_history",
                type: "datetimeoffset",
                nullable: false,
                defaultValueSql: "(SYSDATETIMEOFFSET())",
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "barcode",
                table: "barcode_scan_history",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "confidence",
                table: "barcode_products",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValueSql: "(N'high')",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true,
                oldDefaultValueSql: "(N'high')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_workout_sessions_muscle_group",
                table: "workout_sessions",
                sql: "[muscle_group] IN (N'chest', N'arms', N'legs', N'core', N'back', N'shoulders', N'full_body')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_weekly_activity_actual_pct",
                table: "weekly_activity",
                sql: "[actual_pct] IS NULL OR ([actual_pct] >= 0 AND [actual_pct] <= 100)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_weekly_activity_day_index",
                table: "weekly_activity",
                sql: "[day_index] >= 0 AND [day_index] <= 6");

            migrationBuilder.AddCheckConstraint(
                name: "CK_weekly_activity_goal_pct",
                table: "weekly_activity",
                sql: "[goal_pct] IS NULL OR ([goal_pct] >= 0 AND [goal_pct] <= 100)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_voice_food_logs_confidence",
                table: "voice_food_logs",
                sql: "[confidence] IS NULL OR [confidence] IN (N'low', N'medium', N'high')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_user_programs_muscle_group",
                table: "user_programs",
                sql: "[muscle_group] IN (N'chest', N'arms', N'legs', N'core')");

            migrationBuilder.CreateIndex(
                name: "IX_user_goals_user_id",
                table: "user_goals",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_active_program_user_id",
                table: "user_active_program",
                column: "user_id",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_training_programs_goal",
                table: "training_programs",
                sql: "[goal] IS NULL OR [goal] IN (N'strength', N'muscle_gain', N'weight_loss', N'endurance', N'general_fitness')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_training_programs_level",
                table: "training_programs",
                sql: "[level] IS NULL OR [level] IN (N'beginner', N'intermediate', N'advanced')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_subscriptions_payment_status",
                table: "subscriptions",
                sql: "[payment_status] IS NULL OR [payment_status] IN (N'unpaid', N'paid', N'refunded')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_subscriptions_tier",
                table: "subscriptions",
                sql: "[tier] IN (N'basic', N'standard', N'premium')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_subscription_phases_status",
                table: "subscription_phases",
                sql: "[status] IS NULL OR [status] IN (N'upcoming', N'in_progress', N'completed')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_subscription_phases_type",
                table: "subscription_phases",
                sql: "[type] IS NULL OR [type] IN (N'workout', N'nutrition', N'combined')");

            migrationBuilder.CreateIndex(
                name: "IX_stripe_customers_stripe_customer_id",
                table: "stripe_customers",
                column: "stripe_customer_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_stripe_customers_user_id",
                table: "stripe_customers",
                column: "user_id",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_reviews_rating",
                table: "reviews",
                sql: "[rating] >= 1 AND [rating] <= 5");

            migrationBuilder.AddCheckConstraint(
                name: "CK_profiles_fitness_goal",
                table: "profiles",
                sql: "[fitness_goal] IS NULL OR [fitness_goal] IN (N'muscle_gain', N'weight_loss', N'endurance', N'flexibility')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_profiles_gender",
                table: "profiles",
                sql: "[gender] IS NULL OR [gender] IN (N'male', N'female')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_payment_intents_status",
                table: "payment_intents",
                sql: "[status] IN (N'pending', N'succeeded', N'failed', N'refunded')");

            migrationBuilder.CreateIndex(
                name: "IX_onboarding_user_id",
                table: "onboarding",
                column: "user_id",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_onboarding_activity_level",
                table: "onboarding",
                sql: "[activity_level] IS NULL OR [activity_level] IN (N'sedentary', N'lightly_active', N'moderately_active', N'very_active', N'extra_active')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_onboarding_gender",
                table: "onboarding",
                sql: "[gender] IS NULL OR [gender] IN (N'male', N'female')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_onboarding_goal",
                table: "onboarding",
                sql: "[goal] IS NULL OR [goal] IN (N'muscle_gain', N'weight_loss', N'endurance', N'flexibility', N'general_fitness')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_onboarding_weekly_workouts",
                table: "onboarding",
                sql: "[weekly_workouts] IS NULL OR ([weekly_workouts] >= 1 AND [weekly_workouts] <= 7)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_nutrition_logs_meal_type",
                table: "nutrition_logs",
                sql: "[meal_type] IS NULL OR [meal_type] IN (N'breakfast', N'lunch', N'dinner', N'snack')");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_coach_id",
                table: "notifications",
                column: "coach_id");

            migrationBuilder.AddCheckConstraint(
                name: "CK_notifications_type",
                table: "notifications",
                sql: "[type] IN (N'message', N'plan')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_messages_type",
                table: "messages",
                sql: "[type] IN (N'text', N'image', N'file', N'workout_plan', N'nutrition_plan', N'voice')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_food_scans_confidence",
                table: "food_scans",
                sql: "[confidence] IS NULL OR [confidence] IN (N'low', N'medium', N'high')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_exercises_category",
                table: "exercises",
                sql: "[category] IS NULL OR [category] IN (N'compound', N'isolation', N'cardio', N'stretching')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_exercises_muscle_group",
                table: "exercises",
                sql: "[muscle_group] IS NULL OR [muscle_group] IN (N'chest', N'back', N'shoulders', N'arms', N'legs', N'core', N'full_body', N'cardio')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_daily_summary_mood",
                table: "daily_summary",
                sql: "[mood] IS NULL OR ([mood] >= 1 AND [mood] <= 5)");

            migrationBuilder.CreateIndex(
                name: "IX_coaches_user_id",
                table: "coaches",
                column: "user_id",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_coach_profiles_rating",
                table: "coach_profiles",
                sql: "[rating] IS NULL OR ([rating] >= 0 AND [rating] <= 5)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_coach_content_type",
                table: "coach_content",
                sql: "[type] IN (N'pdf', N'video', N'image')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_barcode_products_confidence",
                table: "barcode_products",
                sql: "[confidence] IS NULL OR [confidence] IN (N'low', N'medium', N'high')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_barcode_products_source",
                table: "barcode_products",
                sql: "[source] IN (N'openfoodfacts', N'gemini_estimate', N'manual')");

            migrationBuilder.AddForeignKey(
                name: "FK_notifications_coach_profiles",
                table: "notifications",
                column: "coach_id",
                principalTable: "profiles",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_notifications_coach_profiles",
                table: "notifications");

            migrationBuilder.DropCheckConstraint(
                name: "CK_workout_sessions_muscle_group",
                table: "workout_sessions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_weekly_activity_actual_pct",
                table: "weekly_activity");

            migrationBuilder.DropCheckConstraint(
                name: "CK_weekly_activity_day_index",
                table: "weekly_activity");

            migrationBuilder.DropCheckConstraint(
                name: "CK_weekly_activity_goal_pct",
                table: "weekly_activity");

            migrationBuilder.DropCheckConstraint(
                name: "CK_voice_food_logs_confidence",
                table: "voice_food_logs");

            migrationBuilder.DropCheckConstraint(
                name: "CK_user_programs_muscle_group",
                table: "user_programs");

            migrationBuilder.DropIndex(
                name: "IX_user_goals_user_id",
                table: "user_goals");

            migrationBuilder.DropIndex(
                name: "IX_user_active_program_user_id",
                table: "user_active_program");

            migrationBuilder.DropCheckConstraint(
                name: "CK_training_programs_goal",
                table: "training_programs");

            migrationBuilder.DropCheckConstraint(
                name: "CK_training_programs_level",
                table: "training_programs");

            migrationBuilder.DropCheckConstraint(
                name: "CK_subscriptions_payment_status",
                table: "subscriptions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_subscriptions_tier",
                table: "subscriptions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_subscription_phases_status",
                table: "subscription_phases");

            migrationBuilder.DropCheckConstraint(
                name: "CK_subscription_phases_type",
                table: "subscription_phases");

            migrationBuilder.DropIndex(
                name: "IX_stripe_customers_stripe_customer_id",
                table: "stripe_customers");

            migrationBuilder.DropIndex(
                name: "IX_stripe_customers_user_id",
                table: "stripe_customers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_reviews_rating",
                table: "reviews");

            migrationBuilder.DropCheckConstraint(
                name: "CK_profiles_fitness_goal",
                table: "profiles");

            migrationBuilder.DropCheckConstraint(
                name: "CK_profiles_gender",
                table: "profiles");

            migrationBuilder.DropCheckConstraint(
                name: "CK_payment_intents_status",
                table: "payment_intents");

            migrationBuilder.DropIndex(
                name: "IX_onboarding_user_id",
                table: "onboarding");

            migrationBuilder.DropCheckConstraint(
                name: "CK_onboarding_activity_level",
                table: "onboarding");

            migrationBuilder.DropCheckConstraint(
                name: "CK_onboarding_gender",
                table: "onboarding");

            migrationBuilder.DropCheckConstraint(
                name: "CK_onboarding_goal",
                table: "onboarding");

            migrationBuilder.DropCheckConstraint(
                name: "CK_onboarding_weekly_workouts",
                table: "onboarding");

            migrationBuilder.DropCheckConstraint(
                name: "CK_nutrition_logs_meal_type",
                table: "nutrition_logs");

            migrationBuilder.DropIndex(
                name: "IX_notifications_coach_id",
                table: "notifications");

            migrationBuilder.DropCheckConstraint(
                name: "CK_notifications_type",
                table: "notifications");

            migrationBuilder.DropCheckConstraint(
                name: "CK_messages_type",
                table: "messages");

            migrationBuilder.DropCheckConstraint(
                name: "CK_food_scans_confidence",
                table: "food_scans");

            migrationBuilder.DropCheckConstraint(
                name: "CK_exercises_category",
                table: "exercises");

            migrationBuilder.DropCheckConstraint(
                name: "CK_exercises_muscle_group",
                table: "exercises");

            migrationBuilder.DropCheckConstraint(
                name: "CK_daily_summary_mood",
                table: "daily_summary");

            migrationBuilder.DropIndex(
                name: "IX_coaches_user_id",
                table: "coaches");

            migrationBuilder.DropCheckConstraint(
                name: "CK_coach_profiles_rating",
                table: "coach_profiles");

            migrationBuilder.DropCheckConstraint(
                name: "CK_coach_content_type",
                table: "coach_content");

            migrationBuilder.DropCheckConstraint(
                name: "CK_barcode_products_confidence",
                table: "barcode_products");

            migrationBuilder.DropCheckConstraint(
                name: "CK_barcode_products_source",
                table: "barcode_products");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "logged_at",
                table: "workout_sets",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldDefaultValueSql: "(SYSDATETIMEOFFSET())");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "started_at",
                table: "workout_sessions",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldDefaultValueSql: "(SYSDATETIMEOFFSET())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "session_date",
                table: "workout_sessions",
                type: "date",
                nullable: true,
                defaultValueSql: "(CAST(SYSUTCDATETIME() AS date))",
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldDefaultValueSql: "(CAST(SYSUTCDATETIME() AS date))");

            migrationBuilder.AlterColumn<decimal>(
                name: "goal_pct",
                table: "weekly_activity",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true,
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<decimal>(
                name: "actual_pct",
                table: "weekly_activity",
                type: "decimal(5,2)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true,
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "logged_at",
                table: "voice_food_logs",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldDefaultValueSql: "(SYSDATETIMEOFFSET())");

            migrationBuilder.AlterColumn<int>(
                name: "freeze_available",
                table: "user_streaks",
                type: "int",
                nullable: true,
                defaultValueSql: "((1))",
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValueSql: "((1))");

            migrationBuilder.AlterColumn<string>(
                name: "tier",
                table: "subscriptions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                defaultValueSql: "(N'basic')",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldDefaultValueSql: "(N'basic')");

            migrationBuilder.AlterColumn<DateTime>(
                name: "start_date",
                table: "subscriptions",
                type: "date",
                nullable: true,
                defaultValueSql: "(CAST(SYSUTCDATETIME() AS date))",
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldDefaultValueSql: "(CAST(SYSUTCDATETIME() AS date))");

            migrationBuilder.AlterColumn<string>(
                name: "tier",
                table: "payment_intents",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                defaultValueSql: "(N'standard')",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldDefaultValueSql: "(N'standard')");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "payment_intents",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                defaultValueSql: "(N'pending')",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldDefaultValueSql: "(N'pending')");

            migrationBuilder.AlterColumn<string>(
                name: "currency",
                table: "payment_intents",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                defaultValueSql: "(N'usd')",
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldDefaultValueSql: "(N'usd')");

            migrationBuilder.AlterColumn<DateTime>(
                name: "logged_date",
                table: "nutrition_logs",
                type: "date",
                nullable: true,
                defaultValueSql: "(CAST(SYSUTCDATETIME() AS date))",
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldDefaultValueSql: "(CAST(SYSUTCDATETIME() AS date))");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "scanned_at",
                table: "food_scans",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldDefaultValueSql: "(SYSDATETIMEOFFSET())");

            migrationBuilder.AlterColumn<int>(
                name: "workout_duration",
                table: "daily_summary",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true,
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "workout_done",
                table: "daily_summary",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true,
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "water_ml",
                table: "daily_summary",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true,
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "steps",
                table: "daily_summary",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true,
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<decimal>(
                name: "protein_g",
                table: "daily_summary",
                type: "decimal(8,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(8,2)",
                oldNullable: true,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "fat_g",
                table: "daily_summary",
                type: "decimal(8,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(8,2)",
                oldNullable: true,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "carbs_g",
                table: "daily_summary",
                type: "decimal(8,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(8,2)",
                oldNullable: true,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "calories_consumed",
                table: "daily_summary",
                type: "decimal(8,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(8,2)",
                oldNullable: true,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<int>(
                name: "calories_burned",
                table: "daily_summary",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true,
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "active_minutes",
                table: "daily_summary",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true,
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "synced_at",
                table: "daily_activity",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldDefaultValueSql: "(SYSDATETIMEOFFSET())");

            migrationBuilder.AlterColumn<int>(
                name: "heart_rate_avg",
                table: "daily_activity",
                type: "int",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(6,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "exercise_minutes",
                table: "daily_activity",
                type: "int",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(6,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "activity_date",
                table: "daily_activity",
                type: "date",
                nullable: true,
                defaultValueSql: "(CAST(SYSUTCDATETIME() AS date))",
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldDefaultValueSql: "(CAST(SYSUTCDATETIME() AS date))");

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                table: "coaches",
                type: "bit",
                nullable: true,
                defaultValueSql: "((1))",
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValueSql: "((1))");

            migrationBuilder.AlterColumn<string>(
                name: "bio",
                table: "coaches",
                type: "nvarchar(max)",
                nullable: true,
                defaultValueSql: "(N'')",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldDefaultValueSql: "(N'')");

            migrationBuilder.AlterColumn<string>(
                name: "specialization",
                table: "coach_onboarding",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<decimal>(
                name: "price_premium",
                table: "coach_onboarding",
                type: "decimal(10,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "price_monthly",
                table: "coach_onboarding",
                type: "decimal(10,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<int>(
                name: "max_clients",
                table: "coach_onboarding",
                type: "int",
                nullable: true,
                defaultValueSql: "((10))",
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValueSql: "((10))");

            migrationBuilder.AlterColumn<string>(
                name: "languages",
                table: "coach_onboarding",
                type: "nvarchar(max)",
                nullable: true,
                defaultValueSql: "(N'[\"Arabic\",\"English\"]')",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldDefaultValueSql: "(N'[\"Arabic\",\"English\"]')");

            migrationBuilder.AlterColumn<string>(
                name: "display_name",
                table: "coach_onboarding",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldDefaultValueSql: "(N'')");

            migrationBuilder.AlterColumn<string>(
                name: "certifications",
                table: "coach_onboarding",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "bio",
                table: "coach_onboarding",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldDefaultValueSql: "(N'')");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "assigned_at",
                table: "client_assignments",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldDefaultValueSql: "(SYSDATETIMEOFFSET())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "measured_date",
                table: "body_measurements",
                type: "date",
                nullable: true,
                defaultValueSql: "(CAST(SYSUTCDATETIME() AS date))",
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldDefaultValueSql: "(CAST(SYSUTCDATETIME() AS date))");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "scanned_at",
                table: "barcode_scan_history",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldDefaultValueSql: "(SYSDATETIMEOFFSET())");

            migrationBuilder.AlterColumn<string>(
                name: "barcode",
                table: "barcode_scan_history",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "confidence",
                table: "barcode_products",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                defaultValueSql: "(N'high')",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldDefaultValueSql: "(N'high')");

            migrationBuilder.CreateIndex(
                name: "IX_user_goals_user_id",
                table: "user_goals",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_active_program_user_id",
                table: "user_active_program",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_stripe_customers_stripe_customer_id",
                table: "stripe_customers",
                column: "stripe_customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_stripe_customers_user_id",
                table: "stripe_customers",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_onboarding_user_id",
                table: "onboarding",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_plan_id",
                table: "notifications",
                column: "plan_id");

            migrationBuilder.AddCheckConstraint(
                name: "CK_messages_type",
                table: "messages",
                sql: "[type] IN (N'text', N'voice', N'image', N'file')");

            migrationBuilder.CreateIndex(
                name: "IX_coaches_user_id",
                table: "coaches",
                column: "user_id");

            migrationBuilder.AddCheckConstraint(
                name: "CK_barcode_products_source",
                table: "barcode_products",
                sql: "[source] IN (N'openfoodfacts', N'gemini_estimate')");

            migrationBuilder.AddForeignKey(
                name: "FK_barcode_scan_history_barcode_products",
                table: "barcode_scan_history",
                column: "barcode",
                principalTable: "barcode_products",
                principalColumn: "barcode",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_notifications_subscription_plans",
                table: "notifications",
                column: "plan_id",
                principalTable: "subscription_plans",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
