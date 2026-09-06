# CoreGym — .NET Migration Brief (Phase 1: Schema + Authorization)

## Context
CoreGym (a fitness/nutrition/coaching app) currently runs on Supabase (PostgreSQL). The goal is to migrate the backend to **.NET** using **SQL Server**. This brief covers **Phase 1 only**: the **Database Schema + Authorization Service**. Other features (AI-powered Edge Functions, chat, notifications...) are deferred to later phases.

---

## Phase 1 Goals
1. Build a complete SQL Server schema matching the existing Supabase tables (adjusting types to fit SQL Server).
2. Build a .NET Authorization Service (policy-based authorization) that replaces the RLS logic in Postgres — specifically the recurring pattern: "a user can see their own data" + "a coach can see a client's data if there's an active subscription."
3. **Do not** build any Controllers/Endpoints or other business logic in this phase — focus only on the data and auth layers.

---

## Suggested Table Order (build in this order)

### Group 1 — Foundations (few foreign keys)
- `profiles`
- `foods`
- `exercises`
- `training_programs`
- `program_days`
- `program_day_exercises`

### Group 2 — User data linked to profiles
- `onboarding`
- `user_goals`
- `user_streaks`
- `body_measurements`

### Group 3 — Daily nutrition and workout data
- `nutrition_logs`
- `daily_summary`
- `daily_activity`
- `streak_activity_log`
- `workout_sessions`
- `workout_sets`
- `exercise_progress`
- `user_active_program`
- `user_programs`
- `barcode_products`
- `barcode_scan_history`
- `food_scans` / `food_scan_items`
- `voice_food_logs` / `voice_food_log_items`
- `weekly_activity`

### Group 4 — Coaches (most complex, do this last)
- `coaches`
- `coach_profiles`
- `coach_onboarding`
- `coach_content`
- `client_assignments`
- `coach_reviews` / `reviews` ⚠️ **Decision needed before starting — see "Points to Resolve" below**
- `coach_subscriptions` / `subscriptions` ⚠️ **Same issue, likely duplicated**
- `subscription_plans`
- `subscription_phases`
- `payment_intents`
- `stripe_customers`

### Group 5 — Chat and notifications (do this last, newest/least tested part)
- `conversations`
- `messages`
- `notifications`
- `notification_preferences`
- `notification_log`

> The **Views** (`personal_records`, `weekly_progress`, `weight_progress`) should become regular SQL Views or computed queries in the Repository layer — handle these after the core tables are done.

---

## Type Conversion Notes (Postgres → SQL Server)
- `uuid` → `UNIQUEIDENTIFIER`
- `text` → `NVARCHAR(MAX)` or `NVARCHAR(n)` depending on usage (most are short names/descriptions — consider realistic length limits instead of MAX everywhere)
- `timestamptz` → `DATETIMEOFFSET`
- `numeric` → `DECIMAL(precision, scale)` — pick appropriate precision per column (weight, calories, percentages...)
- `text[]` (arrays like `secondary_muscles`, `specialties`, `languages`) — SQL Server has no direct array type, two options:
  - A separate normalized table — better if you need to query the values
  - `NVARCHAR(MAX)` as JSON — faster to port, weaker for querying
- `jsonb` (`notification_log.data`) → `NVARCHAR(MAX)` with validation, or use SQL Server's JSON functions
- `enum` types (`user_role`, `subscription_status`) → either a SQL Server `CHECK constraint` on an `NVARCHAR` column, or a separate lookup table — CHECK constraint is simpler and recommended here
- All `default now()` → `DEFAULT SYSUTCDATETIME()` or `GETUTCDATE()` (pick one and use it consistently across the whole project)

---

## Authorization Service — Exact Requirements

### Core logic to replace (instead of RLS)
Every table with personal user data follows this fixed pattern in the original file:
```
-- user can see/edit only their own data
USING (auth.uid() = user_id)

-- coach can see a client's data if there's an active subscription
USING (user_id = auth.uid() OR is_my_active_client(user_id))
```

### Proposed .NET design
1. **`ICurrentUserService`** — retrieves the current user id from the JWT/Claims (equivalent to `auth.uid()`)
2. **`IClientAccessService`** with a method like:
   ```csharp
   Task<bool> IsActiveClientOfCoach(Guid coachUserId, Guid clientUserId);
   ```
   This replaces `is_my_active_client()` exactly — should be a single query against the active-subscriptions table (once we've resolved which table is correct: `subscriptions` or `coach_subscriptions`)
3. **Custom Authorization Handler** (ASP.NET Core `IAuthorizationHandler`) using this service, tied to a policy named e.g. `"OwnDataOrActiveCoach"`, applied to any resource with a `user_id`
4. **Must be reusable** — not duplicated per endpoint, since the original file specifically flagged this as the most important thing to preserve correctly

### Tables that will use this policy specifically (same ones that used `is_my_active_client`)
- `body_measurements`
- `daily_summary`
- `exercise_progress`
- `nutrition_logs`
- `workout_sessions`
- `workout_sets`

---

## Points to Resolve Before Starting the Coaches Group (Group 4)
The original file flagged these as open items — get answers before building on the wrong table:

1. **Two parallel review systems exist**: `reviews` (linked to `coaches`) and `coach_reviews` (linked to `coach_profiles`). Confirm with the team/current app which one is actually in use, and only port that one to .NET.
2. **Same issue with subscriptions**: `subscriptions` and `coach_subscriptions` — two similar tables, determine which is the canonical one.
3. **Stripe integration status**: no Edge Function was found handling Stripe webhooks despite `payment_intents` and `stripe_customers` existing. Confirm payments are actually functional before building on them in .NET, or treat this as a fully separate phase.

---

## Reference Material to Provide the Agent
Attach the full original **Supabase Backend Inventory** document (the complete table/RLS/functions/edge-functions/storage report) alongside this brief. The agent needs the full picture — including the parts deferred to later phases (Edge Functions, chat/notification triggers, Stripe) — so it understands *why* the current phase is scoped the way it is and doesn't accidentally contradict something planned for later. This brief is the "what to do now and in what order"; the inventory file is the full source of truth.

---

## Mandatory Workflow Rules for the Agent

### 1. Maintain a persistent progress file
At the start of the very first session, the agent must create a `MIGRATION_PROGRESS.md` file in the repo (or update it if it already exists) and keep it current after every unit of work. It should track, at minimum:
- Which tables/entities are done (schema + EF config + migration applied)
- Which are in progress
- Which are blocked (and why — e.g. waiting on a decision)
- Which open questions from this brief are still unresolved
- A short "resume here" note describing the exact next step

This file is the resume point for any future session — a new session (or a new agent instance) should be able to read it and continue without re-reading the whole conversation history.

### 2. Walkthrough + verification after each unit of work
After finishing each table group (or each table, if it's a complex one like the coaches group), the agent must:
- Do a short walkthrough of what it built (schema, constraints, indexes, relationships)
- Write and run a basic test/endpoint check that the table and its constraints actually work as expected (e.g. a smoke-test endpoint or integration test hitting the new entity) — not just "the migration ran without errors"
- Report results before moving to the next unit

### 3. Always ask before continuing
The agent should **not** silently proceed from one group/table to the next. After finishing a unit and reporting it, it must explicitly ask for confirmation before starting the next one. No large multi-group work should happen in a single unreviewed pass.

### 4. Report progress incrementally
After every completed piece of work, the agent reports plainly: what got done, what changed, and what's next — before asking to proceed. Don't batch several completed items into one big end-of-session summary.

### 5. Focus areas for every table/feature, not just "does it work"
For every table, entity, and endpoint built, explicitly check and report on:
- **Performance**: appropriate indexes (especially on `user_id`/foreign keys used in the Authorization Service queries), query patterns that avoid N+1, pagination on list endpoints
- **Security**: the Authorization policy is actually applied (not just written), no endpoint exposes another user's data by omission, input validation matches the original NOT NULL/default constraints
- **Data integrity**: foreign keys, cascade rules (or intentional lack thereof) match the intent of the original schema

### 6. Original instructions still apply
- Start with Groups 1 and 2 only in the first run, and submit the schema (as SQL migration scripts or EF Core migrations, depending on your choice) for review before continuing to the other groups.
- For each table, produce the Entity/Model class + EF Core configuration (if using EF Core) alongside the raw SQL.
- Flag immediately if any column or data type is unclear on how to convert (e.g. arrays), rather than assuming a decision on your own.
- Do not start Group 4 (Coaches) until the three questions above have been resolved.
- Build the Authorization Service described above once the `subscriptions`/`coach_subscriptions` table (post-resolution) and the `coaches` table are complete.
