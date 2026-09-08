BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    ALTER TABLE [barcode_scan_history] DROP CONSTRAINT [FK_barcode_scan_history_barcode_products];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    ALTER TABLE [notifications] DROP CONSTRAINT [FK_notifications_subscription_plans];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    DROP INDEX [IX_user_goals_user_id] ON [user_goals];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    DROP INDEX [IX_user_active_program_user_id] ON [user_active_program];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    DROP INDEX [IX_stripe_customers_stripe_customer_id] ON [stripe_customers];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    DROP INDEX [IX_stripe_customers_user_id] ON [stripe_customers];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    DROP INDEX [IX_onboarding_user_id] ON [onboarding];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    DROP INDEX [IX_notifications_plan_id] ON [notifications];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    ALTER TABLE [messages] DROP CONSTRAINT [CK_messages_type];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    DROP INDEX [IX_coaches_user_id] ON [coaches];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    ALTER TABLE [barcode_products] DROP CONSTRAINT [CK_barcode_products_source];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    DECLARE @var nvarchar(max);
    SELECT @var = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[workout_sets]') AND [c].[name] = N'logged_at');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [workout_sets] DROP CONSTRAINT ' + @var + ';');
    EXEC(N'UPDATE [workout_sets] SET [logged_at] = (SYSDATETIMEOFFSET()) WHERE [logged_at] IS NULL');
    ALTER TABLE [workout_sets] ALTER COLUMN [logged_at] datetimeoffset NOT NULL;
    ALTER TABLE [workout_sets] ADD DEFAULT ((SYSDATETIMEOFFSET())) FOR [logged_at];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    DECLARE @var1 nvarchar(max);
    SELECT @var1 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[workout_sessions]') AND [c].[name] = N'started_at');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [workout_sessions] DROP CONSTRAINT ' + @var1 + ';');
    EXEC(N'UPDATE [workout_sessions] SET [started_at] = (SYSDATETIMEOFFSET()) WHERE [started_at] IS NULL');
    ALTER TABLE [workout_sessions] ALTER COLUMN [started_at] datetimeoffset NOT NULL;
    ALTER TABLE [workout_sessions] ADD DEFAULT ((SYSDATETIMEOFFSET())) FOR [started_at];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    DROP INDEX [IX_workout_sessions_user_id_session_date] ON [workout_sessions];
    DECLARE @var2 nvarchar(max);
    SELECT @var2 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[workout_sessions]') AND [c].[name] = N'session_date');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [workout_sessions] DROP CONSTRAINT ' + @var2 + ';');
    EXEC(N'UPDATE [workout_sessions] SET [session_date] = (CAST(SYSUTCDATETIME() AS date)) WHERE [session_date] IS NULL');
    ALTER TABLE [workout_sessions] ALTER COLUMN [session_date] date NOT NULL;
    ALTER TABLE [workout_sessions] ADD DEFAULT ((CAST(SYSUTCDATETIME() AS date))) FOR [session_date];
    CREATE INDEX [IX_workout_sessions_user_id_session_date] ON [workout_sessions] ([user_id], [session_date]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    DECLARE @var3 nvarchar(max);
    SELECT @var3 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[weekly_activity]') AND [c].[name] = N'goal_pct');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [weekly_activity] DROP CONSTRAINT ' + @var3 + ';');
    ALTER TABLE [weekly_activity] ALTER COLUMN [goal_pct] int NULL;
    ALTER TABLE [weekly_activity] ADD DEFAULT 0 FOR [goal_pct];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    DECLARE @var4 nvarchar(max);
    SELECT @var4 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[weekly_activity]') AND [c].[name] = N'actual_pct');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [weekly_activity] DROP CONSTRAINT ' + @var4 + ';');
    ALTER TABLE [weekly_activity] ALTER COLUMN [actual_pct] int NULL;
    ALTER TABLE [weekly_activity] ADD DEFAULT 0 FOR [actual_pct];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    DECLARE @var5 nvarchar(max);
    SELECT @var5 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[voice_food_logs]') AND [c].[name] = N'logged_at');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [voice_food_logs] DROP CONSTRAINT ' + @var5 + ';');
    EXEC(N'UPDATE [voice_food_logs] SET [logged_at] = (SYSDATETIMEOFFSET()) WHERE [logged_at] IS NULL');
    ALTER TABLE [voice_food_logs] ALTER COLUMN [logged_at] datetimeoffset NOT NULL;
    ALTER TABLE [voice_food_logs] ADD DEFAULT ((SYSDATETIMEOFFSET())) FOR [logged_at];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    DECLARE @var6 nvarchar(max);
    SELECT @var6 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[user_streaks]') AND [c].[name] = N'freeze_available');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [user_streaks] DROP CONSTRAINT ' + @var6 + ';');
    EXEC(N'UPDATE [user_streaks] SET [freeze_available] = ((1)) WHERE [freeze_available] IS NULL');
    ALTER TABLE [user_streaks] ALTER COLUMN [freeze_available] int NOT NULL;
    ALTER TABLE [user_streaks] ADD DEFAULT (((1))) FOR [freeze_available];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    DECLARE @var7 nvarchar(max);
    SELECT @var7 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[subscriptions]') AND [c].[name] = N'tier');
    IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [subscriptions] DROP CONSTRAINT ' + @var7 + ';');
    EXEC(N'UPDATE [subscriptions] SET [tier] = (N''basic'') WHERE [tier] IS NULL');
    ALTER TABLE [subscriptions] ALTER COLUMN [tier] nvarchar(50) NOT NULL;
    ALTER TABLE [subscriptions] ADD DEFAULT ((N'basic')) FOR [tier];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    DECLARE @var8 nvarchar(max);
    SELECT @var8 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[subscriptions]') AND [c].[name] = N'start_date');
    IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [subscriptions] DROP CONSTRAINT ' + @var8 + ';');
    EXEC(N'UPDATE [subscriptions] SET [start_date] = (CAST(SYSUTCDATETIME() AS date)) WHERE [start_date] IS NULL');
    ALTER TABLE [subscriptions] ALTER COLUMN [start_date] date NOT NULL;
    ALTER TABLE [subscriptions] ADD DEFAULT ((CAST(SYSUTCDATETIME() AS date))) FOR [start_date];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    DECLARE @var9 nvarchar(max);
    SELECT @var9 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[payment_intents]') AND [c].[name] = N'tier');
    IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [payment_intents] DROP CONSTRAINT ' + @var9 + ';');
    EXEC(N'UPDATE [payment_intents] SET [tier] = (N''standard'') WHERE [tier] IS NULL');
    ALTER TABLE [payment_intents] ALTER COLUMN [tier] nvarchar(50) NOT NULL;
    ALTER TABLE [payment_intents] ADD DEFAULT ((N'standard')) FOR [tier];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    DECLARE @var10 nvarchar(max);
    SELECT @var10 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[payment_intents]') AND [c].[name] = N'status');
    IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [payment_intents] DROP CONSTRAINT ' + @var10 + ';');
    EXEC(N'UPDATE [payment_intents] SET [status] = (N''pending'') WHERE [status] IS NULL');
    ALTER TABLE [payment_intents] ALTER COLUMN [status] nvarchar(50) NOT NULL;
    ALTER TABLE [payment_intents] ADD DEFAULT ((N'pending')) FOR [status];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    DECLARE @var11 nvarchar(max);
    SELECT @var11 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[payment_intents]') AND [c].[name] = N'currency');
    IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [payment_intents] DROP CONSTRAINT ' + @var11 + ';');
    EXEC(N'UPDATE [payment_intents] SET [currency] = (N''usd'') WHERE [currency] IS NULL');
    ALTER TABLE [payment_intents] ALTER COLUMN [currency] nvarchar(10) NOT NULL;
    ALTER TABLE [payment_intents] ADD DEFAULT ((N'usd')) FOR [currency];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    DROP INDEX [IX_nutrition_logs_user_id_logged_date] ON [nutrition_logs];
    DECLARE @var12 nvarchar(max);
    SELECT @var12 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[nutrition_logs]') AND [c].[name] = N'logged_date');
    IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [nutrition_logs] DROP CONSTRAINT ' + @var12 + ';');
    EXEC(N'UPDATE [nutrition_logs] SET [logged_date] = (CAST(SYSUTCDATETIME() AS date)) WHERE [logged_date] IS NULL');
    ALTER TABLE [nutrition_logs] ALTER COLUMN [logged_date] date NOT NULL;
    ALTER TABLE [nutrition_logs] ADD DEFAULT ((CAST(SYSUTCDATETIME() AS date))) FOR [logged_date];
    CREATE INDEX [IX_nutrition_logs_user_id_logged_date] ON [nutrition_logs] ([user_id], [logged_date]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    DECLARE @var13 nvarchar(max);
    SELECT @var13 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[food_scans]') AND [c].[name] = N'scanned_at');
    IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [food_scans] DROP CONSTRAINT ' + @var13 + ';');
    EXEC(N'UPDATE [food_scans] SET [scanned_at] = (SYSDATETIMEOFFSET()) WHERE [scanned_at] IS NULL');
    ALTER TABLE [food_scans] ALTER COLUMN [scanned_at] datetimeoffset NOT NULL;
    ALTER TABLE [food_scans] ADD DEFAULT ((SYSDATETIMEOFFSET())) FOR [scanned_at];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    DECLARE @var14 nvarchar(max);
    SELECT @var14 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[daily_summary]') AND [c].[name] = N'workout_duration');
    IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [daily_summary] DROP CONSTRAINT ' + @var14 + ';');
    ALTER TABLE [daily_summary] ADD DEFAULT 0 FOR [workout_duration];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    DECLARE @var15 nvarchar(max);
    SELECT @var15 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[daily_summary]') AND [c].[name] = N'workout_done');
    IF @var15 IS NOT NULL EXEC(N'ALTER TABLE [daily_summary] DROP CONSTRAINT ' + @var15 + ';');
    ALTER TABLE [daily_summary] ADD DEFAULT CAST(0 AS bit) FOR [workout_done];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    DECLARE @var16 nvarchar(max);
    SELECT @var16 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[daily_summary]') AND [c].[name] = N'water_ml');
    IF @var16 IS NOT NULL EXEC(N'ALTER TABLE [daily_summary] DROP CONSTRAINT ' + @var16 + ';');
    ALTER TABLE [daily_summary] ADD DEFAULT 0 FOR [water_ml];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    DECLARE @var17 nvarchar(max);
    SELECT @var17 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[daily_summary]') AND [c].[name] = N'steps');
    IF @var17 IS NOT NULL EXEC(N'ALTER TABLE [daily_summary] DROP CONSTRAINT ' + @var17 + ';');
    ALTER TABLE [daily_summary] ADD DEFAULT 0 FOR [steps];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    DECLARE @var18 nvarchar(max);
    SELECT @var18 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[daily_summary]') AND [c].[name] = N'protein_g');
    IF @var18 IS NOT NULL EXEC(N'ALTER TABLE [daily_summary] DROP CONSTRAINT ' + @var18 + ';');
    ALTER TABLE [daily_summary] ADD DEFAULT 0.0 FOR [protein_g];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    DECLARE @var19 nvarchar(max);
    SELECT @var19 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[daily_summary]') AND [c].[name] = N'fat_g');
    IF @var19 IS NOT NULL EXEC(N'ALTER TABLE [daily_summary] DROP CONSTRAINT ' + @var19 + ';');
    ALTER TABLE [daily_summary] ADD DEFAULT 0.0 FOR [fat_g];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    DECLARE @var20 nvarchar(max);
    SELECT @var20 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[daily_summary]') AND [c].[name] = N'carbs_g');
    IF @var20 IS NOT NULL EXEC(N'ALTER TABLE [daily_summary] DROP CONSTRAINT ' + @var20 + ';');
    ALTER TABLE [daily_summary] ADD DEFAULT 0.0 FOR [carbs_g];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    DECLARE @var21 nvarchar(max);
    SELECT @var21 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[daily_summary]') AND [c].[name] = N'calories_consumed');
    IF @var21 IS NOT NULL EXEC(N'ALTER TABLE [daily_summary] DROP CONSTRAINT ' + @var21 + ';');
    ALTER TABLE [daily_summary] ADD DEFAULT 0.0 FOR [calories_consumed];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    DECLARE @var22 nvarchar(max);
    SELECT @var22 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[daily_summary]') AND [c].[name] = N'calories_burned');
    IF @var22 IS NOT NULL EXEC(N'ALTER TABLE [daily_summary] DROP CONSTRAINT ' + @var22 + ';');
    ALTER TABLE [daily_summary] ADD DEFAULT 0 FOR [calories_burned];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    DECLARE @var23 nvarchar(max);
    SELECT @var23 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[daily_summary]') AND [c].[name] = N'active_minutes');
    IF @var23 IS NOT NULL EXEC(N'ALTER TABLE [daily_summary] DROP CONSTRAINT ' + @var23 + ';');
    ALTER TABLE [daily_summary] ADD DEFAULT 0 FOR [active_minutes];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    DECLARE @var24 nvarchar(max);
    SELECT @var24 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[daily_activity]') AND [c].[name] = N'synced_at');
    IF @var24 IS NOT NULL EXEC(N'ALTER TABLE [daily_activity] DROP CONSTRAINT ' + @var24 + ';');
    EXEC(N'UPDATE [daily_activity] SET [synced_at] = (SYSDATETIMEOFFSET()) WHERE [synced_at] IS NULL');
    ALTER TABLE [daily_activity] ALTER COLUMN [synced_at] datetimeoffset NOT NULL;
    ALTER TABLE [daily_activity] ADD DEFAULT ((SYSDATETIMEOFFSET())) FOR [synced_at];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    DECLARE @var25 nvarchar(max);
    SELECT @var25 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[daily_activity]') AND [c].[name] = N'heart_rate_avg');
    IF @var25 IS NOT NULL EXEC(N'ALTER TABLE [daily_activity] DROP CONSTRAINT ' + @var25 + ';');
    ALTER TABLE [daily_activity] ALTER COLUMN [heart_rate_avg] decimal(6,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    DECLARE @var26 nvarchar(max);
    SELECT @var26 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[daily_activity]') AND [c].[name] = N'exercise_minutes');
    IF @var26 IS NOT NULL EXEC(N'ALTER TABLE [daily_activity] DROP CONSTRAINT ' + @var26 + ';');
    ALTER TABLE [daily_activity] ALTER COLUMN [exercise_minutes] decimal(6,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    DROP INDEX [IX_daily_activity_user_id_activity_date] ON [daily_activity];
    DECLARE @var27 nvarchar(max);
    SELECT @var27 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[daily_activity]') AND [c].[name] = N'activity_date');
    IF @var27 IS NOT NULL EXEC(N'ALTER TABLE [daily_activity] DROP CONSTRAINT ' + @var27 + ';');
    EXEC(N'UPDATE [daily_activity] SET [activity_date] = (CAST(SYSUTCDATETIME() AS date)) WHERE [activity_date] IS NULL');
    ALTER TABLE [daily_activity] ALTER COLUMN [activity_date] date NOT NULL;
    ALTER TABLE [daily_activity] ADD DEFAULT ((CAST(SYSUTCDATETIME() AS date))) FOR [activity_date];
    CREATE INDEX [IX_daily_activity_user_id_activity_date] ON [daily_activity] ([user_id], [activity_date]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    DECLARE @var28 nvarchar(max);
    SELECT @var28 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[coaches]') AND [c].[name] = N'is_active');
    IF @var28 IS NOT NULL EXEC(N'ALTER TABLE [coaches] DROP CONSTRAINT ' + @var28 + ';');
    EXEC(N'UPDATE [coaches] SET [is_active] = ((1)) WHERE [is_active] IS NULL');
    ALTER TABLE [coaches] ALTER COLUMN [is_active] bit NOT NULL;
    ALTER TABLE [coaches] ADD DEFAULT (((1))) FOR [is_active];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    DECLARE @var29 nvarchar(max);
    SELECT @var29 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[coaches]') AND [c].[name] = N'bio');
    IF @var29 IS NOT NULL EXEC(N'ALTER TABLE [coaches] DROP CONSTRAINT ' + @var29 + ';');
    EXEC(N'UPDATE [coaches] SET [bio] = (N'''') WHERE [bio] IS NULL');
    ALTER TABLE [coaches] ALTER COLUMN [bio] nvarchar(max) NOT NULL;
    ALTER TABLE [coaches] ADD DEFAULT ((N'')) FOR [bio];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    DECLARE @var30 nvarchar(max);
    SELECT @var30 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[coach_onboarding]') AND [c].[name] = N'specialization');
    IF @var30 IS NOT NULL EXEC(N'ALTER TABLE [coach_onboarding] DROP CONSTRAINT ' + @var30 + ';');
    EXEC(N'UPDATE [coach_onboarding] SET [specialization] = N'''' WHERE [specialization] IS NULL');
    ALTER TABLE [coach_onboarding] ALTER COLUMN [specialization] nvarchar(max) NOT NULL;
    ALTER TABLE [coach_onboarding] ADD DEFAULT N'' FOR [specialization];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    DECLARE @var31 nvarchar(max);
    SELECT @var31 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[coach_onboarding]') AND [c].[name] = N'price_premium');
    IF @var31 IS NOT NULL EXEC(N'ALTER TABLE [coach_onboarding] DROP CONSTRAINT ' + @var31 + ';');
    EXEC(N'UPDATE [coach_onboarding] SET [price_premium] = 0.0 WHERE [price_premium] IS NULL');
    ALTER TABLE [coach_onboarding] ALTER COLUMN [price_premium] decimal(10,2) NOT NULL;
    ALTER TABLE [coach_onboarding] ADD DEFAULT 0.0 FOR [price_premium];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    DECLARE @var32 nvarchar(max);
    SELECT @var32 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[coach_onboarding]') AND [c].[name] = N'price_monthly');
    IF @var32 IS NOT NULL EXEC(N'ALTER TABLE [coach_onboarding] DROP CONSTRAINT ' + @var32 + ';');
    EXEC(N'UPDATE [coach_onboarding] SET [price_monthly] = 0.0 WHERE [price_monthly] IS NULL');
    ALTER TABLE [coach_onboarding] ALTER COLUMN [price_monthly] decimal(10,2) NOT NULL;
    ALTER TABLE [coach_onboarding] ADD DEFAULT 0.0 FOR [price_monthly];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    DECLARE @var33 nvarchar(max);
    SELECT @var33 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[coach_onboarding]') AND [c].[name] = N'max_clients');
    IF @var33 IS NOT NULL EXEC(N'ALTER TABLE [coach_onboarding] DROP CONSTRAINT ' + @var33 + ';');
    EXEC(N'UPDATE [coach_onboarding] SET [max_clients] = ((10)) WHERE [max_clients] IS NULL');
    ALTER TABLE [coach_onboarding] ALTER COLUMN [max_clients] int NOT NULL;
    ALTER TABLE [coach_onboarding] ADD DEFAULT (((10))) FOR [max_clients];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    DECLARE @var34 nvarchar(max);
    SELECT @var34 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[coach_onboarding]') AND [c].[name] = N'languages');
    IF @var34 IS NOT NULL EXEC(N'ALTER TABLE [coach_onboarding] DROP CONSTRAINT ' + @var34 + ';');
    EXEC(N'UPDATE [coach_onboarding] SET [languages] = (N''["Arabic","English"]'') WHERE [languages] IS NULL');
    ALTER TABLE [coach_onboarding] ALTER COLUMN [languages] nvarchar(max) NOT NULL;
    ALTER TABLE [coach_onboarding] ADD DEFAULT ((N'["Arabic","English"]')) FOR [languages];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    DECLARE @var35 nvarchar(max);
    SELECT @var35 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[coach_onboarding]') AND [c].[name] = N'display_name');
    IF @var35 IS NOT NULL EXEC(N'ALTER TABLE [coach_onboarding] DROP CONSTRAINT ' + @var35 + ';');
    EXEC(N'UPDATE [coach_onboarding] SET [display_name] = (N'''') WHERE [display_name] IS NULL');
    ALTER TABLE [coach_onboarding] ALTER COLUMN [display_name] nvarchar(200) NOT NULL;
    ALTER TABLE [coach_onboarding] ADD DEFAULT ((N'')) FOR [display_name];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    DECLARE @var36 nvarchar(max);
    SELECT @var36 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[coach_onboarding]') AND [c].[name] = N'certifications');
    IF @var36 IS NOT NULL EXEC(N'ALTER TABLE [coach_onboarding] DROP CONSTRAINT ' + @var36 + ';');
    EXEC(N'UPDATE [coach_onboarding] SET [certifications] = N'''' WHERE [certifications] IS NULL');
    ALTER TABLE [coach_onboarding] ALTER COLUMN [certifications] nvarchar(max) NOT NULL;
    ALTER TABLE [coach_onboarding] ADD DEFAULT N'' FOR [certifications];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    DECLARE @var37 nvarchar(max);
    SELECT @var37 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[coach_onboarding]') AND [c].[name] = N'bio');
    IF @var37 IS NOT NULL EXEC(N'ALTER TABLE [coach_onboarding] DROP CONSTRAINT ' + @var37 + ';');
    EXEC(N'UPDATE [coach_onboarding] SET [bio] = (N'''') WHERE [bio] IS NULL');
    ALTER TABLE [coach_onboarding] ALTER COLUMN [bio] nvarchar(max) NOT NULL;
    ALTER TABLE [coach_onboarding] ADD DEFAULT ((N'')) FOR [bio];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    DECLARE @var38 nvarchar(max);
    SELECT @var38 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[client_assignments]') AND [c].[name] = N'assigned_at');
    IF @var38 IS NOT NULL EXEC(N'ALTER TABLE [client_assignments] DROP CONSTRAINT ' + @var38 + ';');
    EXEC(N'UPDATE [client_assignments] SET [assigned_at] = (SYSDATETIMEOFFSET()) WHERE [assigned_at] IS NULL');
    ALTER TABLE [client_assignments] ALTER COLUMN [assigned_at] datetimeoffset NOT NULL;
    ALTER TABLE [client_assignments] ADD DEFAULT ((SYSDATETIMEOFFSET())) FOR [assigned_at];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    DROP INDEX [IX_body_measurements_user_id_measured_date] ON [body_measurements];
    DECLARE @var39 nvarchar(max);
    SELECT @var39 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[body_measurements]') AND [c].[name] = N'measured_date');
    IF @var39 IS NOT NULL EXEC(N'ALTER TABLE [body_measurements] DROP CONSTRAINT ' + @var39 + ';');
    EXEC(N'UPDATE [body_measurements] SET [measured_date] = (CAST(SYSUTCDATETIME() AS date)) WHERE [measured_date] IS NULL');
    ALTER TABLE [body_measurements] ALTER COLUMN [measured_date] date NOT NULL;
    ALTER TABLE [body_measurements] ADD DEFAULT ((CAST(SYSUTCDATETIME() AS date))) FOR [measured_date];
    CREATE INDEX [IX_body_measurements_user_id_measured_date] ON [body_measurements] ([user_id], [measured_date]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    DECLARE @var40 nvarchar(max);
    SELECT @var40 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[barcode_scan_history]') AND [c].[name] = N'scanned_at');
    IF @var40 IS NOT NULL EXEC(N'ALTER TABLE [barcode_scan_history] DROP CONSTRAINT ' + @var40 + ';');
    EXEC(N'UPDATE [barcode_scan_history] SET [scanned_at] = (SYSDATETIMEOFFSET()) WHERE [scanned_at] IS NULL');
    ALTER TABLE [barcode_scan_history] ALTER COLUMN [scanned_at] datetimeoffset NOT NULL;
    ALTER TABLE [barcode_scan_history] ADD DEFAULT ((SYSDATETIMEOFFSET())) FOR [scanned_at];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    DECLARE @var41 nvarchar(max);
    SELECT @var41 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[barcode_scan_history]') AND [c].[name] = N'barcode');
    IF @var41 IS NOT NULL EXEC(N'ALTER TABLE [barcode_scan_history] DROP CONSTRAINT ' + @var41 + ';');
    ALTER TABLE [barcode_scan_history] ALTER COLUMN [barcode] nvarchar(50) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    DECLARE @var42 nvarchar(max);
    SELECT @var42 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[barcode_products]') AND [c].[name] = N'confidence');
    IF @var42 IS NOT NULL EXEC(N'ALTER TABLE [barcode_products] DROP CONSTRAINT ' + @var42 + ';');
    EXEC(N'UPDATE [barcode_products] SET [confidence] = (N''high'') WHERE [confidence] IS NULL');
    ALTER TABLE [barcode_products] ALTER COLUMN [confidence] nvarchar(50) NOT NULL;
    ALTER TABLE [barcode_products] ADD DEFAULT ((N'high')) FOR [confidence];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    EXEC(N'ALTER TABLE [workout_sessions] ADD CONSTRAINT [CK_workout_sessions_muscle_group] CHECK ([muscle_group] IN (N''chest'', N''arms'', N''legs'', N''core'', N''back'', N''shoulders'', N''full_body''))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    EXEC(N'ALTER TABLE [weekly_activity] ADD CONSTRAINT [CK_weekly_activity_actual_pct] CHECK ([actual_pct] IS NULL OR ([actual_pct] >= 0 AND [actual_pct] <= 100))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    EXEC(N'ALTER TABLE [weekly_activity] ADD CONSTRAINT [CK_weekly_activity_day_index] CHECK ([day_index] >= 0 AND [day_index] <= 6)');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    EXEC(N'ALTER TABLE [weekly_activity] ADD CONSTRAINT [CK_weekly_activity_goal_pct] CHECK ([goal_pct] IS NULL OR ([goal_pct] >= 0 AND [goal_pct] <= 100))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    EXEC(N'ALTER TABLE [voice_food_logs] ADD CONSTRAINT [CK_voice_food_logs_confidence] CHECK ([confidence] IS NULL OR [confidence] IN (N''low'', N''medium'', N''high''))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    EXEC(N'ALTER TABLE [user_programs] ADD CONSTRAINT [CK_user_programs_muscle_group] CHECK ([muscle_group] IN (N''chest'', N''arms'', N''legs'', N''core''))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_user_goals_user_id] ON [user_goals] ([user_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_user_active_program_user_id] ON [user_active_program] ([user_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    EXEC(N'ALTER TABLE [training_programs] ADD CONSTRAINT [CK_training_programs_goal] CHECK ([goal] IS NULL OR [goal] IN (N''strength'', N''muscle_gain'', N''weight_loss'', N''endurance'', N''general_fitness''))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    EXEC(N'ALTER TABLE [training_programs] ADD CONSTRAINT [CK_training_programs_level] CHECK ([level] IS NULL OR [level] IN (N''beginner'', N''intermediate'', N''advanced''))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    EXEC(N'ALTER TABLE [subscriptions] ADD CONSTRAINT [CK_subscriptions_payment_status] CHECK ([payment_status] IS NULL OR [payment_status] IN (N''unpaid'', N''paid'', N''refunded''))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    EXEC(N'ALTER TABLE [subscriptions] ADD CONSTRAINT [CK_subscriptions_tier] CHECK ([tier] IN (N''basic'', N''standard'', N''premium''))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    EXEC(N'ALTER TABLE [subscription_phases] ADD CONSTRAINT [CK_subscription_phases_status] CHECK ([status] IS NULL OR [status] IN (N''upcoming'', N''in_progress'', N''completed''))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    EXEC(N'ALTER TABLE [subscription_phases] ADD CONSTRAINT [CK_subscription_phases_type] CHECK ([type] IS NULL OR [type] IN (N''workout'', N''nutrition'', N''combined''))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_stripe_customers_stripe_customer_id] ON [stripe_customers] ([stripe_customer_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_stripe_customers_user_id] ON [stripe_customers] ([user_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    EXEC(N'ALTER TABLE [reviews] ADD CONSTRAINT [CK_reviews_rating] CHECK ([rating] >= 1 AND [rating] <= 5)');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    EXEC(N'ALTER TABLE [profiles] ADD CONSTRAINT [CK_profiles_fitness_goal] CHECK ([fitness_goal] IS NULL OR [fitness_goal] IN (N''muscle_gain'', N''weight_loss'', N''endurance'', N''flexibility''))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    EXEC(N'ALTER TABLE [profiles] ADD CONSTRAINT [CK_profiles_gender] CHECK ([gender] IS NULL OR [gender] IN (N''male'', N''female''))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    EXEC(N'ALTER TABLE [payment_intents] ADD CONSTRAINT [CK_payment_intents_status] CHECK ([status] IN (N''pending'', N''succeeded'', N''failed'', N''refunded''))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_onboarding_user_id] ON [onboarding] ([user_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    EXEC(N'ALTER TABLE [onboarding] ADD CONSTRAINT [CK_onboarding_activity_level] CHECK ([activity_level] IS NULL OR [activity_level] IN (N''sedentary'', N''lightly_active'', N''moderately_active'', N''very_active'', N''extra_active''))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    EXEC(N'ALTER TABLE [onboarding] ADD CONSTRAINT [CK_onboarding_gender] CHECK ([gender] IS NULL OR [gender] IN (N''male'', N''female''))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    EXEC(N'ALTER TABLE [onboarding] ADD CONSTRAINT [CK_onboarding_goal] CHECK ([goal] IS NULL OR [goal] IN (N''muscle_gain'', N''weight_loss'', N''endurance'', N''flexibility'', N''general_fitness''))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    EXEC(N'ALTER TABLE [onboarding] ADD CONSTRAINT [CK_onboarding_weekly_workouts] CHECK ([weekly_workouts] IS NULL OR ([weekly_workouts] >= 1 AND [weekly_workouts] <= 7))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    EXEC(N'ALTER TABLE [nutrition_logs] ADD CONSTRAINT [CK_nutrition_logs_meal_type] CHECK ([meal_type] IS NULL OR [meal_type] IN (N''breakfast'', N''lunch'', N''dinner'', N''snack''))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    CREATE INDEX [IX_notifications_coach_id] ON [notifications] ([coach_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    EXEC(N'ALTER TABLE [notifications] ADD CONSTRAINT [CK_notifications_type] CHECK ([type] IN (N''message'', N''plan''))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    EXEC(N'ALTER TABLE [messages] ADD CONSTRAINT [CK_messages_type] CHECK ([type] IN (N''text'', N''image'', N''file'', N''workout_plan'', N''nutrition_plan'', N''voice''))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    EXEC(N'ALTER TABLE [food_scans] ADD CONSTRAINT [CK_food_scans_confidence] CHECK ([confidence] IS NULL OR [confidence] IN (N''low'', N''medium'', N''high''))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    EXEC(N'ALTER TABLE [exercises] ADD CONSTRAINT [CK_exercises_category] CHECK ([category] IS NULL OR [category] IN (N''compound'', N''isolation'', N''cardio'', N''stretching''))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    EXEC(N'ALTER TABLE [exercises] ADD CONSTRAINT [CK_exercises_muscle_group] CHECK ([muscle_group] IS NULL OR [muscle_group] IN (N''chest'', N''back'', N''shoulders'', N''arms'', N''legs'', N''core'', N''full_body'', N''cardio''))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    EXEC(N'ALTER TABLE [daily_summary] ADD CONSTRAINT [CK_daily_summary_mood] CHECK ([mood] IS NULL OR ([mood] >= 1 AND [mood] <= 5))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_coaches_user_id] ON [coaches] ([user_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    EXEC(N'ALTER TABLE [coach_profiles] ADD CONSTRAINT [CK_coach_profiles_rating] CHECK ([rating] IS NULL OR ([rating] >= 0 AND [rating] <= 5))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    EXEC(N'ALTER TABLE [coach_content] ADD CONSTRAINT [CK_coach_content_type] CHECK ([type] IN (N''pdf'', N''video'', N''image''))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    EXEC(N'ALTER TABLE [barcode_products] ADD CONSTRAINT [CK_barcode_products_confidence] CHECK ([confidence] IS NULL OR [confidence] IN (N''low'', N''medium'', N''high''))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    EXEC(N'ALTER TABLE [barcode_products] ADD CONSTRAINT [CK_barcode_products_source] CHECK ([source] IN (N''openfoodfacts'', N''gemini_estimate'', N''manual''))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    ALTER TABLE [notifications] ADD CONSTRAINT [FK_notifications_coach_profiles] FOREIGN KEY ([coach_id]) REFERENCES [profiles] ([id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907002558_AlignWithProdSchema'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260907002558_AlignWithProdSchema', N'10.0.11');
END;

COMMIT;
GO

