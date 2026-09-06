# Architecture

## Solution layout

```
CoreGym.sln
├── src/CoreGym.Domain            entities (42), UserRole enum, authorization abstractions,
│                                 keyless view read models. No EF references — pure domain.
├── src/CoreGym.Infrastructure    CoreGymDbContext, one IEntityTypeConfiguration per entity,
│                                 EF migrations (5), authorization service, design-time factory.
└── tests/CoreGym.Infrastructure.Tests
                                  xUnit integration tests. Every test run creates a uniquely-named
                                  scratch database on (localdb)\MSSQLLocalDB, applies all migrations
                                  via Database.Migrate(), and drops it afterwards.
```

Dependency direction: `Tests → Infrastructure → Domain`. The Domain project carries the
authorization abstractions (`IOwnedResource`, `ICurrentUserService`, `IClientAccessService`)
so entities can implement them without referencing EF or ASP.NET.

## Type-mapping conventions (Postgres → SQL Server)

| Postgres | SQL Server | Notes |
|---|---|---|
| `uuid` | `UNIQUEIDENTIFIER` | No DB default — ids are client-generated (EF emits sequential GUIDs). |
| `text` (names/short) | `NVARCHAR(200)` / `NVARCHAR(50)` / `NVARCHAR(320)` (email) / `NVARCHAR(500)` (URLs) | Realistic limits instead of `MAX` everywhere. |
| `text` (free text) | `NVARCHAR(MAX)` | instructions, descriptions, notes, message content, bio. |
| `timestamptz` | `DATETIMEOFFSET(7)` | Default `SYSDATETIMEOFFSET()` (UTC by definition, type-matched). |
| `date` | `DATE` | Default `CAST(SYSUTCDATETIME() AS date)` (UTC date). |
| `numeric` | `DECIMAL(p,s)` | weights (5,2) · heights/lengths (5,1) · nutrition amounts (8,2) · money (10,2) · ratings (3,2) · volume (10,2) · sleep hours (4,2). |
| `bool` / `int` | `BIT` / `INT` | |
| `text[]` | `NVARCHAR(MAX)` + `ISJSON` check | JSON arrays via an EF value converter + comparer → `List<string>?`. Same API payload shape as Supabase. |
| `jsonb` (`notification_log.data`) | `NVARCHAR(MAX)` + `ISJSON` check | Raw JSON string. |
| `time` | `TIME` | quiet hours. |

### Defaults & nullability rules

- DB default `0`/`false` → non-nullable CLR property (EF sentinel behavior is always equivalent).
- DB default ≠ `0`/`false` (e.g. 2000 kcal, 90 s rest) → **nullable CLR property** + `HasDefaultValueSql`,
  so an explicit `0` is never silently replaced by the database default.
- Columns **explicitly marked `NOT NULL`** in the inventory with a non-zero default
  (e.g. `nutrition_logs.quantity`) → nullable CLR property + `.IsRequired()`:
  the column is NOT NULL while EF still omits it on insert so the default applies.
- Columns with defaults but no explicit nullability in the inventory → nullable column + DB default
  (lenient, safe for the upcoming data migration from Supabase).

### Triggers

The original `update_updated_at()` stamper is replicated as a per-table `AFTER UPDATE`
trigger on every table that has an `updated_at` column (11 tables). Two rules that bit once
and are now conventions:

1. **Every table with a trigger must declare it in EF**: `ToTable(t => t.HasTrigger(...))`.
   Without it, EF's `SaveChanges` emits an `OUTPUT` clause and SQL Server rejects every
   write with error 334.
2. Each `CREATE TRIGGER` runs in its own migration batch (`migrationBuilder.Sql` per trigger)
   because T-SQL requires `CREATE TRIGGER` to be the first statement in a batch.
   Recursive triggers are disabled by default in SQL Server, so the inner `UPDATE` does not re-fire.

### CHECK constraints

All new (the original columns are plain text — there were no Postgres enums):

| Constraint | Values | Source |
|---|---|---|
| `CK_profiles_role` | `client, coach, user` | App writes + legacy `'user'` default still checked by the client |
| `CK_subscriptions_status` | `pending, active, cancelled, expired` | Dart `SubscriptionStatus` enum + stripe-webhook writes |
| `CK_streak_activity_log_source` | `workout, nutrition` | Documented in the inventory |
| `CK_barcode_products_source` | `openfoodfacts, gemini_estimate` | Documented in the inventory |
| `CK_messages_type` | `text, voice, image, file` | Documented in the inventory |
| `CK_exercises_secondary_muscles_json`, `CK_program_days_muscle_groups_json`, coach JSON columns, `CK_notification_log_data_json` | `ISJSON(...)` | JSON columns |

> **Pre-flight before data migration:** run `SELECT DISTINCT` on the source columns
> (`profiles.role`, `subscriptions.status`) to catch stray values in prod data.

### `coach_id` key spaces (intentional, locked by tests)

| Table | `coach_id` references |
|---|---|
| `subscriptions`, `coach_content`, `client_assignments`, `payment_intents`, `reviews` | **`coaches.id`** |
| `subscription_plans`, `conversations` | **`profiles.id` (auth user id)** — `coach_id = auth.uid()` in the original RLS |
| `notifications` | no FK — key space unverified (review checklist) |

## Migrations

| Migration | Contents |
|---|---|
| `InitialSchemaGroups1And2` | profiles, foods, exercises, training_programs, program_days, program_day_exercises, onboarding, user_goals, user_streaks, body_measurements |
| `AddGroups3Schema` | 16 daily nutrition/workout/activity tables |
| `AddGroups4Schema` | 11 coach/subscription/payment tables + `CK_profiles_role` |
| `AddPhase1Views` | the three views |
| `AddGroups5Schema` | 5 chat/notification tables |

Review scripts: `SQL/001`–`005` (idempotent, generated per unit) and
`SQL/supabase_backport_reviews_table.sql` (the canonical `reviews` table was created
manually in prod and is missing from Supabase's local migration history).

## Testing approach

- **Schema-shape test** reads `INFORMATION_SCHEMA.COLUMNS` and asserts every column of all
  42 tables (type, length, precision, nullability) against the inventory transcription,
  plus 57 indexes and 58 FKs by name.
- **Behavior tests** exercise defaults, FK enforcement (error 547), PK violations (2627),
  NOT NULL (515), CHECK rejections, `updated_at` trigger restamping, JSON round-trips
  (including Arabic text), and Restrict-delete blocking — through the real `DbContext`.
- **Authorization tests** drive the real `IAuthorizationService` pipeline.
- Run everything with `dotnet test` — the fixture auto-starts LocalDB and cleans up.
