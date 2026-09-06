using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace CoreGym.Infrastructure.Tests;

/// <summary>
/// Verifies the physical schema in SQL Server (columns, types, nullability,
/// indexes, foreign keys) against the Supabase inventory for Groups 1 and 2.
/// </summary>
[Collection("sql-smoke")]
public class SchemaShapeTests
{
    private readonly SqlServerSmokeFixture _fx;

    public SchemaShapeTests(SqlServerSmokeFixture fx) => _fx = fx;

    // Expected shape, transcribed from coregym-migration-inventory.md §1
    // (value formats: SQL Server type + "NULL"/"NOT NULL").
    public static readonly Dictionary<string, Dictionary<string, string>> Expected = new()
    {
        ["profiles"] = new()
        {
            ["id"] = "uniqueidentifier NOT NULL",
            ["name"] = "nvarchar(200) NULL",
            ["email"] = "nvarchar(320) NULL",
            ["gender"] = "nvarchar(50) NULL",
            ["age"] = "int NULL",
            ["weight_kg"] = "decimal(5,2) NULL",
            ["height_cm"] = "decimal(5,1) NULL",
            ["fitness_goal"] = "nvarchar(200) NULL",
            ["avatar_url"] = "nvarchar(500) NULL",
            ["created_at"] = "datetimeoffset NULL",
            ["updated_at"] = "datetimeoffset NULL",
            ["role"] = "nvarchar(20) NOT NULL",
            ["full_name"] = "nvarchar(200) NULL",
        },
        ["foods"] = new()
        {
            ["id"] = "uniqueidentifier NOT NULL",
            ["name"] = "nvarchar(200) NOT NULL",
            ["name_ar"] = "nvarchar(200) NULL",
            ["calories"] = "decimal(8,2) NOT NULL",
            ["protein_g"] = "decimal(8,2) NOT NULL",
            ["carbs_g"] = "decimal(8,2) NOT NULL",
            ["fat_g"] = "decimal(8,2) NOT NULL",
            ["fiber_g"] = "decimal(8,2) NOT NULL",
            ["serving_size"] = "decimal(6,2) NULL",
            ["serving_unit"] = "nvarchar(50) NULL",
            ["is_custom"] = "bit NOT NULL",
            ["created_by"] = "uniqueidentifier NULL",
            ["created_at"] = "datetimeoffset NULL",
            ["category"] = "nvarchar(50) NULL",
            ["image_url"] = "nvarchar(500) NULL",
        },
        ["exercises"] = new()
        {
            ["id"] = "uniqueidentifier NOT NULL",
            ["name"] = "nvarchar(200) NOT NULL",
            ["name_ar"] = "nvarchar(200) NULL",
            ["muscle_group"] = "nvarchar(50) NULL",
            ["secondary_muscles"] = "nvarchar(max) NULL",
            ["equipment"] = "nvarchar(50) NULL",
            ["category"] = "nvarchar(50) NULL",
            ["instructions"] = "nvarchar(max) NULL",
            ["instructions_ar"] = "nvarchar(max) NULL",
            ["tips"] = "nvarchar(max) NULL",
            ["created_at"] = "datetimeoffset NULL",
            ["image_url"] = "nvarchar(500) NULL",
            ["youtube_video_id"] = "nvarchar(50) NULL",
            ["gif_url"] = "nvarchar(500) NULL",
        },
        ["training_programs"] = new()
        {
            ["id"] = "uniqueidentifier NOT NULL",
            ["name"] = "nvarchar(200) NOT NULL",
            ["name_ar"] = "nvarchar(200) NULL",
            ["description"] = "nvarchar(max) NULL",
            ["description_ar"] = "nvarchar(max) NULL",
            ["level"] = "nvarchar(50) NULL",
            ["goal"] = "nvarchar(50) NULL",
            ["days_per_week"] = "int NULL",
            ["duration_weeks"] = "int NULL",
            ["split_type"] = "nvarchar(50) NULL",
            ["is_active"] = "bit NULL",
            ["created_at"] = "datetimeoffset NULL",
        },
        ["program_days"] = new()
        {
            ["id"] = "uniqueidentifier NOT NULL",
            ["program_id"] = "uniqueidentifier NOT NULL",
            ["day_number"] = "int NOT NULL",
            ["name"] = "nvarchar(200) NOT NULL",
            ["name_ar"] = "nvarchar(200) NULL",
            ["muscle_groups"] = "nvarchar(max) NULL",
            ["notes"] = "nvarchar(max) NULL",
        },
        ["program_day_exercises"] = new()
        {
            ["id"] = "uniqueidentifier NOT NULL",
            ["program_day_id"] = "uniqueidentifier NOT NULL",
            ["exercise_id"] = "uniqueidentifier NULL",
            ["order_index"] = "int NOT NULL",
            ["sets"] = "int NULL",
            ["reps_min"] = "int NULL",
            ["reps_max"] = "int NULL",
            ["rest_seconds"] = "int NULL",
            ["notes"] = "nvarchar(max) NULL",
            ["notes_ar"] = "nvarchar(max) NULL",
            ["is_main_lift"] = "bit NOT NULL",
        },
        ["onboarding"] = new()
        {
            ["id"] = "uniqueidentifier NOT NULL",
            ["user_id"] = "uniqueidentifier NOT NULL",
            ["age"] = "int NULL",
            ["gender"] = "nvarchar(50) NULL",
            ["height_cm"] = "decimal(5,1) NULL",
            ["weight_kg"] = "decimal(5,2) NULL",
            ["goal"] = "nvarchar(50) NULL",
            ["activity_level"] = "nvarchar(50) NULL",
            ["target_weight"] = "decimal(5,2) NULL",
            ["weekly_workouts"] = "int NULL",
            ["completed"] = "bit NOT NULL",
            ["created_at"] = "datetimeoffset NULL",
            ["updated_at"] = "datetimeoffset NULL",
        },
        ["user_goals"] = new()
        {
            ["id"] = "uniqueidentifier NOT NULL",
            ["user_id"] = "uniqueidentifier NOT NULL",
            ["daily_calories"] = "int NULL",
            ["daily_protein_g"] = "int NULL",
            ["daily_carbs_g"] = "int NULL",
            ["daily_fat_g"] = "int NULL",
            ["daily_water_ml"] = "int NULL",
            ["daily_steps"] = "int NULL",
            ["daily_sleep_hours"] = "decimal(4,2) NULL",
            ["weekly_workouts"] = "int NULL",
            ["target_weight_kg"] = "decimal(5,2) NULL",
            ["created_at"] = "datetimeoffset NULL",
            ["updated_at"] = "datetimeoffset NULL",
        },
        ["user_streaks"] = new()
        {
            ["user_id"] = "uniqueidentifier NOT NULL",
            ["current_streak"] = "int NOT NULL",
            ["longest_streak"] = "int NOT NULL",
            ["last_active_date"] = "date NULL",
            ["freeze_available"] = "int NULL",
            ["updated_at"] = "datetimeoffset NULL",
        },
        ["body_measurements"] = new()
        {
            ["id"] = "uniqueidentifier NOT NULL",
            ["user_id"] = "uniqueidentifier NOT NULL",
            ["weight_kg"] = "decimal(5,2) NULL",
            ["body_fat_pct"] = "decimal(5,2) NULL",
            ["muscle_mass"] = "decimal(5,2) NULL",
            ["chest_cm"] = "decimal(5,1) NULL",
            ["waist_cm"] = "decimal(5,1) NULL",
            ["hips_cm"] = "decimal(5,1) NULL",
            ["arms_cm"] = "decimal(5,1) NULL",
            ["thighs_cm"] = "decimal(5,1) NULL",
            ["measured_date"] = "date NULL",
            ["notes"] = "nvarchar(max) NULL",
            ["created_at"] = "datetimeoffset NULL",
        },
        ["nutrition_logs"] = new()
        {
            ["id"] = "uniqueidentifier NOT NULL",
            ["user_id"] = "uniqueidentifier NOT NULL",
            ["food_id"] = "uniqueidentifier NULL",
            ["food_name"] = "nvarchar(200) NOT NULL",
            ["meal_type"] = "nvarchar(50) NULL",
            ["quantity"] = "decimal(8,2) NOT NULL",
            ["serving_unit"] = "nvarchar(50) NULL",
            ["calories"] = "decimal(8,2) NOT NULL",
            ["protein_g"] = "decimal(8,2) NOT NULL",
            ["carbs_g"] = "decimal(8,2) NOT NULL",
            ["fat_g"] = "decimal(8,2) NOT NULL",
            ["logged_date"] = "date NULL",
            ["logged_at"] = "datetimeoffset NULL",
        },
        ["daily_summary"] = new()
        {
            ["id"] = "uniqueidentifier NOT NULL",
            ["user_id"] = "uniqueidentifier NOT NULL",
            ["summary_date"] = "date NULL",
            ["steps"] = "int NULL",
            ["active_minutes"] = "int NULL",
            ["calories_burned"] = "int NULL",
            ["water_ml"] = "int NULL",
            ["sleep_hours"] = "decimal(4,2) NULL",
            ["calories_consumed"] = "decimal(8,2) NULL",
            ["protein_g"] = "decimal(8,2) NULL",
            ["carbs_g"] = "decimal(8,2) NULL",
            ["fat_g"] = "decimal(8,2) NULL",
            ["workout_done"] = "bit NULL",
            ["workout_duration"] = "int NULL",
            ["mood"] = "int NULL",
            ["notes"] = "nvarchar(max) NULL",
            ["created_at"] = "datetimeoffset NULL",
            ["updated_at"] = "datetimeoffset NULL",
        },
        ["daily_activity"] = new()
        {
            ["id"] = "uniqueidentifier NOT NULL",
            ["user_id"] = "uniqueidentifier NOT NULL",
            ["activity_date"] = "date NULL",
            ["steps"] = "int NOT NULL",
            ["active_calories_burned"] = "decimal(8,2) NOT NULL",
            ["heart_rate_avg"] = "int NULL",
            ["exercise_minutes"] = "int NULL",
            ["source"] = "nvarchar(50) NULL",
            ["synced_at"] = "datetimeoffset NULL",
            ["created_at"] = "datetimeoffset NULL",
        },
        ["streak_activity_log"] = new()
        {
            ["id"] = "uniqueidentifier NOT NULL",
            ["user_id"] = "uniqueidentifier NOT NULL",
            ["activity_date"] = "date NOT NULL",
            ["source"] = "nvarchar(50) NOT NULL",
            ["created_at"] = "datetimeoffset NULL",
        },
        ["workout_sessions"] = new()
        {
            ["id"] = "uniqueidentifier NOT NULL",
            ["user_id"] = "uniqueidentifier NOT NULL",
            ["muscle_group"] = "nvarchar(50) NOT NULL",
            ["session_name"] = "nvarchar(200) NULL",
            ["duration_min"] = "int NOT NULL",
            ["notes"] = "nvarchar(max) NULL",
            ["session_date"] = "date NULL",
            ["started_at"] = "datetimeoffset NULL",
            ["ended_at"] = "datetimeoffset NULL",
        },
        ["workout_sets"] = new()
        {
            ["id"] = "uniqueidentifier NOT NULL",
            ["session_id"] = "uniqueidentifier NULL",
            ["user_id"] = "uniqueidentifier NOT NULL",
            ["exercise_name"] = "nvarchar(200) NOT NULL",
            ["set_number"] = "int NOT NULL",
            ["reps"] = "int NULL",
            ["weight_kg"] = "decimal(5,2) NULL",
            ["duration_sec"] = "int NULL",
            ["rest_sec"] = "int NULL",
            ["is_warmup"] = "bit NOT NULL",
            ["logged_at"] = "datetimeoffset NULL",
        },
        ["exercise_progress"] = new()
        {
            ["id"] = "uniqueidentifier NOT NULL",
            ["user_id"] = "uniqueidentifier NOT NULL",
            ["exercise_id"] = "uniqueidentifier NULL",
            ["session_date"] = "date NULL",
            ["best_set_weight"] = "decimal(5,2) NULL",
            ["best_set_reps"] = "int NULL",
            ["total_volume"] = "decimal(10,2) NULL",
            ["one_rm_estimate"] = "decimal(6,2) NULL",
            ["session_id"] = "uniqueidentifier NULL",
            ["created_at"] = "datetimeoffset NULL",
        },
        ["user_active_program"] = new()
        {
            ["id"] = "uniqueidentifier NOT NULL",
            ["user_id"] = "uniqueidentifier NOT NULL",
            ["program_id"] = "uniqueidentifier NOT NULL",
            ["started_at"] = "datetimeoffset NULL",
            ["current_week"] = "int NULL",
            ["current_day"] = "int NULL",
            ["notes"] = "nvarchar(max) NULL",
            ["updated_at"] = "datetimeoffset NULL",
        },
        ["user_programs"] = new()
        {
            ["id"] = "uniqueidentifier NOT NULL",
            ["user_id"] = "uniqueidentifier NOT NULL",
            ["program_name"] = "nvarchar(200) NOT NULL",
            ["muscle_group"] = "nvarchar(50) NOT NULL",
            ["is_active"] = "bit NULL",
            ["started_at"] = "datetimeoffset NULL",
        },
        ["barcode_products"] = new()
        {
            ["barcode"] = "nvarchar(50) NOT NULL",
            ["product_name"] = "nvarchar(200) NOT NULL",
            ["product_name_ar"] = "nvarchar(200) NULL",
            ["brand"] = "nvarchar(200) NULL",
            ["serving_size_g"] = "decimal(6,2) NULL",
            ["calories"] = "decimal(8,2) NOT NULL",
            ["protein_g"] = "decimal(8,2) NOT NULL",
            ["carbs_g"] = "decimal(8,2) NOT NULL",
            ["fat_g"] = "decimal(8,2) NOT NULL",
            ["source"] = "nvarchar(50) NOT NULL",
            ["confidence"] = "nvarchar(50) NULL",
            ["lookup_count"] = "int NULL",
            ["created_at"] = "datetimeoffset NULL",
            ["updated_at"] = "datetimeoffset NULL",
        },
        ["barcode_scan_history"] = new()
        {
            ["id"] = "uniqueidentifier NOT NULL",
            ["user_id"] = "uniqueidentifier NOT NULL",
            ["barcode"] = "nvarchar(50) NOT NULL",
            ["quantity_g"] = "decimal(8,2) NULL",
            ["nutrition_log_id"] = "uniqueidentifier NULL",
            ["scanned_at"] = "datetimeoffset NULL",
        },
        ["food_scans"] = new()
        {
            ["id"] = "uniqueidentifier NOT NULL",
            ["user_id"] = "uniqueidentifier NOT NULL",
            ["image_path"] = "nvarchar(500) NULL",
            ["is_food"] = "bit NULL",
            ["confidence"] = "nvarchar(50) NULL",
            ["notes"] = "nvarchar(max) NULL",
            ["scanned_at"] = "datetimeoffset NULL",
            ["created_at"] = "datetimeoffset NULL",
        },
        ["food_scan_items"] = new()
        {
            ["id"] = "uniqueidentifier NOT NULL",
            ["scan_id"] = "uniqueidentifier NOT NULL",
            ["name"] = "nvarchar(200) NOT NULL",
            ["name_ar"] = "nvarchar(200) NULL",
            ["estimated_weight_g"] = "decimal(8,2) NOT NULL",
            ["calories"] = "decimal(8,2) NOT NULL",
            ["protein_g"] = "decimal(8,2) NOT NULL",
            ["carbs_g"] = "decimal(8,2) NOT NULL",
            ["fat_g"] = "decimal(8,2) NOT NULL",
            ["nutrition_log_id"] = "uniqueidentifier NULL",
            ["created_at"] = "datetimeoffset NULL",
        },
        ["voice_food_logs"] = new()
        {
            ["id"] = "uniqueidentifier NOT NULL",
            ["user_id"] = "uniqueidentifier NOT NULL",
            ["audio_path"] = "nvarchar(500) NULL",
            ["transcript"] = "nvarchar(max) NULL",
            ["is_food"] = "bit NULL",
            ["confidence"] = "nvarchar(50) NULL",
            ["notes"] = "nvarchar(max) NULL",
            ["logged_at"] = "datetimeoffset NULL",
            ["created_at"] = "datetimeoffset NULL",
        },
        ["voice_food_log_items"] = new()
        {
            ["id"] = "uniqueidentifier NOT NULL",
            ["log_id"] = "uniqueidentifier NOT NULL",
            ["name"] = "nvarchar(200) NOT NULL",
            ["name_ar"] = "nvarchar(200) NULL",
            ["estimated_weight_g"] = "decimal(8,2) NOT NULL",
            ["calories"] = "decimal(8,2) NOT NULL",
            ["protein_g"] = "decimal(8,2) NOT NULL",
            ["carbs_g"] = "decimal(8,2) NOT NULL",
            ["fat_g"] = "decimal(8,2) NOT NULL",
            ["nutrition_log_id"] = "uniqueidentifier NULL",
            ["created_at"] = "datetimeoffset NULL",
        },
        ["weekly_activity"] = new()
        {
            ["id"] = "uniqueidentifier NOT NULL",
            ["user_id"] = "uniqueidentifier NOT NULL",
            ["week_start"] = "date NOT NULL",
            ["day_index"] = "int NOT NULL",
            ["actual_pct"] = "decimal(5,2) NULL",
            ["goal_pct"] = "decimal(5,2) NOT NULL",
        },
        ["coaches"] = new()
        {
            ["id"] = "uniqueidentifier NOT NULL",
            ["user_id"] = "uniqueidentifier NOT NULL",
            ["bio"] = "nvarchar(max) NULL",
            ["price_monthly"] = "decimal(10,2) NOT NULL",
            ["specialization"] = "nvarchar(max) NULL",
            ["rating"] = "decimal(3,2) NOT NULL",
            ["is_active"] = "bit NULL",
            ["stripe_account_id"] = "nvarchar(100) NULL",
            ["created_at"] = "datetimeoffset NULL",
            ["updated_at"] = "datetimeoffset NULL",
        },
        ["coach_profiles"] = new()
        {
            ["id"] = "uniqueidentifier NOT NULL",
            ["bio"] = "nvarchar(max) NULL",
            ["bio_ar"] = "nvarchar(max) NULL",
            ["specialties"] = "nvarchar(max) NULL",
            ["certifications"] = "nvarchar(max) NULL",
            ["experience_years"] = "int NOT NULL",
            ["price_per_month"] = "decimal(10,2) NOT NULL",
            ["currency"] = "nvarchar(10) NULL",
            ["rating"] = "decimal(3,2) NOT NULL",
            ["reviews_count"] = "int NOT NULL",
            ["is_available"] = "bit NULL",
            ["is_verified"] = "bit NOT NULL",
            ["cover_image_url"] = "nvarchar(500) NULL",
            ["instagram_url"] = "nvarchar(500) NULL",
            ["youtube_url"] = "nvarchar(500) NULL",
            ["max_clients"] = "int NULL",
            ["current_clients"] = "int NOT NULL",
            ["created_at"] = "datetimeoffset NULL",
            ["updated_at"] = "datetimeoffset NULL",
        },
        ["coach_onboarding"] = new()
        {
            ["id"] = "uniqueidentifier NOT NULL",
            ["user_id"] = "uniqueidentifier NOT NULL",
            ["display_name"] = "nvarchar(200) NULL",
            ["years_experience"] = "int NOT NULL",
            ["certifications"] = "nvarchar(max) NULL",
            ["specialization"] = "nvarchar(max) NULL",
            ["bio"] = "nvarchar(max) NULL",
            ["price_monthly"] = "decimal(10,2) NULL",
            ["price_premium"] = "decimal(10,2) NULL",
            ["languages"] = "nvarchar(max) NULL",
            ["max_clients"] = "int NULL",
            ["profile_image_url"] = "nvarchar(500) NULL",
            ["intro_video_url"] = "nvarchar(500) NULL",
            ["is_completed"] = "bit NOT NULL",
            ["created_at"] = "datetimeoffset NULL",
            ["updated_at"] = "datetimeoffset NULL",
            ["phone_number"] = "nvarchar(30) NULL",
            ["city"] = "nvarchar(100) NULL",
            ["gender"] = "nvarchar(50) NULL",
            ["gallery_images"] = "nvarchar(max) NULL",
            ["pdf_urls"] = "nvarchar(max) NULL",
            ["certificate_files"] = "nvarchar(max) NULL",
            ["transformation_images"] = "nvarchar(max) NULL",
        },
        ["coach_content"] = new()
        {
            ["id"] = "uniqueidentifier NOT NULL",
            ["coach_id"] = "uniqueidentifier NOT NULL",
            ["title"] = "nvarchar(200) NOT NULL",
            ["description"] = "nvarchar(max) NULL",
            ["type"] = "nvarchar(50) NOT NULL",
            ["file_url"] = "nvarchar(500) NOT NULL",
            ["is_public"] = "bit NOT NULL",
            ["created_at"] = "datetimeoffset NULL",
            ["thumbnail_url"] = "nvarchar(500) NULL",
            ["file_size_kb"] = "int NULL",
            ["sort_order"] = "int NOT NULL",
        },
        ["client_assignments"] = new()
        {
            ["id"] = "uniqueidentifier NOT NULL",
            ["coach_id"] = "uniqueidentifier NOT NULL",
            ["client_id"] = "uniqueidentifier NOT NULL",
            ["content_id"] = "uniqueidentifier NOT NULL",
            ["note"] = "nvarchar(max) NULL",
            ["assigned_at"] = "datetimeoffset NULL",
        },
        ["reviews"] = new()
        {
            ["id"] = "uniqueidentifier NOT NULL",
            ["client_id"] = "uniqueidentifier NOT NULL",
            ["coach_id"] = "uniqueidentifier NOT NULL",
            ["rating"] = "int NOT NULL",
            ["comment"] = "nvarchar(max) NULL",
            ["created_at"] = "datetimeoffset NULL",
        },
        ["subscriptions"] = new()
        {
            ["id"] = "uniqueidentifier NOT NULL",
            ["client_id"] = "uniqueidentifier NOT NULL",
            ["coach_id"] = "uniqueidentifier NOT NULL",
            ["status"] = "nvarchar(20) NOT NULL",
            ["tier"] = "nvarchar(50) NULL",
            ["start_date"] = "date NULL",
            ["end_date"] = "date NULL",
            ["stripe_sub_id"] = "nvarchar(100) NULL",
            ["created_at"] = "datetimeoffset NULL",
            ["updated_at"] = "datetimeoffset NULL",
            ["plan_id"] = "uniqueidentifier NULL",
            ["payment_status"] = "nvarchar(50) NULL",
            ["started_at"] = "datetimeoffset NULL",
            ["expires_at"] = "datetimeoffset NULL",
            ["goals"] = "nvarchar(max) NULL",
            ["notes"] = "nvarchar(max) NULL",
        },
        ["subscription_plans"] = new()
        {
            ["id"] = "uniqueidentifier NOT NULL",
            ["coach_id"] = "uniqueidentifier NOT NULL",
            ["name"] = "nvarchar(200) NOT NULL",
            ["price_usd"] = "decimal(10,2) NULL",
            ["duration_days"] = "int NULL",
            ["max_clients"] = "int NULL",
            ["created_at"] = "datetimeoffset NULL",
        },
        ["subscription_phases"] = new()
        {
            ["id"] = "uniqueidentifier NOT NULL",
            ["subscription_id"] = "uniqueidentifier NOT NULL",
            ["phase_number"] = "int NOT NULL",
            ["title"] = "nvarchar(200) NOT NULL",
            ["type"] = "nvarchar(50) NULL",
            ["description"] = "nvarchar(max) NULL",
            ["duration_weeks"] = "int NULL",
            ["status"] = "nvarchar(50) NULL",
            ["started_at"] = "datetimeoffset NULL",
            ["completed_at"] = "datetimeoffset NULL",
            ["created_at"] = "datetimeoffset NULL",
        },
        ["payment_intents"] = new()
        {
            ["id"] = "uniqueidentifier NOT NULL",
            ["client_id"] = "uniqueidentifier NOT NULL",
            ["coach_id"] = "uniqueidentifier NOT NULL",
            ["stripe_payment_id"] = "nvarchar(100) NOT NULL",
            ["stripe_customer_id"] = "nvarchar(100) NULL",
            ["amount"] = "decimal(12,2) NOT NULL",
            ["currency"] = "nvarchar(10) NULL",
            ["status"] = "nvarchar(50) NULL",
            ["tier"] = "nvarchar(50) NULL",
            ["created_at"] = "datetimeoffset NULL",
        },
        ["stripe_customers"] = new()
        {
            ["id"] = "uniqueidentifier NOT NULL",
            ["user_id"] = "uniqueidentifier NOT NULL",
            ["stripe_customer_id"] = "nvarchar(100) NOT NULL",
            ["created_at"] = "datetimeoffset NULL",
        },
        ["conversations"] = new()
        {
            ["id"] = "uniqueidentifier NOT NULL",
            ["client_id"] = "uniqueidentifier NOT NULL",
            ["coach_id"] = "uniqueidentifier NOT NULL",
            ["subscription_id"] = "uniqueidentifier NULL",
            ["last_message"] = "nvarchar(max) NULL",
            ["last_message_at"] = "datetimeoffset NULL",
            ["client_unread"] = "int NOT NULL",
            ["coach_unread"] = "int NOT NULL",
            ["is_active"] = "bit NULL",
            ["created_at"] = "datetimeoffset NULL",
        },
        ["messages"] = new()
        {
            ["id"] = "uniqueidentifier NOT NULL",
            ["conversation_id"] = "uniqueidentifier NOT NULL",
            ["sender_id"] = "uniqueidentifier NOT NULL",
            ["content"] = "nvarchar(max) NOT NULL",
            ["type"] = "nvarchar(20) NULL",
            ["file_url"] = "nvarchar(500) NULL",
            ["is_read"] = "bit NOT NULL",
            ["is_deleted"] = "bit NOT NULL",
            ["created_at"] = "datetimeoffset NULL",
        },
        ["notifications"] = new()
        {
            ["id"] = "uniqueidentifier NOT NULL",
            ["user_id"] = "uniqueidentifier NOT NULL",
            ["type"] = "nvarchar(50) NOT NULL",
            ["title"] = "nvarchar(200) NOT NULL",
            ["body"] = "nvarchar(max) NOT NULL",
            ["conversation_id"] = "uniqueidentifier NULL",
            ["plan_id"] = "uniqueidentifier NULL",
            ["coach_id"] = "uniqueidentifier NULL",
            ["is_read"] = "bit NOT NULL",
            ["created_at"] = "datetimeoffset NULL",
        },
        ["notification_preferences"] = new()
        {
            ["user_id"] = "uniqueidentifier NOT NULL",
            ["meal_reminders_enabled"] = "bit NULL",
            ["water_reminders_enabled"] = "bit NULL",
            ["calorie_alerts_enabled"] = "bit NULL",
            ["chat_notifications_enabled"] = "bit NULL",
            ["quiet_hours_start"] = "time NULL",
            ["quiet_hours_end"] = "time NULL",
            ["updated_at"] = "datetimeoffset NULL",
        },
        ["notification_log"] = new()
        {
            ["id"] = "uniqueidentifier NOT NULL",
            ["user_id"] = "uniqueidentifier NOT NULL",
            ["type"] = "nvarchar(50) NOT NULL",
            ["title"] = "nvarchar(200) NOT NULL",
            ["body"] = "nvarchar(max) NOT NULL",
            ["data"] = "nvarchar(max) NULL",
            ["sent_at"] = "datetimeoffset NULL",
            ["read_at"] = "datetimeoffset NULL",
        },
    };

    public static readonly string[] ExpectedIndexes =
    [
        "IX_foods_created_by",
        "IX_program_days_program_id",
        "IX_program_day_exercises_program_day_id_order_index",
        "IX_program_day_exercises_exercise_id",
        "IX_onboarding_user_id",
        "IX_user_goals_user_id",
        "IX_body_measurements_user_id_measured_date",
        "IX_nutrition_logs_user_id_logged_date",
        "IX_nutrition_logs_food_id",
        "IX_daily_summary_user_id_summary_date",
        "IX_daily_activity_user_id_activity_date",
        "IX_streak_activity_log_user_id_activity_date",
        "IX_workout_sessions_user_id_session_date",
        "IX_workout_sets_session_id",
        "IX_workout_sets_user_id_exercise_name",
        "IX_exercise_progress_user_id_session_date",
        "IX_exercise_progress_exercise_id",
        "IX_exercise_progress_session_id",
        "IX_user_active_program_user_id",
        "IX_user_programs_user_id",
        "IX_barcode_scan_history_user_id",
        "IX_barcode_scan_history_barcode",
        "IX_barcode_scan_history_nutrition_log_id",
        "IX_food_scans_user_id",
        "IX_food_scan_items_scan_id",
        "IX_food_scan_items_nutrition_log_id",
        "IX_voice_food_logs_user_id",
        "IX_voice_food_log_items_log_id",
        "IX_voice_food_log_items_nutrition_log_id",
        "IX_weekly_activity_user_id_week_start",
        "IX_coaches_user_id",
        "IX_coach_onboarding_user_id",
        "IX_coach_content_coach_id",
        "IX_client_assignments_client_id",
        "IX_client_assignments_coach_id",
        "IX_client_assignments_content_id",
        "IX_reviews_coach_id",
        "IX_reviews_client_id",
        "IX_subscriptions_client_id_coach_id",
        "IX_subscriptions_coach_id",
        "IX_subscriptions_plan_id",
        "IX_subscription_plans_coach_id",
        "IX_subscription_phases_subscription_id",
        "IX_payment_intents_client_id",
        "IX_payment_intents_coach_id",
        "IX_payment_intents_stripe_payment_id",
        "IX_stripe_customers_user_id",
        "IX_stripe_customers_stripe_customer_id",
        "IX_conversations_client_id",
        "IX_conversations_coach_id",
        "IX_conversations_subscription_id",
        "IX_messages_conversation_id_created_at",
        "IX_messages_sender_id",
        "IX_notifications_user_id_created_at",
        "IX_notifications_conversation_id",
        "IX_notifications_plan_id",
        "IX_notification_log_user_id_sent_at",
    ];

    public static readonly string[] ExpectedForeignKeys =
    [
        "FK_foods_profiles",
        "FK_program_days_training_programs",
        "FK_program_day_exercises_exercises",
        "FK_program_day_exercises_program_days",
        "FK_onboarding_profiles",
        "FK_user_goals_profiles",
        "FK_user_streaks_profiles",
        "FK_body_measurements_profiles",
        "FK_nutrition_logs_profiles",
        "FK_nutrition_logs_foods",
        "FK_daily_summary_profiles",
        "FK_daily_activity_profiles",
        "FK_streak_activity_log_profiles",
        "FK_workout_sessions_profiles",
        "FK_workout_sets_workout_sessions",
        "FK_workout_sets_profiles",
        "FK_exercise_progress_profiles",
        "FK_exercise_progress_exercises",
        "FK_exercise_progress_workout_sessions",
        "FK_user_active_program_profiles",
        "FK_user_active_program_training_programs",
        "FK_user_programs_profiles",
        "FK_barcode_scan_history_profiles",
        "FK_barcode_scan_history_barcode_products",
        "FK_barcode_scan_history_nutrition_logs",
        "FK_food_scans_profiles",
        "FK_food_scan_items_food_scans",
        "FK_food_scan_items_nutrition_logs",
        "FK_voice_food_logs_profiles",
        "FK_voice_food_log_items_voice_food_logs",
        "FK_voice_food_log_items_nutrition_logs",
        "FK_weekly_activity_profiles",
        "FK_coaches_profiles",
        "FK_coach_profiles_profiles",
        "FK_coach_onboarding_profiles",
        "FK_coach_content_coaches",
        "FK_client_assignments_coaches",
        "FK_client_assignments_profiles",
        "FK_client_assignments_coach_content",
        "FK_reviews_profiles",
        "FK_reviews_coaches",
        "FK_subscriptions_profiles",
        "FK_subscriptions_coaches",
        "FK_subscriptions_subscription_plans",
        "FK_subscription_plans_profiles",
        "FK_subscription_phases_subscriptions",
        "FK_payment_intents_profiles",
        "FK_payment_intents_coaches",
        "FK_stripe_customers_profiles",
        "FK_conversations_profiles",
        "FK_conversations_coach_profiles",
        "FK_conversations_subscriptions",
        "FK_messages_conversations",
        "FK_messages_profiles",
        "FK_notifications_profiles",
        "FK_notifications_conversations",
        "FK_notifications_subscription_plans",
        "FK_notification_preferences_profiles",
        "FK_notification_log_profiles",
    ];

    [Fact]
    public async Task All_tables_have_exactly_the_inventory_columns()
    {
        var actual = await ReadActualColumnsAsync();

        foreach (var (table, expectedColumns) in Expected)
        {
            Assert.True(actual.TryGetValue(table, out var actualColumns),
                $"table '{table}' does not exist in the database");

            foreach (var (column, expectedDescriptor) in expectedColumns)
            {
                Assert.True(actualColumns.TryGetValue(column, out var actualDescriptor),
                    $"column '{table}.{column}' is missing");

                Assert.True(actualDescriptor == expectedDescriptor,
                    $"column '{table}.{column}' is {actualDescriptor}, expected {expectedDescriptor}");
            }

            var unexpected = actualColumns.Keys.Except(expectedColumns.Keys).ToList();
            Assert.True(unexpected.Count == 0,
                $"table '{table}' has unexpected columns: {string.Join(", ", unexpected)}");
        }
    }

    [Fact]
    public async Task Performance_and_integrity_indexes_exist()
    {
        await using var ctx = _fx.CreateContext();
        var actualIndexes = await ctx.Database
            .SqlQuery<string>($"SELECT name AS Value FROM sys.indexes WHERE name IS NOT NULL")
            .ToListAsync();

        foreach (var index in ExpectedIndexes)
        {
            Assert.Contains(index, actualIndexes);
        }

        foreach (var table in Expected.Keys)
        {
            Assert.Contains($"PK_{table}", actualIndexes);
        }
    }

    [Fact]
    public async Task All_inferred_foreign_keys_exist()
    {
        await using var ctx = _fx.CreateContext();
        var actualFks = await ctx.Database
            .SqlQuery<string>($"SELECT name AS Value FROM sys.foreign_keys WHERE name IS NOT NULL")
            .ToListAsync();

        foreach (var fk in ExpectedForeignKeys)
        {
            Assert.Contains(fk, actualFks);
        }
    }

    private async Task<Dictionary<string, Dictionary<string, string>>> ReadActualColumnsAsync()
    {
        await using var ctx = _fx.CreateContext();
        var connection = (SqlConnection)ctx.Database.GetDbConnection();
        await connection.OpenAsync();

        var result = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE,
                   IS_NULLABLE, CHARACTER_MAXIMUM_LENGTH, NUMERIC_PRECISION, NUMERIC_SCALE
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_SCHEMA = 'dbo'
            """;

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var table = reader.GetString(0);
            var column = reader.GetString(1);
            var dataType = reader.GetString(2);
            var isNullable = reader.GetString(3) == "YES" ? "NULL" : "NOT NULL";
            var maxLen = reader.IsDBNull(4) ? (long?)null : reader.GetInt32(4);
            var precision = reader.IsDBNull(5) ? (byte?)null : reader.GetByte(5);
            var scale = reader.IsDBNull(6) ? (int?)null : reader.GetInt32(6);

            var type = dataType switch
            {
                "nvarchar" => maxLen == -1 ? "nvarchar(max)" : $"nvarchar({maxLen})",
                "decimal" => $"decimal({precision},{scale})",
                _ => dataType,
            };

            if (!result.TryGetValue(table, out var columns))
            {
                columns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                result[table] = columns;
            }

            columns[column] = $"{type} {isNullable}";
        }

        return result;
    }
}
