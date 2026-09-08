# Phase 1 review checklist

Open items to confirm before production cutover. Full context and history in
[MIGRATION_PROGRESS.md](../MIGRATION_PROGRESS.md).

## Must confirm before data migration

- [ ] **Pre-flight on prod (Supabase):** `SELECT DISTINCT role FROM profiles;` — must return only
      `client`, `coach`, `user` or the new `CK_profiles_role` CHECK will reject rows.
- [ ] **Pre-flight on prod (Supabase):** `SELECT DISTINCT status FROM subscriptions;` — must return only
      `pending`, `active`, `cancelled`, `expired` or `CK_subscriptions_status` will reject rows.
- [ ] **Validate inferred view definitions** against prod:
      `SELECT pg_get_viewdef('public.personal_records');` (and `weekly_progress`, `weight_progress`).
      If they differ from the inferred definitions in `docs/SCHEMA.md`, the `AddPhase1Views`
      migration is the single place to align.
- [ ] **Export-or-drop decision for the dropped tables:** `coach_reviews` and `coach_subscriptions`
      contain prod data that will NOT migrate.

## Schema assumptions to confirm

- [x] **Unique constraints beyond PKs** — RESOLVED 2026-09-07 from the live prod schema dump:
      `onboarding.user_id`, `user_goals.user_id`, `user_active_program.user_id`, `coaches.user_id`,
      `stripe_customers.user_id` and `stripe_customers.stripe_customer_id` are all UNIQUE — now
      enforced in the port (migration `AlignWithProdSchema`).
- [x] **`notifications.coach_id` key space** — RESOLVED 2026-09-07: the prod dump shows it references
      `profiles(id)` (a user id) **with a foreign key** — FK added.
- [ ] **`subscription_plans.coach_id` CONTRADICTION in the prod schema** — the FK references
      `coaches(id)` but the RLS policy compares `coach_id = auth.uid()` (a user id). Both cannot be
      satisfied by real data unless coaches.id ever equals the user id. The port currently follows
      the RLS (FK → profiles). Resolve with: `SELECT COUNT(*) FROM subscription_plans sp JOIN
      coaches c ON sp.coach_id = c.id` vs `JOIN profiles p ON sp.coach_id = p.id` — then either flip
      the FK to coaches or fix the app query.
- [ ] **`conversations.subscription_id` nullability** — assumed nullable (chats can start
      outside an accepted-subscription flow). The prod dump confirms nullable ✓.
- [ ] **`payment_intents.amount` units** — Stripe cents or major currency units? Modeled as
      `DECIMAL(12,2)` either way; confirm when porting the webhook function.
- [x] **CHECK constraint value lists** — taken from the live prod dump (2026-09-07): gender,
      fitness_goal, onboarding (goal/activity_level/weekly_workouts), meal_type, muscle groups
      (user_programs/workout_sessions/exercises), exercises.category, training level/goal, mood 1–5,
      reviews.rating 1–5, subscriptions.tier/payment_status, payment_intents.status,
      coach_content.type, subscription_phases.type/status, coach_profiles.rating 0–5, confidence
      columns, weekly_activity day_index/pcts, daily_activity numeric types, `messages.type` (6
      values incl. `workout_plan`/`nutrition_plan` — the port was wrongly rejecting those),
      `barcode_products.source` (incl. `manual`), `notifications.type` (`message`|`plan` — the
      messaging service now writes `message`).
- [x] **`barcode_scan_history.barcode`** — nullable with NO FK in the prod dump; the port now matches
      (a scan can exist before the product is cached).
- [ ] **`profiles.full_name` defaults to the `name` column** in prod (`DEFAULT name`) — SQL Server
      cannot express a column-reference default; the app/API must set `full_name` explicitly.
- [x] **`notifications.plan_id`** — no FK in the prod dump; the port's extra FK was removed.
- [x] **Meal reminder windows** — prod cron fires 06:00/12:00/18:00 UTC (08:00/14:00/20:00 Cairo) ✓
      matches the ported scheduler; quiet hours evaluated in Cairo local time.

## Environment checks

- [ ] **Stripe functions deployed:** the code exists in the app repo
      (`create-checkout-session`, `get-subscription-status`, `stripe-webhook`) but confirm
      they are deployed with env vars set on the *current* prod project
      (`supabase functions list`).
- [ ] **Auth provider decision:** what replaces Supabase Auth (JWT issuer/claims shape).
      `UserIdClaimReader` is the single place to adjust claim mapping.
- [ ] **`foods.category` values** — looks enumerated (default `'other'`) but the value set is
      unknown; no CHECK yet.

## Done (resolved during Phase 1)

- ✅ `reviews` is canonical; `coach_reviews` dropped (zero app references)
- ✅ `subscriptions` is canonical; `coach_subscriptions` dropped
- ✅ Stripe tables ported now (flow is coded and active)
- ✅ `user_role` / `subscription_status` values confirmed against the app; CHECKs added
- ✅ `text[]` → JSON columns; `updated_at` triggers replicated; `HasTrigger` EF requirement handled
