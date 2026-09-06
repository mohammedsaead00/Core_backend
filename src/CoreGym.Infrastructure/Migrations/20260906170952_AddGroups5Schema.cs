using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreGym.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGroups5Schema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "conversations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    client_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    coach_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    subscription_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    last_message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    last_message_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "(SYSDATETIMEOFFSET())"),
                    client_unread = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    coach_unread = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    is_active = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((1))"),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "(SYSDATETIMEOFFSET())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conversations", x => x.id);
                    table.ForeignKey(
                        name: "FK_conversations_coach_profiles",
                        column: x => x.coach_id,
                        principalTable: "profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_conversations_profiles",
                        column: x => x.client_id,
                        principalTable: "profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_conversations_subscriptions",
                        column: x => x.subscription_id,
                        principalTable: "subscriptions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notification_log",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    data = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sent_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "(SYSDATETIMEOFFSET())"),
                    read_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_log", x => x.id);
                    table.CheckConstraint("CK_notification_log_data_json", "[data] IS NULL OR ISJSON([data]) = 1");
                    table.ForeignKey(
                        name: "FK_notification_log_profiles",
                        column: x => x.user_id,
                        principalTable: "profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notification_preferences",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    meal_reminders_enabled = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((1))"),
                    water_reminders_enabled = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((1))"),
                    calorie_alerts_enabled = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((1))"),
                    chat_notifications_enabled = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((1))"),
                    quiet_hours_start = table.Column<TimeSpan>(type: "time", nullable: true),
                    quiet_hours_end = table.Column<TimeSpan>(type: "time", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "(SYSDATETIMEOFFSET())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_preferences", x => x.user_id);
                    table.ForeignKey(
                        name: "FK_notification_preferences_profiles",
                        column: x => x.user_id,
                        principalTable: "profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    conversation_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    sender_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValueSql: "(N'text')"),
                    file_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    is_read = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    is_deleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "(SYSDATETIMEOFFSET())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_messages", x => x.id);
                    table.CheckConstraint("CK_messages_type", "[type] IN (N'text', N'voice', N'image', N'file')");
                    table.ForeignKey(
                        name: "FK_messages_conversations",
                        column: x => x.conversation_id,
                        principalTable: "conversations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_messages_profiles",
                        column: x => x.sender_id,
                        principalTable: "profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    conversation_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    plan_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    coach_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    is_read = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "(SYSDATETIMEOFFSET())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.id);
                    table.ForeignKey(
                        name: "FK_notifications_conversations",
                        column: x => x.conversation_id,
                        principalTable: "conversations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_notifications_profiles",
                        column: x => x.user_id,
                        principalTable: "profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_notifications_subscription_plans",
                        column: x => x.plan_id,
                        principalTable: "subscription_plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_conversations_client_id",
                table: "conversations",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "IX_conversations_coach_id",
                table: "conversations",
                column: "coach_id");

            migrationBuilder.CreateIndex(
                name: "IX_conversations_subscription_id",
                table: "conversations",
                column: "subscription_id");

            migrationBuilder.CreateIndex(
                name: "IX_messages_conversation_id_created_at",
                table: "messages",
                columns: new[] { "conversation_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_messages_sender_id",
                table: "messages",
                column: "sender_id");

            migrationBuilder.CreateIndex(
                name: "IX_notification_log_user_id_sent_at",
                table: "notification_log",
                columns: new[] { "user_id", "sent_at" });

            migrationBuilder.CreateIndex(
                name: "IX_notifications_conversation_id",
                table: "notifications",
                column: "conversation_id");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_plan_id",
                table: "notifications",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_user_id_created_at",
                table: "notifications",
                columns: new[] { "user_id", "created_at" });

            // Replicates the Supabase update_updated_at() trigger on the only
            // Group 5 table with an updated_at column. Recursive triggers are
            // disabled by default in SQL Server, so the inner UPDATE does not re-fire.
            // CREATE TRIGGER runs in its own batch (required by T-SQL).
            migrationBuilder.Sql(@"
CREATE TRIGGER dbo.trg_notification_preferences_updated_at
ON dbo.notification_preferences
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE p SET updated_at = SYSDATETIMEOFFSET()
    FROM dbo.notification_preferences p INNER JOIN inserted i ON i.user_id = p.user_id;
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "messages");

            migrationBuilder.DropTable(
                name: "notification_log");

            migrationBuilder.DropTable(
                name: "notification_preferences");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "conversations");
        }
    }
}
