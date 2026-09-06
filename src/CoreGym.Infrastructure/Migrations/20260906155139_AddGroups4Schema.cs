using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreGym.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGroups4Schema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "coach_onboarding",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    display_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    years_experience = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    certifications = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    specialization = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    bio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    price_monthly = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    price_premium = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    languages = table.Column<string>(type: "nvarchar(max)", nullable: true, defaultValueSql: "(N'[\"Arabic\",\"English\"]')"),
                    max_clients = table.Column<int>(type: "int", nullable: true, defaultValueSql: "((10))"),
                    profile_image_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    intro_video_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    is_completed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "(SYSDATETIMEOFFSET())"),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "(SYSDATETIMEOFFSET())"),
                    phone_number = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    city = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    gender = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    gallery_images = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    pdf_urls = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    certificate_files = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    transformation_images = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_coach_onboarding", x => x.id);
                    table.CheckConstraint("CK_coach_onboarding_certificate_files_json", "[certificate_files] IS NULL OR ISJSON([certificate_files]) = 1");
                    table.CheckConstraint("CK_coach_onboarding_certifications_json", "[certifications] IS NULL OR ISJSON([certifications]) = 1");
                    table.CheckConstraint("CK_coach_onboarding_gallery_images_json", "[gallery_images] IS NULL OR ISJSON([gallery_images]) = 1");
                    table.CheckConstraint("CK_coach_onboarding_languages_json", "[languages] IS NULL OR ISJSON([languages]) = 1");
                    table.CheckConstraint("CK_coach_onboarding_pdf_urls_json", "[pdf_urls] IS NULL OR ISJSON([pdf_urls]) = 1");
                    table.CheckConstraint("CK_coach_onboarding_specialization_json", "[specialization] IS NULL OR ISJSON([specialization]) = 1");
                    table.CheckConstraint("CK_coach_onboarding_transformation_images_json", "[transformation_images] IS NULL OR ISJSON([transformation_images]) = 1");
                    table.ForeignKey(
                        name: "FK_coach_onboarding_profiles",
                        column: x => x.user_id,
                        principalTable: "profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "coach_profiles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    bio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    bio_ar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    specialties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    certifications = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    experience_years = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    price_per_month = table.Column<decimal>(type: "decimal(10,2)", nullable: false, defaultValue: 0m),
                    currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true, defaultValueSql: "(N'EGP')"),
                    rating = table.Column<decimal>(type: "decimal(3,2)", nullable: false, defaultValue: 0m),
                    reviews_count = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    is_available = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((1))"),
                    is_verified = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    cover_image_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    instagram_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    youtube_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    max_clients = table.Column<int>(type: "int", nullable: true, defaultValueSql: "((20))"),
                    current_clients = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "(SYSDATETIMEOFFSET())"),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "(SYSDATETIMEOFFSET())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_coach_profiles", x => x.id);
                    table.CheckConstraint("CK_coach_profiles_certifications_json", "[certifications] IS NULL OR ISJSON([certifications]) = 1");
                    table.CheckConstraint("CK_coach_profiles_specialties_json", "[specialties] IS NULL OR ISJSON([specialties]) = 1");
                    table.ForeignKey(
                        name: "FK_coach_profiles_profiles",
                        column: x => x.id,
                        principalTable: "profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "coaches",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    bio = table.Column<string>(type: "nvarchar(max)", nullable: true, defaultValueSql: "(N'')"),
                    price_monthly = table.Column<decimal>(type: "decimal(10,2)", nullable: false, defaultValue: 0m),
                    specialization = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    rating = table.Column<decimal>(type: "decimal(3,2)", nullable: false, defaultValue: 0m),
                    is_active = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((1))"),
                    stripe_account_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "(SYSDATETIMEOFFSET())"),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "(SYSDATETIMEOFFSET())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_coaches", x => x.id);
                    table.CheckConstraint("CK_coaches_specialization_json", "[specialization] IS NULL OR ISJSON([specialization]) = 1");
                    table.ForeignKey(
                        name: "FK_coaches_profiles",
                        column: x => x.user_id,
                        principalTable: "profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "stripe_customers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    stripe_customer_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "(SYSDATETIMEOFFSET())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stripe_customers", x => x.id);
                    table.ForeignKey(
                        name: "FK_stripe_customers_profiles",
                        column: x => x.user_id,
                        principalTable: "profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "subscription_plans",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    coach_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    price_usd = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    duration_days = table.Column<int>(type: "int", nullable: true),
                    max_clients = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "(SYSDATETIMEOFFSET())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscription_plans", x => x.id);
                    table.ForeignKey(
                        name: "FK_subscription_plans_profiles",
                        column: x => x.coach_id,
                        principalTable: "profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "coach_content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    coach_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    file_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    is_public = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "(SYSDATETIMEOFFSET())"),
                    thumbnail_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    file_size_kb = table.Column<int>(type: "int", nullable: true),
                    sort_order = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_coach_content", x => x.id);
                    table.ForeignKey(
                        name: "FK_coach_content_coaches",
                        column: x => x.coach_id,
                        principalTable: "coaches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "payment_intents",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    client_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    coach_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    stripe_payment_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    stripe_customer_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    amount = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true, defaultValueSql: "(N'usd')"),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, defaultValueSql: "(N'pending')"),
                    tier = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, defaultValueSql: "(N'standard')"),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "(SYSDATETIMEOFFSET())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_intents", x => x.id);
                    table.ForeignKey(
                        name: "FK_payment_intents_coaches",
                        column: x => x.coach_id,
                        principalTable: "coaches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payment_intents_profiles",
                        column: x => x.client_id,
                        principalTable: "profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "reviews",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    client_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    coach_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    rating = table.Column<int>(type: "int", nullable: false),
                    comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "(SYSDATETIMEOFFSET())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reviews", x => x.id);
                    table.ForeignKey(
                        name: "FK_reviews_coaches",
                        column: x => x.coach_id,
                        principalTable: "coaches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reviews_profiles",
                        column: x => x.client_id,
                        principalTable: "profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "subscriptions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    client_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    coach_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValueSql: "(N'active')"),
                    tier = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, defaultValueSql: "(N'basic')"),
                    start_date = table.Column<DateTime>(type: "date", nullable: true, defaultValueSql: "(CAST(SYSUTCDATETIME() AS date))"),
                    end_date = table.Column<DateTime>(type: "date", nullable: true),
                    stripe_sub_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "(SYSDATETIMEOFFSET())"),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "(SYSDATETIMEOFFSET())"),
                    plan_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    payment_status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    started_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    expires_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    goals = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscriptions", x => x.id);
                    table.CheckConstraint("CK_subscriptions_status", "[status] IN (N'pending', N'active', N'cancelled', N'expired')");
                    table.ForeignKey(
                        name: "FK_subscriptions_coaches",
                        column: x => x.coach_id,
                        principalTable: "coaches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_subscriptions_profiles",
                        column: x => x.client_id,
                        principalTable: "profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_subscriptions_subscription_plans",
                        column: x => x.plan_id,
                        principalTable: "subscription_plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "client_assignments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    coach_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    client_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    content_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    assigned_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_client_assignments", x => x.id);
                    table.ForeignKey(
                        name: "FK_client_assignments_coach_content",
                        column: x => x.content_id,
                        principalTable: "coach_content",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_client_assignments_coaches",
                        column: x => x.coach_id,
                        principalTable: "coaches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_client_assignments_profiles",
                        column: x => x.client_id,
                        principalTable: "profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "subscription_phases",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    subscription_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    phase_number = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    duration_weeks = table.Column<int>(type: "int", nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, defaultValueSql: "(N'upcoming')"),
                    started_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    completed_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, defaultValueSql: "(SYSDATETIMEOFFSET())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscription_phases", x => x.id);
                    table.ForeignKey(
                        name: "FK_subscription_phases_subscriptions",
                        column: x => x.subscription_id,
                        principalTable: "subscriptions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_profiles_role",
                table: "profiles",
                sql: "[role] IN (N'client', N'coach', N'user')");

            migrationBuilder.CreateIndex(
                name: "IX_client_assignments_client_id",
                table: "client_assignments",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "IX_client_assignments_coach_id",
                table: "client_assignments",
                column: "coach_id");

            migrationBuilder.CreateIndex(
                name: "IX_client_assignments_content_id",
                table: "client_assignments",
                column: "content_id");

            migrationBuilder.CreateIndex(
                name: "IX_coach_content_coach_id",
                table: "coach_content",
                column: "coach_id");

            migrationBuilder.CreateIndex(
                name: "IX_coach_onboarding_user_id",
                table: "coach_onboarding",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_coaches_user_id",
                table: "coaches",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_payment_intents_client_id",
                table: "payment_intents",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "IX_payment_intents_coach_id",
                table: "payment_intents",
                column: "coach_id");

            migrationBuilder.CreateIndex(
                name: "IX_payment_intents_stripe_payment_id",
                table: "payment_intents",
                column: "stripe_payment_id");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_client_id",
                table: "reviews",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_coach_id",
                table: "reviews",
                column: "coach_id");

            migrationBuilder.CreateIndex(
                name: "IX_stripe_customers_stripe_customer_id",
                table: "stripe_customers",
                column: "stripe_customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_stripe_customers_user_id",
                table: "stripe_customers",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_subscription_phases_subscription_id",
                table: "subscription_phases",
                column: "subscription_id");

            migrationBuilder.CreateIndex(
                name: "IX_subscription_plans_coach_id",
                table: "subscription_plans",
                column: "coach_id");

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_client_id_coach_id",
                table: "subscriptions",
                columns: new[] { "client_id", "coach_id" });

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_coach_id",
                table: "subscriptions",
                column: "coach_id");

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_plan_id",
                table: "subscriptions",
                column: "plan_id");

            // Replicates the Supabase update_updated_at() trigger on the Group 4
            // tables that have an updated_at column. Recursive triggers are disabled
            // by default in SQL Server, so the inner UPDATE does not re-fire.
            // Each CREATE TRIGGER runs in its own batch (required by T-SQL).
            migrationBuilder.Sql(@"
CREATE TRIGGER dbo.trg_coaches_updated_at
ON dbo.coaches
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE c SET updated_at = SYSDATETIMEOFFSET()
    FROM dbo.coaches c INNER JOIN inserted i ON i.id = c.id;
END");

            migrationBuilder.Sql(@"
CREATE TRIGGER dbo.trg_coach_profiles_updated_at
ON dbo.coach_profiles
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE cp SET updated_at = SYSDATETIMEOFFSET()
    FROM dbo.coach_profiles cp INNER JOIN inserted i ON i.id = cp.id;
END");

            migrationBuilder.Sql(@"
CREATE TRIGGER dbo.trg_coach_onboarding_updated_at
ON dbo.coach_onboarding
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE co SET updated_at = SYSDATETIMEOFFSET()
    FROM dbo.coach_onboarding co INNER JOIN inserted i ON i.id = co.id;
END");

            migrationBuilder.Sql(@"
CREATE TRIGGER dbo.trg_subscriptions_updated_at
ON dbo.subscriptions
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE s SET updated_at = SYSDATETIMEOFFSET()
    FROM dbo.subscriptions s INNER JOIN inserted i ON i.id = s.id;
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "client_assignments");

            migrationBuilder.DropTable(
                name: "coach_onboarding");

            migrationBuilder.DropTable(
                name: "coach_profiles");

            migrationBuilder.DropTable(
                name: "payment_intents");

            migrationBuilder.DropTable(
                name: "reviews");

            migrationBuilder.DropTable(
                name: "stripe_customers");

            migrationBuilder.DropTable(
                name: "subscription_phases");

            migrationBuilder.DropTable(
                name: "coach_content");

            migrationBuilder.DropTable(
                name: "subscriptions");

            migrationBuilder.DropTable(
                name: "coaches");

            migrationBuilder.DropTable(
                name: "subscription_plans");

            migrationBuilder.DropCheckConstraint(
                name: "CK_profiles_role",
                table: "profiles");
        }
    }
}
