BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905173708_AddGroups3Schema'
)
BEGIN
    CREATE TABLE [barcode_products] (
        [barcode] nvarchar(50) NOT NULL,
        [product_name] nvarchar(200) NOT NULL,
        [product_name_ar] nvarchar(200) NULL,
        [brand] nvarchar(200) NULL,
        [serving_size_g] decimal(6,2) NULL,
        [calories] decimal(8,2) NOT NULL DEFAULT 0.0,
        [protein_g] decimal(8,2) NOT NULL DEFAULT 0.0,
        [carbs_g] decimal(8,2) NOT NULL DEFAULT 0.0,
        [fat_g] decimal(8,2) NOT NULL DEFAULT 0.0,
        [source] nvarchar(50) NOT NULL,
        [confidence] nvarchar(50) NULL DEFAULT ((N'high')),
        [lookup_count] int NULL DEFAULT (((1))),
        [created_at] datetimeoffset NULL DEFAULT ((SYSDATETIMEOFFSET())),
        [updated_at] datetimeoffset NULL DEFAULT ((SYSDATETIMEOFFSET())),
        CONSTRAINT [PK_barcode_products] PRIMARY KEY ([barcode]),
        CONSTRAINT [CK_barcode_products_source] CHECK ([source] IN (N'openfoodfacts', N'gemini_estimate'))
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905173708_AddGroups3Schema'
)
BEGIN
    CREATE TABLE [daily_activity] (
        [id] uniqueidentifier NOT NULL,
        [user_id] uniqueidentifier NOT NULL,
        [activity_date] date NULL DEFAULT ((CAST(SYSUTCDATETIME() AS date))),
        [steps] int NOT NULL DEFAULT 0,
        [active_calories_burned] decimal(8,2) NOT NULL DEFAULT 0.0,
        [heart_rate_avg] int NULL,
        [exercise_minutes] int NULL,
        [source] nvarchar(50) NULL DEFAULT ((N'health_connect')),
        [synced_at] datetimeoffset NULL,
        [created_at] datetimeoffset NULL DEFAULT ((SYSDATETIMEOFFSET())),
        CONSTRAINT [PK_daily_activity] PRIMARY KEY ([id]),
        CONSTRAINT [FK_daily_activity_profiles] FOREIGN KEY ([user_id]) REFERENCES [profiles] ([id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905173708_AddGroups3Schema'
)
BEGIN
    CREATE TABLE [daily_summary] (
        [id] uniqueidentifier NOT NULL,
        [user_id] uniqueidentifier NOT NULL,
        [summary_date] date NULL DEFAULT ((CAST(SYSUTCDATETIME() AS date))),
        [steps] int NULL,
        [active_minutes] int NULL,
        [calories_burned] int NULL,
        [water_ml] int NULL,
        [sleep_hours] decimal(4,2) NULL,
        [calories_consumed] decimal(8,2) NULL,
        [protein_g] decimal(8,2) NULL,
        [carbs_g] decimal(8,2) NULL,
        [fat_g] decimal(8,2) NULL,
        [workout_done] bit NULL,
        [workout_duration] int NULL,
        [mood] int NULL,
        [notes] nvarchar(max) NULL,
        [created_at] datetimeoffset NULL DEFAULT ((SYSDATETIMEOFFSET())),
        [updated_at] datetimeoffset NULL DEFAULT ((SYSDATETIMEOFFSET())),
        CONSTRAINT [PK_daily_summary] PRIMARY KEY ([id]),
        CONSTRAINT [FK_daily_summary_profiles] FOREIGN KEY ([user_id]) REFERENCES [profiles] ([id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905173708_AddGroups3Schema'
)
BEGIN
    CREATE TABLE [food_scans] (
        [id] uniqueidentifier NOT NULL,
        [user_id] uniqueidentifier NOT NULL,
        [image_path] nvarchar(500) NULL,
        [is_food] bit NULL DEFAULT (((1))),
        [confidence] nvarchar(50) NULL DEFAULT ((N'medium')),
        [notes] nvarchar(max) NULL,
        [scanned_at] datetimeoffset NULL,
        [created_at] datetimeoffset NULL DEFAULT ((SYSDATETIMEOFFSET())),
        CONSTRAINT [PK_food_scans] PRIMARY KEY ([id]),
        CONSTRAINT [FK_food_scans_profiles] FOREIGN KEY ([user_id]) REFERENCES [profiles] ([id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905173708_AddGroups3Schema'
)
BEGIN
    CREATE TABLE [nutrition_logs] (
        [id] uniqueidentifier NOT NULL,
        [user_id] uniqueidentifier NOT NULL,
        [food_id] uniqueidentifier NULL,
        [food_name] nvarchar(200) NOT NULL,
        [meal_type] nvarchar(50) NULL,
        [quantity] decimal(8,2) NOT NULL DEFAULT (((1))),
        [serving_unit] nvarchar(50) NULL DEFAULT ((N'g')),
        [calories] decimal(8,2) NOT NULL,
        [protein_g] decimal(8,2) NOT NULL DEFAULT 0.0,
        [carbs_g] decimal(8,2) NOT NULL DEFAULT 0.0,
        [fat_g] decimal(8,2) NOT NULL DEFAULT 0.0,
        [logged_date] date NULL DEFAULT ((CAST(SYSUTCDATETIME() AS date))),
        [logged_at] datetimeoffset NULL DEFAULT ((SYSDATETIMEOFFSET())),
        CONSTRAINT [PK_nutrition_logs] PRIMARY KEY ([id]),
        CONSTRAINT [FK_nutrition_logs_foods] FOREIGN KEY ([food_id]) REFERENCES [foods] ([id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_nutrition_logs_profiles] FOREIGN KEY ([user_id]) REFERENCES [profiles] ([id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905173708_AddGroups3Schema'
)
BEGIN
    CREATE TABLE [streak_activity_log] (
        [id] uniqueidentifier NOT NULL,
        [user_id] uniqueidentifier NOT NULL,
        [activity_date] date NOT NULL,
        [source] nvarchar(50) NOT NULL,
        [created_at] datetimeoffset NULL DEFAULT ((SYSDATETIMEOFFSET())),
        CONSTRAINT [PK_streak_activity_log] PRIMARY KEY ([id]),
        CONSTRAINT [CK_streak_activity_log_source] CHECK ([source] IN (N'workout', N'nutrition')),
        CONSTRAINT [FK_streak_activity_log_profiles] FOREIGN KEY ([user_id]) REFERENCES [profiles] ([id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905173708_AddGroups3Schema'
)
BEGIN
    CREATE TABLE [user_active_program] (
        [id] uniqueidentifier NOT NULL,
        [user_id] uniqueidentifier NOT NULL,
        [program_id] uniqueidentifier NOT NULL,
        [started_at] datetimeoffset NULL DEFAULT ((SYSDATETIMEOFFSET())),
        [current_week] int NULL DEFAULT (((1))),
        [current_day] int NULL DEFAULT (((1))),
        [notes] nvarchar(max) NULL,
        [updated_at] datetimeoffset NULL DEFAULT ((SYSDATETIMEOFFSET())),
        CONSTRAINT [PK_user_active_program] PRIMARY KEY ([id]),
        CONSTRAINT [FK_user_active_program_profiles] FOREIGN KEY ([user_id]) REFERENCES [profiles] ([id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_user_active_program_training_programs] FOREIGN KEY ([program_id]) REFERENCES [training_programs] ([id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905173708_AddGroups3Schema'
)
BEGIN
    CREATE TABLE [user_programs] (
        [id] uniqueidentifier NOT NULL,
        [user_id] uniqueidentifier NOT NULL,
        [program_name] nvarchar(200) NOT NULL,
        [muscle_group] nvarchar(50) NOT NULL,
        [is_active] bit NULL DEFAULT (((1))),
        [started_at] datetimeoffset NULL,
        CONSTRAINT [PK_user_programs] PRIMARY KEY ([id]),
        CONSTRAINT [FK_user_programs_profiles] FOREIGN KEY ([user_id]) REFERENCES [profiles] ([id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905173708_AddGroups3Schema'
)
BEGIN
    CREATE TABLE [voice_food_logs] (
        [id] uniqueidentifier NOT NULL,
        [user_id] uniqueidentifier NOT NULL,
        [audio_path] nvarchar(500) NULL,
        [transcript] nvarchar(max) NULL,
        [is_food] bit NULL DEFAULT (((1))),
        [confidence] nvarchar(50) NULL DEFAULT ((N'medium')),
        [notes] nvarchar(max) NULL,
        [logged_at] datetimeoffset NULL,
        [created_at] datetimeoffset NULL DEFAULT ((SYSDATETIMEOFFSET())),
        CONSTRAINT [PK_voice_food_logs] PRIMARY KEY ([id]),
        CONSTRAINT [FK_voice_food_logs_profiles] FOREIGN KEY ([user_id]) REFERENCES [profiles] ([id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905173708_AddGroups3Schema'
)
BEGIN
    CREATE TABLE [weekly_activity] (
        [id] uniqueidentifier NOT NULL,
        [user_id] uniqueidentifier NOT NULL,
        [week_start] date NOT NULL,
        [day_index] int NOT NULL,
        [actual_pct] decimal(5,2) NULL,
        [goal_pct] decimal(5,2) NOT NULL DEFAULT 0.0,
        CONSTRAINT [PK_weekly_activity] PRIMARY KEY ([id]),
        CONSTRAINT [FK_weekly_activity_profiles] FOREIGN KEY ([user_id]) REFERENCES [profiles] ([id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905173708_AddGroups3Schema'
)
BEGIN
    CREATE TABLE [workout_sessions] (
        [id] uniqueidentifier NOT NULL,
        [user_id] uniqueidentifier NOT NULL,
        [muscle_group] nvarchar(50) NOT NULL,
        [session_name] nvarchar(200) NULL,
        [duration_min] int NOT NULL DEFAULT 0,
        [notes] nvarchar(max) NULL,
        [session_date] date NULL DEFAULT ((CAST(SYSUTCDATETIME() AS date))),
        [started_at] datetimeoffset NULL,
        [ended_at] datetimeoffset NULL,
        CONSTRAINT [PK_workout_sessions] PRIMARY KEY ([id]),
        CONSTRAINT [FK_workout_sessions_profiles] FOREIGN KEY ([user_id]) REFERENCES [profiles] ([id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905173708_AddGroups3Schema'
)
BEGIN
    CREATE TABLE [barcode_scan_history] (
        [id] uniqueidentifier NOT NULL,
        [user_id] uniqueidentifier NOT NULL,
        [barcode] nvarchar(50) NOT NULL,
        [quantity_g] decimal(8,2) NULL,
        [nutrition_log_id] uniqueidentifier NULL,
        [scanned_at] datetimeoffset NULL,
        CONSTRAINT [PK_barcode_scan_history] PRIMARY KEY ([id]),
        CONSTRAINT [FK_barcode_scan_history_barcode_products] FOREIGN KEY ([barcode]) REFERENCES [barcode_products] ([barcode]) ON DELETE NO ACTION,
        CONSTRAINT [FK_barcode_scan_history_nutrition_logs] FOREIGN KEY ([nutrition_log_id]) REFERENCES [nutrition_logs] ([id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_barcode_scan_history_profiles] FOREIGN KEY ([user_id]) REFERENCES [profiles] ([id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905173708_AddGroups3Schema'
)
BEGIN
    CREATE TABLE [food_scan_items] (
        [id] uniqueidentifier NOT NULL,
        [scan_id] uniqueidentifier NOT NULL,
        [name] nvarchar(200) NOT NULL,
        [name_ar] nvarchar(200) NULL,
        [estimated_weight_g] decimal(8,2) NOT NULL DEFAULT 0.0,
        [calories] decimal(8,2) NOT NULL DEFAULT 0.0,
        [protein_g] decimal(8,2) NOT NULL DEFAULT 0.0,
        [carbs_g] decimal(8,2) NOT NULL DEFAULT 0.0,
        [fat_g] decimal(8,2) NOT NULL DEFAULT 0.0,
        [nutrition_log_id] uniqueidentifier NULL,
        [created_at] datetimeoffset NULL DEFAULT ((SYSDATETIMEOFFSET())),
        CONSTRAINT [PK_food_scan_items] PRIMARY KEY ([id]),
        CONSTRAINT [FK_food_scan_items_food_scans] FOREIGN KEY ([scan_id]) REFERENCES [food_scans] ([id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_food_scan_items_nutrition_logs] FOREIGN KEY ([nutrition_log_id]) REFERENCES [nutrition_logs] ([id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905173708_AddGroups3Schema'
)
BEGIN
    CREATE TABLE [voice_food_log_items] (
        [id] uniqueidentifier NOT NULL,
        [log_id] uniqueidentifier NOT NULL,
        [name] nvarchar(200) NOT NULL,
        [name_ar] nvarchar(200) NULL,
        [estimated_weight_g] decimal(8,2) NOT NULL DEFAULT 0.0,
        [calories] decimal(8,2) NOT NULL DEFAULT 0.0,
        [protein_g] decimal(8,2) NOT NULL DEFAULT 0.0,
        [carbs_g] decimal(8,2) NOT NULL DEFAULT 0.0,
        [fat_g] decimal(8,2) NOT NULL DEFAULT 0.0,
        [nutrition_log_id] uniqueidentifier NULL,
        [created_at] datetimeoffset NULL DEFAULT ((SYSDATETIMEOFFSET())),
        CONSTRAINT [PK_voice_food_log_items] PRIMARY KEY ([id]),
        CONSTRAINT [FK_voice_food_log_items_nutrition_logs] FOREIGN KEY ([nutrition_log_id]) REFERENCES [nutrition_logs] ([id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_voice_food_log_items_voice_food_logs] FOREIGN KEY ([log_id]) REFERENCES [voice_food_logs] ([id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905173708_AddGroups3Schema'
)
BEGIN
    CREATE TABLE [exercise_progress] (
        [id] uniqueidentifier NOT NULL,
        [user_id] uniqueidentifier NOT NULL,
        [exercise_id] uniqueidentifier NULL,
        [session_date] date NULL DEFAULT ((CAST(SYSUTCDATETIME() AS date))),
        [best_set_weight] decimal(5,2) NULL,
        [best_set_reps] int NULL,
        [total_volume] decimal(10,2) NULL,
        [one_rm_estimate] decimal(6,2) NULL,
        [session_id] uniqueidentifier NULL,
        [created_at] datetimeoffset NULL DEFAULT ((SYSDATETIMEOFFSET())),
        CONSTRAINT [PK_exercise_progress] PRIMARY KEY ([id]),
        CONSTRAINT [FK_exercise_progress_exercises] FOREIGN KEY ([exercise_id]) REFERENCES [exercises] ([id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_exercise_progress_profiles] FOREIGN KEY ([user_id]) REFERENCES [profiles] ([id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_exercise_progress_workout_sessions] FOREIGN KEY ([session_id]) REFERENCES [workout_sessions] ([id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905173708_AddGroups3Schema'
)
BEGIN
    CREATE TABLE [workout_sets] (
        [id] uniqueidentifier NOT NULL,
        [session_id] uniqueidentifier NULL,
        [user_id] uniqueidentifier NOT NULL,
        [exercise_name] nvarchar(200) NOT NULL,
        [set_number] int NOT NULL,
        [reps] int NULL,
        [weight_kg] decimal(5,2) NULL,
        [duration_sec] int NULL,
        [rest_sec] int NULL DEFAULT (((60))),
        [is_warmup] bit NOT NULL DEFAULT CAST(0 AS bit),
        [logged_at] datetimeoffset NULL,
        CONSTRAINT [PK_workout_sets] PRIMARY KEY ([id]),
        CONSTRAINT [FK_workout_sets_profiles] FOREIGN KEY ([user_id]) REFERENCES [profiles] ([id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_workout_sets_workout_sessions] FOREIGN KEY ([session_id]) REFERENCES [workout_sessions] ([id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905173708_AddGroups3Schema'
)
BEGIN
    CREATE INDEX [IX_barcode_scan_history_barcode] ON [barcode_scan_history] ([barcode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905173708_AddGroups3Schema'
)
BEGIN
    CREATE INDEX [IX_barcode_scan_history_nutrition_log_id] ON [barcode_scan_history] ([nutrition_log_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905173708_AddGroups3Schema'
)
BEGIN
    CREATE INDEX [IX_barcode_scan_history_user_id] ON [barcode_scan_history] ([user_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905173708_AddGroups3Schema'
)
BEGIN
    CREATE INDEX [IX_daily_activity_user_id_activity_date] ON [daily_activity] ([user_id], [activity_date]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905173708_AddGroups3Schema'
)
BEGIN
    CREATE INDEX [IX_daily_summary_user_id_summary_date] ON [daily_summary] ([user_id], [summary_date]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905173708_AddGroups3Schema'
)
BEGIN
    CREATE INDEX [IX_exercise_progress_exercise_id] ON [exercise_progress] ([exercise_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905173708_AddGroups3Schema'
)
BEGIN
    CREATE INDEX [IX_exercise_progress_session_id] ON [exercise_progress] ([session_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905173708_AddGroups3Schema'
)
BEGIN
    CREATE INDEX [IX_exercise_progress_user_id_session_date] ON [exercise_progress] ([user_id], [session_date]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905173708_AddGroups3Schema'
)
BEGIN
    CREATE INDEX [IX_food_scan_items_nutrition_log_id] ON [food_scan_items] ([nutrition_log_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905173708_AddGroups3Schema'
)
BEGIN
    CREATE INDEX [IX_food_scan_items_scan_id] ON [food_scan_items] ([scan_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905173708_AddGroups3Schema'
)
BEGIN
    CREATE INDEX [IX_food_scans_user_id] ON [food_scans] ([user_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905173708_AddGroups3Schema'
)
BEGIN
    CREATE INDEX [IX_nutrition_logs_food_id] ON [nutrition_logs] ([food_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905173708_AddGroups3Schema'
)
BEGIN
    CREATE INDEX [IX_nutrition_logs_user_id_logged_date] ON [nutrition_logs] ([user_id], [logged_date]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905173708_AddGroups3Schema'
)
BEGIN
    CREATE INDEX [IX_streak_activity_log_user_id_activity_date] ON [streak_activity_log] ([user_id], [activity_date]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905173708_AddGroups3Schema'
)
BEGIN
    CREATE INDEX [IX_user_active_program_program_id] ON [user_active_program] ([program_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905173708_AddGroups3Schema'
)
BEGIN
    CREATE INDEX [IX_user_active_program_user_id] ON [user_active_program] ([user_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905173708_AddGroups3Schema'
)
BEGIN
    CREATE INDEX [IX_user_programs_user_id] ON [user_programs] ([user_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905173708_AddGroups3Schema'
)
BEGIN
    CREATE INDEX [IX_voice_food_log_items_log_id] ON [voice_food_log_items] ([log_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905173708_AddGroups3Schema'
)
BEGIN
    CREATE INDEX [IX_voice_food_log_items_nutrition_log_id] ON [voice_food_log_items] ([nutrition_log_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905173708_AddGroups3Schema'
)
BEGIN
    CREATE INDEX [IX_voice_food_logs_user_id] ON [voice_food_logs] ([user_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905173708_AddGroups3Schema'
)
BEGIN
    CREATE INDEX [IX_weekly_activity_user_id_week_start] ON [weekly_activity] ([user_id], [week_start]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905173708_AddGroups3Schema'
)
BEGIN
    CREATE INDEX [IX_workout_sessions_user_id_session_date] ON [workout_sessions] ([user_id], [session_date]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905173708_AddGroups3Schema'
)
BEGIN
    CREATE INDEX [IX_workout_sets_session_id] ON [workout_sets] ([session_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905173708_AddGroups3Schema'
)
BEGIN
    CREATE INDEX [IX_workout_sets_user_id_exercise_name] ON [workout_sets] ([user_id], [exercise_name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905173708_AddGroups3Schema'
)
BEGIN

    CREATE TRIGGER dbo.trg_daily_summary_updated_at
    ON dbo.daily_summary
    AFTER UPDATE
    AS
    BEGIN
        SET NOCOUNT ON;
        UPDATE s SET updated_at = SYSDATETIMEOFFSET()
        FROM dbo.daily_summary s INNER JOIN inserted i ON i.id = s.id;
    END
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905173708_AddGroups3Schema'
)
BEGIN

    CREATE TRIGGER dbo.trg_user_active_program_updated_at
    ON dbo.user_active_program
    AFTER UPDATE
    AS
    BEGIN
        SET NOCOUNT ON;
        UPDATE p SET updated_at = SYSDATETIMEOFFSET()
        FROM dbo.user_active_program p INNER JOIN inserted i ON i.id = p.id;
    END
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905173708_AddGroups3Schema'
)
BEGIN

    CREATE TRIGGER dbo.trg_barcode_products_updated_at
    ON dbo.barcode_products
    AFTER UPDATE
    AS
    BEGIN
        SET NOCOUNT ON;
        UPDATE b SET updated_at = SYSDATETIMEOFFSET()
        FROM dbo.barcode_products b INNER JOIN inserted i ON i.barcode = b.barcode;
    END
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905173708_AddGroups3Schema'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260905173708_AddGroups3Schema', N'10.0.11');
END;

COMMIT;
GO

