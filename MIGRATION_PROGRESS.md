# CoreGym .NET Migration — Progress File

> Resume point for any session. Read this file first, then the brief
> (`docs/source-documents/coregym-dotnet-migration-brief-phase1-en.md`) and the source of truth
> (`docs/source-documents/coregym-migration-inventory.md`).
>
> Last updated: 2026-09-05 — Session 1 (Phase 1)

## Project

CoreGym backend migration: Supabase (PostgreSQL 17) → .NET 10 + SQL Server.
Phase 1 scope: **database schema + authorization service only** — no controllers/endpoints, no business logic.

Solution layout:

```
F:\core_project\
  CoreGym.sln
  src\CoreGym.Domain\            entities, enums, authorization abstractions (IOwnedResource, ICurrentUserService, IClientAccessService), view read models — no EF references
  src\CoreGym.Infrastructure\    DbContext, per-entity configurations, EF migrations, Authorization service (CurrentUser/ClientAccess/OwnDataOrActiveCoach policy), design-time factory
  tests\CoreGym.Infrastructure.Tests\  xUnit integration tests against a scratch LocalDB database
  SQL\                           generated idempotent SQL scripts (one per migration, for review)
  MIGRATION_PROGRESS.md          this file
```

## Decisions made (with rationale)

### App decisions (2026-09-06, confirmed against the Flutter repo by the owner)
| Topic | Decision |
|---|---|
| Reviews | **`reviews` is canonical; `coach_reviews` is NOT ported.** The app reads `reviews` in exactly two places (coach dashboard stats, coach detail screen); zero references to `coach_reviews`. `reviews` was created manually in prod (absent from local migration files) → backport script at `SQL/supabase_backport_reviews_table.sql` (CREATE TABLE IF NOT EXISTS, no invented FKs). |
| Subscriptions | **`subscriptions` is canonical; `coach_subscriptions` is NOT ported.** Used by 5 repository queries, dashboard client list, and the stripe-webhook function. The Authorization Service must query `subscriptions` (client_id / coach_id / status). |
| Stripe | **Port `payment_intents` + `stripe_customers` now.** Flow is coded and active: `create-checkout-session` (writes stripe_customers), `get-subscription-status`, `stripe-webhook` (updates subscriptions.status, inserts payment_intents). Residual check: confirm the functions are deployed/env vars set on current prod (`supabase functions list`). |
| CHECK constraints | **New constraints** (original columns are plain text, no PG enums): `profiles.role IN ('client','coach','user')` — 'user' is the legacy default still checked in profile_provider.dart:70; `subscriptions.status IN ('pending','active','cancelled','expired')` — matches the Dart SubscriptionStatus enum + webhook writes. **Pre-flight before data migration:** run `SELECT DISTINCT role FROM profiles` / `SELECT DISTINCT status FROM subscriptions` on prod to catch stray values. |
| coach_id key spaces | `subscriptions.coach_id`, `coach_content.coach_id`, `client_assignments.coach_id`, `payment_intents.coach_id`, `reviews.coach_id` → **coaches.id**; `subscription_plans.coach_id` → **profiles.id (auth user id)** (`coach_id = auth.uid()` in its RLS). Locked by test `SubscriptionPlan_coach_id_is_a_user_id_not_a_coaches_id`. |
| Dropped data | Rows living in `coach_reviews` / `coach_subscriptions` in prod will NOT be migrated — export/decide before cutover if they contain anything valuable. |

| Topic | Decision |
|---|---|
| Stack | .NET 10, EF Core 10 (SQL Server provider), xUnit integration tests, SQL Server LocalDB `MSSQLLocalDB` for verification |
| Schema approach | EF Core code-first migrations + generated idempotent SQL script per unit of work (so both review styles work) |
| `uuid` | `UNIQUEIDENTIFIER`; no DB default (matches inventory — ids are client-generated; EF generates sequential GUIDs client-side) |
| `text` | `NVARCHAR` with realistic limits: names/full names/fitness goals 200, email 320, short enum-like text (gender, goal, level, category, meal_type, serving_unit, source, type, equipment...) 50, URLs/paths 500, free text (instructions, descriptions, notes, tips) `NVARCHAR(MAX)`, `role` 20 |
| `timestamptz` | `DATETIMEOFFSET(7)`, default `SYSDATETIMEOFFSET()` — deliberate deviation from the brief's offered `SYSUTCDATETIME()`/`GETUTCDATE()`: those return `datetime2`/`datetime` and would rely on an implicit timezone conversion; `SYSDATETIMEOFFSET()` is UTC by definition and type-matches the column. Used consistently everywhere. |
| `date` | `DATE`, default `CAST(SYSUTCDATETIME() AS date)` (UTC date, consistent with the timestamp decision) |
| `numeric` | `DECIMAL(p,s)`: weights/target weights (5,2), heights/circumferences (5,1)/(5,2), nutrition amounts (calories/macros) (8,2), serving sizes (6,2), sleep hours (4,2) |
| `bool` / `int` | `BIT` / `INT` |
| `text[]` | `NVARCHAR(MAX)` JSON + `ISJSON` check constraint (**user decision 2026-09-05**). EF maps via value converter + value comparer to `List<string>?`, so API payload shape stays identical to Supabase. Revisit per-table in Group 4 if a coach-filter query needs normalized tables. |
| `enum` | `NVARCHAR` + CHECK constraint. **`user_role`: CHECK deferred** (**user decision 2026-09-05** — values unconfirmed); column is `NVARCHAR(20)` default `'client'`, EF converts enum ↔ lowercase string. |
| `updated_at` stamping | Replicated the `update_updated_at()` trigger as a per-table `AFTER UPDATE` trigger (guarded against no-op updates). Applied to every Group 1–2 table that has `updated_at`: `profiles`, `onboarding`, `user_goals`, `user_streaks`. |
| Foreign keys | **Inferred from column names — the inventory does not list FK constraints.** All set to `ON DELETE NO ACTION` (Restrict) pending confirmation. See "FK assumptions" below. |
| Nullability | Detailed-format tables (e.g. `profiles`): as stated in inventory. Compressed-format tables: **(a)** columns explicitly marked `NOT NULL` → NOT NULL column (when they also carry a non-zero default, e.g. `nutrition_logs.quantity`, the CLR property stays nullable + `IsRequired()` so EF omits it on insert and the DB default applies); **(b)** columns with defaults but no explicit nullability → nullable column + DB default (lenient, safe for data migration); **(c)** id/FK columns (user_id, program_id, scan_id, log_id...) treated as NOT NULL; everything else nullable. Flagged for review. |
| CLR property shape | DB default 0/false → non-nullable property (EF sentinel behavior is then always equivalent). DB default ≠ 0/false (e.g. 2000 kcal, 90s rest) → **nullable** CLR property so an explicit `0` is never silently replaced by the DB default. |
| EF + DB triggers | Every table carrying a SQL Server trigger (e.g. the `updated_at` stampers) **must** declare it via `ToTable(t => t.HasTrigger(...))` in its EF configuration. Without it, EF's default `SaveChanges` uses an `OUTPUT` clause and SQL Server rejects every insert/update with error 334. Caught and verified by smoke tests. |

## Table status

Legend: `[x]` done (schema + EF config + migration + tests) · `[~]` in progress · `[ ]` todo · `[B]` blocked

### Group 1 — Foundations
- [x] `profiles`
- [x] `foods`
- [x] `exercises` (secondary_muscles → JSON)
- [x] `training_programs`
- [x] `program_days` (muscle_groups → JSON)
- [x] `program_day_exercises`

### Group 2 — User data linked to profiles
- [x] `onboarding`
- [x] `user_goals`
- [x] `user_streaks` (PK = user_id)
- [x] `body_measurements`

### Group 3 — Daily nutrition and workout data
- [x] `nutrition_logs`
- [x] `daily_summary` (updated_at trigger)
- [x] `daily_activity`
- [x] `streak_activity_log` (source CHECK: 'workout'|'nutrition')
- [x] `workout_sessions`
- [x] `workout_sets`
- [x] `exercise_progress`
- [x] `user_active_program` (updated_at trigger)
- [x] `user_programs`
- [x] `barcode_products` (PK = barcode string; source CHECK: 'openfoodfacts'|'gemini_estimate'; updated_at trigger)
- [x] `barcode_scan_history`
- [x] `food_scans` / `food_scan_items`
- [x] `voice_food_logs` / `voice_food_log_items`
- [x] `weekly_activity`

### Group 4 — Coaches
- [x] `coaches` (updated_at trigger; specialization JSON)
- [x] `coach_profiles` (PK = user id; updated_at trigger; specialties/certifications JSON)
- [x] `coach_onboarding` (updated_at trigger; 7 JSON array columns; languages default `["Arabic","English"]`)
- [x] `coach_content`
- [x] `client_assignments`
- [x] `reviews` — canonical (see decisions); `coach_reviews` **not ported**
- [x] `subscriptions` — canonical (status CHECK; updated_at trigger); `coach_subscriptions` **not ported**
- [x] `subscription_plans` (coach_id = user id!)
- [x] `subscription_phases`
- [x] `payment_intents` (Stripe ported now — decision 2026-09-06)
- [x] `stripe_customers`

### Group 5 — Chat and notifications
- [x] `conversations` (client_id AND coach_id both keyed on user ids; subscription_id nullable FK)
- [x] `messages` (type CHECK: 'text'|'voice'|'image'|'file'; consolidates the duplicated-RLS era schema)
- [x] `notifications` (coach_id has NO FK — key space unverified, open question 12)
- [x] `notification_preferences` (PK = user_id; updated_at trigger; time-typed quiet hours)
- [x] `notification_log` (jsonb data → NVARCHAR(MAX) + ISJSON check)

### Other Phase 1 work
- [x] Views: `personal_records`, `weekly_progress`, `weight_progress` — created in migration `AddPhase1Views` with **inferred definitions** (see open question 11); mapped as keyless EF read models (`Domain/ReadModels`)
- [x] Authorization Service — `ICurrentUserService` (user id from JWT claims), `IClientAccessService.IsActiveClientOfCoach` (single query on canonical `subscriptions` joining coaches), reusable resource-based policy `OwnDataOrActiveCoach` (`AuthorizationSetup.AddCoreGymAuthorization()`), applied via `IOwnedResource` on the six own-data+coach tables: body_measurements, daily_summary, exercise_progress, nutrition_logs, workout_sessions, workout_sets
- Deferred to later phases: Edge Functions (AI/notifications/Stripe port), business-logic triggers (streak RPCs `record_daily_activity`/`get_streak_status`, summary sync, chat triggers, `handle_subscription_accepted` → conversations + `coach_profiles.current_clients`, `update_coach_rating`/`refresh_coach_rating` → reviews → coaches.rating + coach_profiles.rating), storage buckets, pg_cron jobs

## Open questions

1. ~~`user_role` enum values~~ **RESOLVED 2026-09-06** — no PG enum; plain text. Values in use: 'client', 'coach', legacy 'user'. CHECK `CK_profiles_role` added (includes 'user'). Pre-flight `SELECT DISTINCT role FROM profiles` on prod before data migration.
2. ~~`reviews` vs `coach_reviews`~~ **RESOLVED 2026-09-06** — `reviews` is canonical; `coach_reviews` not ported.
3. ~~`subscriptions` vs `coach_subscriptions`~~ **RESOLVED 2026-09-06** — `subscriptions` is canonical; the Authorization Service queries it.
4. ~~Stripe integration status~~ **RESOLVED 2026-09-06** — port now. Residual: verify functions are deployed on current prod (`supabase functions list`).
5. **FK assumptions (Groups 1–4) to confirm at review:**
   - `program_days.program_id` → `training_programs.id`
   - `program_day_exercises.program_day_id` → `program_days.id`; `.exercise_id` → `exercises.id`
   - `foods.created_by` → `profiles.id`
   - `onboarding.user_id`, `user_goals.user_id`, `user_streaks.user_id`, `body_measurements.user_id` → `profiles.id`
   - `nutrition_logs.user_id` → profiles, `.food_id` → foods
   - `daily_summary`/`daily_activity`/`streak_activity_log`/`workout_sessions`/`workout_sets`/`exercise_progress`/`user_active_program`/`user_programs`/`barcode_scan_history`/`food_scans`/`voice_food_logs`/`weekly_activity` `.user_id` → profiles
   - `workout_sets.session_id` → workout_sessions; `exercise_progress.exercise_id` → exercises, `.session_id` → workout_sessions
   - `user_active_program.program_id` → training_programs
   - `barcode_scan_history.barcode` → barcode_products, `.nutrition_log_id` → nutrition_logs
   - `food_scan_items.scan_id` → food_scans, `.nutrition_log_id` → nutrition_logs
   - `voice_food_log_items.log_id` → voice_food_logs, `.nutrition_log_id` → nutrition_logs
   - `coaches.user_id` → profiles; `coach_profiles.id` → profiles (PK = user id); `coach_onboarding.user_id` → profiles
   - `coach_content.coach_id` → coaches; `client_assignments.coach_id` → coaches, `.client_id` → profiles, `.content_id` → coach_content
   - `reviews.client_id` → profiles, `.coach_id` → coaches
   - `subscriptions.client_id` → profiles, `.coach_id` → coaches, `.plan_id` → subscription_plans; `subscription_phases.subscription_id` → subscriptions
   - `subscription_plans.coach_id` → **profiles** (user id, see decisions)
   - `payment_intents.client_id` → profiles, `.coach_id` → coaches; `stripe_customers.user_id` → profiles
   - All `ON DELETE NO ACTION`. Confirm whether the original has FKs at all / different cascade rules (esp. `*_scan_items`/`*_log_items` child tables — cascade is plausible there).
6. **Unique constraints beyond PKs** — not listed in inventory. Candidates to confirm: `daily_summary (user_id, summary_date)`, `user_active_program (user_id)`, `user_goals (user_id)`, `coaches (user_id)` (one coach row per user?), `stripe_customers (stripe_customer_id)` and/or `(user_id)` unique?
7. **Auth provider** — what replaces Supabase Auth (JWT issuer/claims shape) is undefined; needed before `ICurrentUserService`/Authorization Service can be finalized (next unit of work).
8. ~~`subscription_status` enum values~~ **RESOLVED 2026-09-06** — no PG enum; values pending/active/cancelled/expired. CHECK `CK_subscriptions_status` added. Pre-flight `SELECT DISTINCT status FROM subscriptions` on prod before data migration.
9. **`foods.category`** — looks enumerated (default `'other'`) but values unknown; no CHECK for now.
10. **`payment_intents.amount` units** — the inventory doesn't state whether it stores Stripe cents or major units; modeled as `DECIMAL(12,2)` either way. Confirm when porting the webhook function.
11. **View definitions are INFERRED** (2026-09-06): the inventory lists `personal_records` / `weekly_progress` / `weight_progress` but not their SQL. Built from the underlying tables: personal_records = per (user, exercise_name) bests from `workout_sets` (max weight, max reps, max set volume, Epley 1RM estimate, last logged_at; warmups excluded); weekly_progress = per (user, week_start) day counts + averages from `weekly_activity`; weight_progress = weight history from `body_measurements` with LAG-based change. **Validate with `SELECT pg_get_viewdef('public.personal_records')` etc. on prod and paste the results** — I will align the SQL Server definitions if they differ.
12. **`notifications.coach_id` key space unverified** (chat flow suggests user id, but the original never disambiguates) — modeled as a soft reference with NO FK so nothing breaks either way. Confirm during the chat-phase port, then add the FK.
13. **`conversations.subscription_id` assumed nullable** (chats can start outside an accepted-subscription flow per the insert RLS). Confirm with app behavior.

## Session log

### 2026-09-05 — Session 1
- Read brief + inventory; resolved text[] (JSON) and user_role (defer CHECK) with user.
- Scaffolded solution; built Groups 1 + 2 (10 tables): entities, EF configurations, migration `InitialSchemaGroups1And2`, idempotent SQL script `SQL/001_initial_schema_groups_1_2.sql`.
- Replicated `update_updated_at()` as per-table AFTER UPDATE triggers on `profiles`, `onboarding`, `user_goals`, `user_streaks`; declared via `HasTrigger` so EF writes work.
- Integration smoke tests: **14/14 passed** against a scratch LocalDB database —
  - schema shape: every column of all 10 tables vs the inventory (type, length, nullability), plus expected indexes, PKs and FKs;
  - behavior: DB defaults (profile role/name/email, user_goals targets, streaks, serving size/unit/category, is_active), explicit-zero preservation on user_goals, FK enforcement (bogus program_id rejected, error 547), Restrict delete blocking, NOT NULL (foods.calories, error 515), JSON roundtrip incl. Arabic text, ISJSON check rejection, measured_date UTC default, updated_at trigger restamping.
- **Awaiting:** user review of Groups 1–2 schema (entities + `SQL/001_initial_schema_groups_1_2.sql`) and confirmation of the FK/cascade assumptions in open question 5.
### 2026-09-05 — Session 1 (continued)
- **Group 3 built and verified (16 tables).** Migration `AddGroups3Schema` + idempotent script `SQL/002_groups_3_schema.sql`.
- Added `updated_at` triggers on `daily_summary`, `user_active_program`, `barcode_products` (declared via `HasTrigger`).
- Added CHECK constraints on the two sources with inventory-documented value lists: `streak_activity_log.source` ('workout'|'nutrition') and `barcode_products.source` ('openfoodfacts'|'gemini_estimate').
- New convention recorded: explicitly-NOT-NULL columns with non-zero defaults use nullable CLR + `IsRequired()` (e.g. `nutrition_logs.quantity`); defaulted-but-unmarked columns stay nullable (lenient, matches Groups 1–2).
- Tests: **25/25 passed** — schema shape now asserts all 26 tables' columns/types/nullability, 30 indexes, 31 FKs; behavior tests cover nutrition defaults + food link, source CHECK rejections, daily activity defaults, daily_summary/user_active_program trigger restamping, workout session↔sets chain + Restrict delete, barcode PK duplicate (2627) + CHECK + defaults, food-scan and voice-log item chains incl. Arabic text, exercise_progress defaults, weekly_activity roundtrip.
- **Next:** user review → Group 4 (coaches) — **blocked** until open questions 2–4 are answered (which reviews table, which subscriptions table, Stripe status) and 8 (`subscription_status` enum values) is answered for the CHECK constraint.

### 2026-09-06 — Session 1 (continued)
- Owner resolved all Group 4 blockers against the Flutter repo (see decisions table above).
- **Group 4 built and verified (11 tables ported; `coach_reviews`/`coach_subscriptions` deliberately dropped).** Migration `AddGroups4Schema` + idempotent script `SQL/003_groups_4_schema.sql` + Supabase backport `SQL/supabase_backport_reviews_table.sql`.
- Added the NEW CHECK constraints: `CK_profiles_role` (client/coach/user) and `CK_subscriptions_status` (pending/active/cancelled/expired).
- Added `updated_at` triggers on `coaches`, `coach_profiles`, `coach_onboarding`, `subscriptions`.
- Tests: **34/34 passed** — schema shape now asserts all 37 tables, 48 indexes, 48 FKs; Group 4 behavior tests lock the role/status CHECK values (incl. legacy 'user'), coach_profiles PK = user id, subscription_plans.coach_id = user id vs subscriptions.coach_id = coaches.id, coach_onboarding languages default, payment/stripe-customer defaults, review chain, assignment Restrict delete.
- **Next:** Authorization Service (`ICurrentUserService`, `IClientAccessService` querying `subscriptions`, `OwnDataOrActiveCoach` policy) — awaiting go-ahead; auth-provider question (7) open but service is buildable against ClaimsPrincipal.

### 2026-09-06 — Session 1 (continued 2)
- **Views built** (migration `AddPhase1Views`, script `SQL/004_phase1_views.sql`): `personal_records`, `weekly_progress`, `weight_progress` with inferred definitions (open question 11), mapped to keyless EF read models and verified by aggregation/exclusion/isolation tests.
- **Authorization Service built** (the brief's highest-priority item): `ICurrentUserService` (reads NameIdentifier or 'sub' claim — IdP-agnostic until open question 7 is settled), `IClientAccessService.IsActiveClientOfCoach` (single no-tracking query: subscriptions ⋈ coaches on user_id, `status = 'active'`, exactly `is_my_active_client()`), and the resource-based `OwnDataOrActiveCoach` policy wired once via `AuthorizationSetup.AddCoreGymAuthorization()`; the six own-data+coach entities implement `IOwnedResource`. Verified through the real `IAuthorizationService` pipeline: owner ✓, coach with active sub ✓, pending/cancelled/expired ✗, stranger ✗, principal without user-id claim ✗, and exact coach↔client pair matching.
- Tests: **45/45 passed** (37 tables, 48 indexes, 48 FKs asserted + behavior + views + authorization).
- **Next:** Group 5 — chat and notifications (5 tables: conversations, messages, notifications, notification_preferences, notification_log) — the last remaining Phase 1 schema work. Newest/least-tested part per the brief; consolidate the duplicated conversation/message RLS patterns into one clean policy in the service layer later.

### 2026-09-06 — Session 1 (continued 3) — PHASE 1 SCHEMA COMPLETE
- **Group 5 built and verified (5 tables).** Migration `AddGroups5Schema` + idempotent script `SQL/005_groups_5_schema.sql`.
- `messages.type` CHECK ('text'|'voice'|'image'|'file'), `notification_log.data` jsonb → ISJSON-checked JSON, `notification_preferences` updated_at trigger + time-typed quiet hours, chat indexes on (conversation_id, created_at).
- Key-space findings locked by tests: `conversations.coach_id` = user id (like subscription_plans); `notifications.coach_id` deliberately FK-less until confirmed (open question 12).
- Tests: **50/50 passed** — schema shape asserts all 42 ported tables' columns/types/nullability, 57 indexes, 58 FKs; behavior coverage now includes chat chains, type CHECK rejection, preferences trigger + quiet-hours roundtrip, notification JSON payloads.
- **Phase 1 status: 42 of the original 44 tables ported** (coach_reviews and coach_subscriptions intentionally dropped — decisions 2026-09-06), plus 3 views, 11 updated_at triggers, 7 CHECK constraints, and the Authorization Service.
- **Remaining before Phase 1 sign-off (all recorded above):** (a) validate inferred view definitions vs prod, (b) confirm FK/cascade + unique-constraint assumptions (open questions 5, 6, 13), (c) run the SELECT DISTINCT pre-flights on prod before data migration, (d) confirm notifications.coach_id key space (12), (e) deployability check of the Stripe functions on current prod.
- **Next session:** Phase 1 final review → then Phase 2 (business-logic triggers/RPCs: streaks, summary sync, chat/notification triggers, subscription-accepted hook, coach rating refresh) and/or the API layer.

### 2026-09-06 — Session 2 — PHASE 2 COMPLETE (business logic)
- **Design decision:** the original SECURITY DEFINER functions/triggers/RPCs are reproduced as **.NET application services** (`Infrastructure/Services`, interfaces in `Domain/Services`), not SQL Server triggers — testable, and HTTP push can't live in a trigger anyway. DB triggers remain only for the mechanical `updated_at` stampers.
- Seven services registered via `AddCoreGymApplicationServices()`:
  - `IStreakService` — `record_daily_activity` / `get_streak_status` / freeze monthly reset (consecutive-day increments, one-missed-day forgiveness consuming `freeze_available`, reset otherwise, idempotent per source/day).
  - `IDailySummaryService` — `sync_nutrition_to_summary` / `sync_workout_to_summary` (upsert; creates a summary row only when data exists).
  - `IMessagingService` — `notify_new_message` + `update_conversation_on_message` (type-aware preview, unread counters) + `mark_conversation_read` + `unread_count` (participant-only enforcement).
  - `INotificationService` — `mark_notification_read` / `mark_all_notifications_read` (own-only).
  - `ISubscriptionLifecycleService` — `handle_subscription_accepted` (status transition, conversation create/update, `current_clients` increment/decrement floored at 0, idempotent transitions). The future Stripe webhook endpoint calls this.
  - `ICoachRatingService` — `update_coach_rating` / `refresh_coach_rating` (average over reviews; 0 when none).
  - `IProfileProvisioningService` — `handle_new_user` (idempotent profile row).
- Tests: **81/81 passed** (31 new). Three test bugs were caught and fixed along the way (shared-DB assumptions in older tests after the messaging tests started populating tables).
- **Inferred semantics flagged** (validate via `pg_get_functiondef` on prod): streak rule details, chat notification `type = 'chat_message'`, preview wording. See docs/BUSINESS_LOGIC.md.
- **Deferred to Phase 3:** API layer + auth integration, Stripe webhook HTTP receiver, OneSignal push, AI functions (analyze-food, log-food-voice/text, lookup-barcode), meal reminders, scheduler host for the freeze reset, file storage.
