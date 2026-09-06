BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906155139_AddGroups4Schema'
)
BEGIN
    CREATE TABLE [coach_onboarding] (
        [id] uniqueidentifier NOT NULL,
        [user_id] uniqueidentifier NOT NULL,
        [display_name] nvarchar(200) NULL,
        [years_experience] int NOT NULL DEFAULT 0,
        [certifications] nvarchar(max) NULL,
        [specialization] nvarchar(max) NULL,
        [bio] nvarchar(max) NULL,
        [price_monthly] decimal(10,2) NULL,
        [price_premium] decimal(10,2) NULL,
        [languages] nvarchar(max) NULL DEFAULT ((N'["Arabic","English"]')),
        [max_clients] int NULL DEFAULT (((10))),
        [profile_image_url] nvarchar(500) NULL,
        [intro_video_url] nvarchar(500) NULL,
        [is_completed] bit NOT NULL DEFAULT CAST(0 AS bit),
        [created_at] datetimeoffset NULL DEFAULT ((SYSDATETIMEOFFSET())),
        [updated_at] datetimeoffset NULL DEFAULT ((SYSDATETIMEOFFSET())),
        [phone_number] nvarchar(30) NULL,
        [city] nvarchar(100) NULL,
        [gender] nvarchar(50) NULL,
        [gallery_images] nvarchar(max) NULL,
        [pdf_urls] nvarchar(max) NULL,
        [certificate_files] nvarchar(max) NULL,
        [transformation_images] nvarchar(max) NULL,
        CONSTRAINT [PK_coach_onboarding] PRIMARY KEY ([id]),
        CONSTRAINT [CK_coach_onboarding_certificate_files_json] CHECK ([certificate_files] IS NULL OR ISJSON([certificate_files]) = 1),
        CONSTRAINT [CK_coach_onboarding_certifications_json] CHECK ([certifications] IS NULL OR ISJSON([certifications]) = 1),
        CONSTRAINT [CK_coach_onboarding_gallery_images_json] CHECK ([gallery_images] IS NULL OR ISJSON([gallery_images]) = 1),
        CONSTRAINT [CK_coach_onboarding_languages_json] CHECK ([languages] IS NULL OR ISJSON([languages]) = 1),
        CONSTRAINT [CK_coach_onboarding_pdf_urls_json] CHECK ([pdf_urls] IS NULL OR ISJSON([pdf_urls]) = 1),
        CONSTRAINT [CK_coach_onboarding_specialization_json] CHECK ([specialization] IS NULL OR ISJSON([specialization]) = 1),
        CONSTRAINT [CK_coach_onboarding_transformation_images_json] CHECK ([transformation_images] IS NULL OR ISJSON([transformation_images]) = 1),
        CONSTRAINT [FK_coach_onboarding_profiles] FOREIGN KEY ([user_id]) REFERENCES [profiles] ([id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906155139_AddGroups4Schema'
)
BEGIN
    CREATE TABLE [coach_profiles] (
        [id] uniqueidentifier NOT NULL,
        [bio] nvarchar(max) NULL,
        [bio_ar] nvarchar(max) NULL,
        [specialties] nvarchar(max) NULL,
        [certifications] nvarchar(max) NULL,
        [experience_years] int NOT NULL DEFAULT 0,
        [price_per_month] decimal(10,2) NOT NULL DEFAULT 0.0,
        [currency] nvarchar(10) NULL DEFAULT ((N'EGP')),
        [rating] decimal(3,2) NOT NULL DEFAULT 0.0,
        [reviews_count] int NOT NULL DEFAULT 0,
        [is_available] bit NULL DEFAULT (((1))),
        [is_verified] bit NOT NULL DEFAULT CAST(0 AS bit),
        [cover_image_url] nvarchar(500) NULL,
        [instagram_url] nvarchar(500) NULL,
        [youtube_url] nvarchar(500) NULL,
        [max_clients] int NULL DEFAULT (((20))),
        [current_clients] int NOT NULL DEFAULT 0,
        [created_at] datetimeoffset NULL DEFAULT ((SYSDATETIMEOFFSET())),
        [updated_at] datetimeoffset NULL DEFAULT ((SYSDATETIMEOFFSET())),
        CONSTRAINT [PK_coach_profiles] PRIMARY KEY ([id]),
        CONSTRAINT [CK_coach_profiles_certifications_json] CHECK ([certifications] IS NULL OR ISJSON([certifications]) = 1),
        CONSTRAINT [CK_coach_profiles_specialties_json] CHECK ([specialties] IS NULL OR ISJSON([specialties]) = 1),
        CONSTRAINT [FK_coach_profiles_profiles] FOREIGN KEY ([id]) REFERENCES [profiles] ([id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906155139_AddGroups4Schema'
)
BEGIN
    CREATE TABLE [coaches] (
        [id] uniqueidentifier NOT NULL,
        [user_id] uniqueidentifier NOT NULL,
        [bio] nvarchar(max) NULL DEFAULT ((N'')),
        [price_monthly] decimal(10,2) NOT NULL DEFAULT 0.0,
        [specialization] nvarchar(max) NULL,
        [rating] decimal(3,2) NOT NULL DEFAULT 0.0,
        [is_active] bit NULL DEFAULT (((1))),
        [stripe_account_id] nvarchar(100) NULL,
        [created_at] datetimeoffset NULL DEFAULT ((SYSDATETIMEOFFSET())),
        [updated_at] datetimeoffset NULL DEFAULT ((SYSDATETIMEOFFSET())),
        CONSTRAINT [PK_coaches] PRIMARY KEY ([id]),
        CONSTRAINT [CK_coaches_specialization_json] CHECK ([specialization] IS NULL OR ISJSON([specialization]) = 1),
        CONSTRAINT [FK_coaches_profiles] FOREIGN KEY ([user_id]) REFERENCES [profiles] ([id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906155139_AddGroups4Schema'
)
BEGIN
    CREATE TABLE [stripe_customers] (
        [id] uniqueidentifier NOT NULL,
        [user_id] uniqueidentifier NOT NULL,
        [stripe_customer_id] nvarchar(100) NOT NULL,
        [created_at] datetimeoffset NULL DEFAULT ((SYSDATETIMEOFFSET())),
        CONSTRAINT [PK_stripe_customers] PRIMARY KEY ([id]),
        CONSTRAINT [FK_stripe_customers_profiles] FOREIGN KEY ([user_id]) REFERENCES [profiles] ([id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906155139_AddGroups4Schema'
)
BEGIN
    CREATE TABLE [subscription_plans] (
        [id] uniqueidentifier NOT NULL,
        [coach_id] uniqueidentifier NOT NULL,
        [name] nvarchar(200) NOT NULL,
        [price_usd] decimal(10,2) NULL,
        [duration_days] int NULL,
        [max_clients] int NULL,
        [created_at] datetimeoffset NULL DEFAULT ((SYSDATETIMEOFFSET())),
        CONSTRAINT [PK_subscription_plans] PRIMARY KEY ([id]),
        CONSTRAINT [FK_subscription_plans_profiles] FOREIGN KEY ([coach_id]) REFERENCES [profiles] ([id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906155139_AddGroups4Schema'
)
BEGIN
    CREATE TABLE [coach_content] (
        [id] uniqueidentifier NOT NULL,
        [coach_id] uniqueidentifier NOT NULL,
        [title] nvarchar(200) NOT NULL,
        [description] nvarchar(max) NULL,
        [type] nvarchar(50) NOT NULL,
        [file_url] nvarchar(500) NOT NULL,
        [is_public] bit NOT NULL DEFAULT CAST(0 AS bit),
        [created_at] datetimeoffset NULL DEFAULT ((SYSDATETIMEOFFSET())),
        [thumbnail_url] nvarchar(500) NULL,
        [file_size_kb] int NULL,
        [sort_order] int NOT NULL DEFAULT 0,
        CONSTRAINT [PK_coach_content] PRIMARY KEY ([id]),
        CONSTRAINT [FK_coach_content_coaches] FOREIGN KEY ([coach_id]) REFERENCES [coaches] ([id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906155139_AddGroups4Schema'
)
BEGIN
    CREATE TABLE [payment_intents] (
        [id] uniqueidentifier NOT NULL,
        [client_id] uniqueidentifier NOT NULL,
        [coach_id] uniqueidentifier NOT NULL,
        [stripe_payment_id] nvarchar(100) NOT NULL,
        [stripe_customer_id] nvarchar(100) NULL,
        [amount] decimal(12,2) NOT NULL,
        [currency] nvarchar(10) NULL DEFAULT ((N'usd')),
        [status] nvarchar(50) NULL DEFAULT ((N'pending')),
        [tier] nvarchar(50) NULL DEFAULT ((N'standard')),
        [created_at] datetimeoffset NULL DEFAULT ((SYSDATETIMEOFFSET())),
        CONSTRAINT [PK_payment_intents] PRIMARY KEY ([id]),
        CONSTRAINT [FK_payment_intents_coaches] FOREIGN KEY ([coach_id]) REFERENCES [coaches] ([id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_payment_intents_profiles] FOREIGN KEY ([client_id]) REFERENCES [profiles] ([id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906155139_AddGroups4Schema'
)
BEGIN
    CREATE TABLE [reviews] (
        [id] uniqueidentifier NOT NULL,
        [client_id] uniqueidentifier NOT NULL,
        [coach_id] uniqueidentifier NOT NULL,
        [rating] int NOT NULL,
        [comment] nvarchar(max) NULL,
        [created_at] datetimeoffset NULL DEFAULT ((SYSDATETIMEOFFSET())),
        CONSTRAINT [PK_reviews] PRIMARY KEY ([id]),
        CONSTRAINT [FK_reviews_coaches] FOREIGN KEY ([coach_id]) REFERENCES [coaches] ([id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_reviews_profiles] FOREIGN KEY ([client_id]) REFERENCES [profiles] ([id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906155139_AddGroups4Schema'
)
BEGIN
    CREATE TABLE [subscriptions] (
        [id] uniqueidentifier NOT NULL,
        [client_id] uniqueidentifier NOT NULL,
        [coach_id] uniqueidentifier NOT NULL,
        [status] nvarchar(20) NOT NULL DEFAULT ((N'active')),
        [tier] nvarchar(50) NULL DEFAULT ((N'basic')),
        [start_date] date NULL DEFAULT ((CAST(SYSUTCDATETIME() AS date))),
        [end_date] date NULL,
        [stripe_sub_id] nvarchar(100) NULL,
        [created_at] datetimeoffset NULL DEFAULT ((SYSDATETIMEOFFSET())),
        [updated_at] datetimeoffset NULL DEFAULT ((SYSDATETIMEOFFSET())),
        [plan_id] uniqueidentifier NULL,
        [payment_status] nvarchar(50) NULL,
        [started_at] datetimeoffset NULL,
        [expires_at] datetimeoffset NULL,
        [goals] nvarchar(max) NULL,
        [notes] nvarchar(max) NULL,
        CONSTRAINT [PK_subscriptions] PRIMARY KEY ([id]),
        CONSTRAINT [CK_subscriptions_status] CHECK ([status] IN (N'pending', N'active', N'cancelled', N'expired')),
        CONSTRAINT [FK_subscriptions_coaches] FOREIGN KEY ([coach_id]) REFERENCES [coaches] ([id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_subscriptions_profiles] FOREIGN KEY ([client_id]) REFERENCES [profiles] ([id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_subscriptions_subscription_plans] FOREIGN KEY ([plan_id]) REFERENCES [subscription_plans] ([id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906155139_AddGroups4Schema'
)
BEGIN
    CREATE TABLE [client_assignments] (
        [id] uniqueidentifier NOT NULL,
        [coach_id] uniqueidentifier NOT NULL,
        [client_id] uniqueidentifier NOT NULL,
        [content_id] uniqueidentifier NOT NULL,
        [note] nvarchar(max) NULL,
        [assigned_at] datetimeoffset NULL,
        CONSTRAINT [PK_client_assignments] PRIMARY KEY ([id]),
        CONSTRAINT [FK_client_assignments_coach_content] FOREIGN KEY ([content_id]) REFERENCES [coach_content] ([id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_client_assignments_coaches] FOREIGN KEY ([coach_id]) REFERENCES [coaches] ([id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_client_assignments_profiles] FOREIGN KEY ([client_id]) REFERENCES [profiles] ([id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906155139_AddGroups4Schema'
)
BEGIN
    CREATE TABLE [subscription_phases] (
        [id] uniqueidentifier NOT NULL,
        [subscription_id] uniqueidentifier NOT NULL,
        [phase_number] int NOT NULL,
        [title] nvarchar(200) NOT NULL,
        [type] nvarchar(50) NULL,
        [description] nvarchar(max) NULL,
        [duration_weeks] int NULL,
        [status] nvarchar(50) NULL DEFAULT ((N'upcoming')),
        [started_at] datetimeoffset NULL,
        [completed_at] datetimeoffset NULL,
        [created_at] datetimeoffset NULL DEFAULT ((SYSDATETIMEOFFSET())),
        CONSTRAINT [PK_subscription_phases] PRIMARY KEY ([id]),
        CONSTRAINT [FK_subscription_phases_subscriptions] FOREIGN KEY ([subscription_id]) REFERENCES [subscriptions] ([id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906155139_AddGroups4Schema'
)
BEGIN
    EXEC(N'ALTER TABLE [profiles] ADD CONSTRAINT [CK_profiles_role] CHECK ([role] IN (N''client'', N''coach'', N''user''))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906155139_AddGroups4Schema'
)
BEGIN
    CREATE INDEX [IX_client_assignments_client_id] ON [client_assignments] ([client_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906155139_AddGroups4Schema'
)
BEGIN
    CREATE INDEX [IX_client_assignments_coach_id] ON [client_assignments] ([coach_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906155139_AddGroups4Schema'
)
BEGIN
    CREATE INDEX [IX_client_assignments_content_id] ON [client_assignments] ([content_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906155139_AddGroups4Schema'
)
BEGIN
    CREATE INDEX [IX_coach_content_coach_id] ON [coach_content] ([coach_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906155139_AddGroups4Schema'
)
BEGIN
    CREATE INDEX [IX_coach_onboarding_user_id] ON [coach_onboarding] ([user_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906155139_AddGroups4Schema'
)
BEGIN
    CREATE INDEX [IX_coaches_user_id] ON [coaches] ([user_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906155139_AddGroups4Schema'
)
BEGIN
    CREATE INDEX [IX_payment_intents_client_id] ON [payment_intents] ([client_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906155139_AddGroups4Schema'
)
BEGIN
    CREATE INDEX [IX_payment_intents_coach_id] ON [payment_intents] ([coach_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906155139_AddGroups4Schema'
)
BEGIN
    CREATE INDEX [IX_payment_intents_stripe_payment_id] ON [payment_intents] ([stripe_payment_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906155139_AddGroups4Schema'
)
BEGIN
    CREATE INDEX [IX_reviews_client_id] ON [reviews] ([client_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906155139_AddGroups4Schema'
)
BEGIN
    CREATE INDEX [IX_reviews_coach_id] ON [reviews] ([coach_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906155139_AddGroups4Schema'
)
BEGIN
    CREATE INDEX [IX_stripe_customers_stripe_customer_id] ON [stripe_customers] ([stripe_customer_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906155139_AddGroups4Schema'
)
BEGIN
    CREATE INDEX [IX_stripe_customers_user_id] ON [stripe_customers] ([user_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906155139_AddGroups4Schema'
)
BEGIN
    CREATE INDEX [IX_subscription_phases_subscription_id] ON [subscription_phases] ([subscription_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906155139_AddGroups4Schema'
)
BEGIN
    CREATE INDEX [IX_subscription_plans_coach_id] ON [subscription_plans] ([coach_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906155139_AddGroups4Schema'
)
BEGIN
    CREATE INDEX [IX_subscriptions_client_id_coach_id] ON [subscriptions] ([client_id], [coach_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906155139_AddGroups4Schema'
)
BEGIN
    CREATE INDEX [IX_subscriptions_coach_id] ON [subscriptions] ([coach_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906155139_AddGroups4Schema'
)
BEGIN
    CREATE INDEX [IX_subscriptions_plan_id] ON [subscriptions] ([plan_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906155139_AddGroups4Schema'
)
BEGIN

    CREATE TRIGGER dbo.trg_coaches_updated_at
    ON dbo.coaches
    AFTER UPDATE
    AS
    BEGIN
        SET NOCOUNT ON;
        UPDATE c SET updated_at = SYSDATETIMEOFFSET()
        FROM dbo.coaches c INNER JOIN inserted i ON i.id = c.id;
    END
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906155139_AddGroups4Schema'
)
BEGIN

    CREATE TRIGGER dbo.trg_coach_profiles_updated_at
    ON dbo.coach_profiles
    AFTER UPDATE
    AS
    BEGIN
        SET NOCOUNT ON;
        UPDATE cp SET updated_at = SYSDATETIMEOFFSET()
        FROM dbo.coach_profiles cp INNER JOIN inserted i ON i.id = cp.id;
    END
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906155139_AddGroups4Schema'
)
BEGIN

    CREATE TRIGGER dbo.trg_coach_onboarding_updated_at
    ON dbo.coach_onboarding
    AFTER UPDATE
    AS
    BEGIN
        SET NOCOUNT ON;
        UPDATE co SET updated_at = SYSDATETIMEOFFSET()
        FROM dbo.coach_onboarding co INNER JOIN inserted i ON i.id = co.id;
    END
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906155139_AddGroups4Schema'
)
BEGIN

    CREATE TRIGGER dbo.trg_subscriptions_updated_at
    ON dbo.subscriptions
    AFTER UPDATE
    AS
    BEGIN
        SET NOCOUNT ON;
        UPDATE s SET updated_at = SYSDATETIMEOFFSET()
        FROM dbo.subscriptions s INNER JOIN inserted i ON i.id = s.id;
    END
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906155139_AddGroups4Schema'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260906155139_AddGroups4Schema', N'10.0.11');
END;

COMMIT;
GO

