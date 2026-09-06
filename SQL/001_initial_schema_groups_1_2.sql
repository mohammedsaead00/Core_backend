IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905165116_InitialSchemaGroups1And2'
)
BEGIN
    CREATE TABLE [exercises] (
        [id] uniqueidentifier NOT NULL,
        [name] nvarchar(200) NOT NULL,
        [name_ar] nvarchar(200) NULL,
        [muscle_group] nvarchar(50) NULL,
        [secondary_muscles] nvarchar(max) NULL,
        [equipment] nvarchar(50) NULL,
        [category] nvarchar(50) NULL,
        [instructions] nvarchar(max) NULL,
        [instructions_ar] nvarchar(max) NULL,
        [tips] nvarchar(max) NULL,
        [created_at] datetimeoffset NULL DEFAULT ((SYSDATETIMEOFFSET())),
        [image_url] nvarchar(500) NULL,
        [youtube_video_id] nvarchar(50) NULL,
        [gif_url] nvarchar(500) NULL,
        CONSTRAINT [PK_exercises] PRIMARY KEY ([id]),
        CONSTRAINT [CK_exercises_secondary_muscles_json] CHECK ([secondary_muscles] IS NULL OR ISJSON([secondary_muscles]) = 1)
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905165116_InitialSchemaGroups1And2'
)
BEGIN
    CREATE TABLE [profiles] (
        [id] uniqueidentifier NOT NULL,
        [name] nvarchar(200) NULL DEFAULT ((N'')),
        [email] nvarchar(320) NULL DEFAULT ((N'')),
        [gender] nvarchar(50) NULL,
        [age] int NULL,
        [weight_kg] decimal(5,2) NULL,
        [height_cm] decimal(5,1) NULL,
        [fitness_goal] nvarchar(200) NULL,
        [avatar_url] nvarchar(500) NULL,
        [created_at] datetimeoffset NULL DEFAULT ((SYSDATETIMEOFFSET())),
        [updated_at] datetimeoffset NULL DEFAULT ((SYSDATETIMEOFFSET())),
        [role] nvarchar(20) NOT NULL DEFAULT ((N'client')),
        [full_name] nvarchar(200) NULL,
        CONSTRAINT [PK_profiles] PRIMARY KEY ([id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905165116_InitialSchemaGroups1And2'
)
BEGIN
    CREATE TABLE [training_programs] (
        [id] uniqueidentifier NOT NULL,
        [name] nvarchar(200) NOT NULL,
        [name_ar] nvarchar(200) NULL,
        [description] nvarchar(max) NULL,
        [description_ar] nvarchar(max) NULL,
        [level] nvarchar(50) NULL,
        [goal] nvarchar(50) NULL,
        [days_per_week] int NULL,
        [duration_weeks] int NULL,
        [split_type] nvarchar(50) NULL,
        [is_active] bit NULL DEFAULT (((1))),
        [created_at] datetimeoffset NULL DEFAULT ((SYSDATETIMEOFFSET())),
        CONSTRAINT [PK_training_programs] PRIMARY KEY ([id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905165116_InitialSchemaGroups1And2'
)
BEGIN
    CREATE TABLE [body_measurements] (
        [id] uniqueidentifier NOT NULL,
        [user_id] uniqueidentifier NOT NULL,
        [weight_kg] decimal(5,2) NULL,
        [body_fat_pct] decimal(5,2) NULL,
        [muscle_mass] decimal(5,2) NULL,
        [chest_cm] decimal(5,1) NULL,
        [waist_cm] decimal(5,1) NULL,
        [hips_cm] decimal(5,1) NULL,
        [arms_cm] decimal(5,1) NULL,
        [thighs_cm] decimal(5,1) NULL,
        [measured_date] date NULL DEFAULT ((CAST(SYSUTCDATETIME() AS date))),
        [notes] nvarchar(max) NULL,
        [created_at] datetimeoffset NULL DEFAULT ((SYSDATETIMEOFFSET())),
        CONSTRAINT [PK_body_measurements] PRIMARY KEY ([id]),
        CONSTRAINT [FK_body_measurements_profiles] FOREIGN KEY ([user_id]) REFERENCES [profiles] ([id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905165116_InitialSchemaGroups1And2'
)
BEGIN
    CREATE TABLE [foods] (
        [id] uniqueidentifier NOT NULL,
        [name] nvarchar(200) NOT NULL,
        [name_ar] nvarchar(200) NULL,
        [calories] decimal(8,2) NOT NULL,
        [protein_g] decimal(8,2) NOT NULL DEFAULT 0.0,
        [carbs_g] decimal(8,2) NOT NULL DEFAULT 0.0,
        [fat_g] decimal(8,2) NOT NULL DEFAULT 0.0,
        [fiber_g] decimal(8,2) NOT NULL DEFAULT 0.0,
        [serving_size] decimal(6,2) NULL DEFAULT (((100))),
        [serving_unit] nvarchar(50) NULL DEFAULT ((N'g')),
        [is_custom] bit NOT NULL DEFAULT CAST(0 AS bit),
        [created_by] uniqueidentifier NULL,
        [created_at] datetimeoffset NULL DEFAULT ((SYSDATETIMEOFFSET())),
        [category] nvarchar(50) NULL DEFAULT ((N'other')),
        [image_url] nvarchar(500) NULL,
        CONSTRAINT [PK_foods] PRIMARY KEY ([id]),
        CONSTRAINT [FK_foods_profiles] FOREIGN KEY ([created_by]) REFERENCES [profiles] ([id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905165116_InitialSchemaGroups1And2'
)
BEGIN
    CREATE TABLE [onboarding] (
        [id] uniqueidentifier NOT NULL,
        [user_id] uniqueidentifier NOT NULL,
        [age] int NULL,
        [gender] nvarchar(50) NULL,
        [height_cm] decimal(5,1) NULL,
        [weight_kg] decimal(5,2) NULL,
        [goal] nvarchar(50) NULL,
        [activity_level] nvarchar(50) NULL,
        [target_weight] decimal(5,2) NULL,
        [weekly_workouts] int NULL,
        [completed] bit NOT NULL DEFAULT CAST(0 AS bit),
        [created_at] datetimeoffset NULL DEFAULT ((SYSDATETIMEOFFSET())),
        [updated_at] datetimeoffset NULL DEFAULT ((SYSDATETIMEOFFSET())),
        CONSTRAINT [PK_onboarding] PRIMARY KEY ([id]),
        CONSTRAINT [FK_onboarding_profiles] FOREIGN KEY ([user_id]) REFERENCES [profiles] ([id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905165116_InitialSchemaGroups1And2'
)
BEGIN
    CREATE TABLE [user_goals] (
        [id] uniqueidentifier NOT NULL,
        [user_id] uniqueidentifier NOT NULL,
        [daily_calories] int NULL DEFAULT (((2000))),
        [daily_protein_g] int NULL DEFAULT (((150))),
        [daily_carbs_g] int NULL DEFAULT (((250))),
        [daily_fat_g] int NULL DEFAULT (((65))),
        [daily_water_ml] int NULL DEFAULT (((2500))),
        [daily_steps] int NULL DEFAULT (((10000))),
        [daily_sleep_hours] decimal(4,2) NULL DEFAULT (((8))),
        [weekly_workouts] int NULL DEFAULT (((4))),
        [target_weight_kg] decimal(5,2) NULL,
        [created_at] datetimeoffset NULL DEFAULT ((SYSDATETIMEOFFSET())),
        [updated_at] datetimeoffset NULL DEFAULT ((SYSDATETIMEOFFSET())),
        CONSTRAINT [PK_user_goals] PRIMARY KEY ([id]),
        CONSTRAINT [FK_user_goals_profiles] FOREIGN KEY ([user_id]) REFERENCES [profiles] ([id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905165116_InitialSchemaGroups1And2'
)
BEGIN
    CREATE TABLE [user_streaks] (
        [user_id] uniqueidentifier NOT NULL,
        [current_streak] int NOT NULL DEFAULT (((0))),
        [longest_streak] int NOT NULL DEFAULT (((0))),
        [last_active_date] date NULL,
        [freeze_available] int NULL DEFAULT (((1))),
        [updated_at] datetimeoffset NULL DEFAULT ((SYSDATETIMEOFFSET())),
        CONSTRAINT [PK_user_streaks] PRIMARY KEY ([user_id]),
        CONSTRAINT [FK_user_streaks_profiles] FOREIGN KEY ([user_id]) REFERENCES [profiles] ([id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905165116_InitialSchemaGroups1And2'
)
BEGIN
    CREATE TABLE [program_days] (
        [id] uniqueidentifier NOT NULL,
        [program_id] uniqueidentifier NOT NULL,
        [day_number] int NOT NULL,
        [name] nvarchar(200) NOT NULL,
        [name_ar] nvarchar(200) NULL,
        [muscle_groups] nvarchar(max) NULL,
        [notes] nvarchar(max) NULL,
        CONSTRAINT [PK_program_days] PRIMARY KEY ([id]),
        CONSTRAINT [CK_program_days_muscle_groups_json] CHECK ([muscle_groups] IS NULL OR ISJSON([muscle_groups]) = 1),
        CONSTRAINT [FK_program_days_training_programs] FOREIGN KEY ([program_id]) REFERENCES [training_programs] ([id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905165116_InitialSchemaGroups1And2'
)
BEGIN
    CREATE TABLE [program_day_exercises] (
        [id] uniqueidentifier NOT NULL,
        [program_day_id] uniqueidentifier NOT NULL,
        [exercise_id] uniqueidentifier NULL,
        [order_index] int NOT NULL,
        [sets] int NULL,
        [reps_min] int NULL,
        [reps_max] int NULL,
        [rest_seconds] int NULL DEFAULT (((90))),
        [notes] nvarchar(max) NULL,
        [notes_ar] nvarchar(max) NULL,
        [is_main_lift] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_program_day_exercises] PRIMARY KEY ([id]),
        CONSTRAINT [FK_program_day_exercises_exercises] FOREIGN KEY ([exercise_id]) REFERENCES [exercises] ([id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_program_day_exercises_program_days] FOREIGN KEY ([program_day_id]) REFERENCES [program_days] ([id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905165116_InitialSchemaGroups1And2'
)
BEGIN
    CREATE INDEX [IX_body_measurements_user_id_measured_date] ON [body_measurements] ([user_id], [measured_date]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905165116_InitialSchemaGroups1And2'
)
BEGIN
    CREATE INDEX [IX_foods_created_by] ON [foods] ([created_by]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905165116_InitialSchemaGroups1And2'
)
BEGIN
    CREATE INDEX [IX_onboarding_user_id] ON [onboarding] ([user_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905165116_InitialSchemaGroups1And2'
)
BEGIN
    CREATE INDEX [IX_program_day_exercises_exercise_id] ON [program_day_exercises] ([exercise_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905165116_InitialSchemaGroups1And2'
)
BEGIN
    CREATE INDEX [IX_program_day_exercises_program_day_id_order_index] ON [program_day_exercises] ([program_day_id], [order_index]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905165116_InitialSchemaGroups1And2'
)
BEGIN
    CREATE INDEX [IX_program_days_program_id] ON [program_days] ([program_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905165116_InitialSchemaGroups1And2'
)
BEGIN
    CREATE INDEX [IX_user_goals_user_id] ON [user_goals] ([user_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905165116_InitialSchemaGroups1And2'
)
BEGIN

    CREATE TRIGGER dbo.trg_profiles_updated_at
    ON dbo.profiles
    AFTER UPDATE
    AS
    BEGIN
        SET NOCOUNT ON;
        UPDATE p SET updated_at = SYSDATETIMEOFFSET()
        FROM dbo.profiles p INNER JOIN inserted i ON i.id = p.id;
    END
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905165116_InitialSchemaGroups1And2'
)
BEGIN

    CREATE TRIGGER dbo.trg_onboarding_updated_at
    ON dbo.onboarding
    AFTER UPDATE
    AS
    BEGIN
        SET NOCOUNT ON;
        UPDATE o SET updated_at = SYSDATETIMEOFFSET()
        FROM dbo.onboarding o INNER JOIN inserted i ON i.id = o.id;
    END
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905165116_InitialSchemaGroups1And2'
)
BEGIN

    CREATE TRIGGER dbo.trg_user_goals_updated_at
    ON dbo.user_goals
    AFTER UPDATE
    AS
    BEGIN
        SET NOCOUNT ON;
        UPDATE g SET updated_at = SYSDATETIMEOFFSET()
        FROM dbo.user_goals g INNER JOIN inserted i ON i.id = g.id;
    END
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905165116_InitialSchemaGroups1And2'
)
BEGIN

    CREATE TRIGGER dbo.trg_user_streaks_updated_at
    ON dbo.user_streaks
    AFTER UPDATE
    AS
    BEGIN
        SET NOCOUNT ON;
        UPDATE s SET updated_at = SYSDATETIMEOFFSET()
        FROM dbo.user_streaks s INNER JOIN inserted i ON i.user_id = s.user_id;
    END
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905165116_InitialSchemaGroups1And2'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260905165116_InitialSchemaGroups1And2', N'10.0.11');
END;

COMMIT;
GO

