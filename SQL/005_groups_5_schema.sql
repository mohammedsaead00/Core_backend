BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906170952_AddGroups5Schema'
)
BEGIN
    CREATE TABLE [conversations] (
        [id] uniqueidentifier NOT NULL,
        [client_id] uniqueidentifier NOT NULL,
        [coach_id] uniqueidentifier NOT NULL,
        [subscription_id] uniqueidentifier NULL,
        [last_message] nvarchar(max) NULL,
        [last_message_at] datetimeoffset NULL DEFAULT ((SYSDATETIMEOFFSET())),
        [client_unread] int NOT NULL DEFAULT 0,
        [coach_unread] int NOT NULL DEFAULT 0,
        [is_active] bit NULL DEFAULT (((1))),
        [created_at] datetimeoffset NULL DEFAULT ((SYSDATETIMEOFFSET())),
        CONSTRAINT [PK_conversations] PRIMARY KEY ([id]),
        CONSTRAINT [FK_conversations_coach_profiles] FOREIGN KEY ([coach_id]) REFERENCES [profiles] ([id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_conversations_profiles] FOREIGN KEY ([client_id]) REFERENCES [profiles] ([id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_conversations_subscriptions] FOREIGN KEY ([subscription_id]) REFERENCES [subscriptions] ([id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906170952_AddGroups5Schema'
)
BEGIN
    CREATE TABLE [notification_log] (
        [id] uniqueidentifier NOT NULL,
        [user_id] uniqueidentifier NOT NULL,
        [type] nvarchar(50) NOT NULL,
        [title] nvarchar(200) NOT NULL,
        [body] nvarchar(max) NOT NULL,
        [data] nvarchar(max) NULL,
        [sent_at] datetimeoffset NULL DEFAULT ((SYSDATETIMEOFFSET())),
        [read_at] datetimeoffset NULL,
        CONSTRAINT [PK_notification_log] PRIMARY KEY ([id]),
        CONSTRAINT [CK_notification_log_data_json] CHECK ([data] IS NULL OR ISJSON([data]) = 1),
        CONSTRAINT [FK_notification_log_profiles] FOREIGN KEY ([user_id]) REFERENCES [profiles] ([id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906170952_AddGroups5Schema'
)
BEGIN
    CREATE TABLE [notification_preferences] (
        [user_id] uniqueidentifier NOT NULL,
        [meal_reminders_enabled] bit NULL DEFAULT (((1))),
        [water_reminders_enabled] bit NULL DEFAULT (((1))),
        [calorie_alerts_enabled] bit NULL DEFAULT (((1))),
        [chat_notifications_enabled] bit NULL DEFAULT (((1))),
        [quiet_hours_start] time NULL,
        [quiet_hours_end] time NULL,
        [updated_at] datetimeoffset NULL DEFAULT ((SYSDATETIMEOFFSET())),
        CONSTRAINT [PK_notification_preferences] PRIMARY KEY ([user_id]),
        CONSTRAINT [FK_notification_preferences_profiles] FOREIGN KEY ([user_id]) REFERENCES [profiles] ([id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906170952_AddGroups5Schema'
)
BEGIN
    CREATE TABLE [messages] (
        [id] uniqueidentifier NOT NULL,
        [conversation_id] uniqueidentifier NOT NULL,
        [sender_id] uniqueidentifier NOT NULL,
        [content] nvarchar(max) NOT NULL,
        [type] nvarchar(20) NULL DEFAULT ((N'text')),
        [file_url] nvarchar(500) NULL,
        [is_read] bit NOT NULL DEFAULT CAST(0 AS bit),
        [is_deleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        [created_at] datetimeoffset NULL DEFAULT ((SYSDATETIMEOFFSET())),
        CONSTRAINT [PK_messages] PRIMARY KEY ([id]),
        CONSTRAINT [CK_messages_type] CHECK ([type] IN (N'text', N'voice', N'image', N'file')),
        CONSTRAINT [FK_messages_conversations] FOREIGN KEY ([conversation_id]) REFERENCES [conversations] ([id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_messages_profiles] FOREIGN KEY ([sender_id]) REFERENCES [profiles] ([id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906170952_AddGroups5Schema'
)
BEGIN
    CREATE TABLE [notifications] (
        [id] uniqueidentifier NOT NULL,
        [user_id] uniqueidentifier NOT NULL,
        [type] nvarchar(50) NOT NULL,
        [title] nvarchar(200) NOT NULL,
        [body] nvarchar(max) NOT NULL,
        [conversation_id] uniqueidentifier NULL,
        [plan_id] uniqueidentifier NULL,
        [coach_id] uniqueidentifier NULL,
        [is_read] bit NOT NULL DEFAULT CAST(0 AS bit),
        [created_at] datetimeoffset NULL DEFAULT ((SYSDATETIMEOFFSET())),
        CONSTRAINT [PK_notifications] PRIMARY KEY ([id]),
        CONSTRAINT [FK_notifications_conversations] FOREIGN KEY ([conversation_id]) REFERENCES [conversations] ([id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_notifications_profiles] FOREIGN KEY ([user_id]) REFERENCES [profiles] ([id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_notifications_subscription_plans] FOREIGN KEY ([plan_id]) REFERENCES [subscription_plans] ([id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906170952_AddGroups5Schema'
)
BEGIN
    CREATE INDEX [IX_conversations_client_id] ON [conversations] ([client_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906170952_AddGroups5Schema'
)
BEGIN
    CREATE INDEX [IX_conversations_coach_id] ON [conversations] ([coach_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906170952_AddGroups5Schema'
)
BEGIN
    CREATE INDEX [IX_conversations_subscription_id] ON [conversations] ([subscription_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906170952_AddGroups5Schema'
)
BEGIN
    CREATE INDEX [IX_messages_conversation_id_created_at] ON [messages] ([conversation_id], [created_at]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906170952_AddGroups5Schema'
)
BEGIN
    CREATE INDEX [IX_messages_sender_id] ON [messages] ([sender_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906170952_AddGroups5Schema'
)
BEGIN
    CREATE INDEX [IX_notification_log_user_id_sent_at] ON [notification_log] ([user_id], [sent_at]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906170952_AddGroups5Schema'
)
BEGIN
    CREATE INDEX [IX_notifications_conversation_id] ON [notifications] ([conversation_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906170952_AddGroups5Schema'
)
BEGIN
    CREATE INDEX [IX_notifications_plan_id] ON [notifications] ([plan_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906170952_AddGroups5Schema'
)
BEGIN
    CREATE INDEX [IX_notifications_user_id_created_at] ON [notifications] ([user_id], [created_at]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906170952_AddGroups5Schema'
)
BEGIN

    CREATE TRIGGER dbo.trg_notification_preferences_updated_at
    ON dbo.notification_preferences
    AFTER UPDATE
    AS
    BEGIN
        SET NOCOUNT ON;
        UPDATE p SET updated_at = SYSDATETIMEOFFSET()
        FROM dbo.notification_preferences p INNER JOIN inserted i ON i.user_id = p.user_id;
    END
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260906170952_AddGroups5Schema'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260906170952_AddGroups5Schema', N'10.0.11');
END;

COMMIT;
GO

