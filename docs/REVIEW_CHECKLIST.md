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

- [ ] **FKs & cascade rules** — all 58 FKs are inferred from column names and set to
      `ON DELETE NO ACTION`. Confirm whether the original has FKs at all, and whether any
      should cascade (most plausible: `food_scan_items` → `food_scans`,
      `voice_food_log_items` → `voice_food_logs`).
- [ ] **Unique constraints beyond PKs** — candidates: `daily_summary (user_id, summary_date)`,
      `user_active_program (user_id)`, `user_goals (user_id)`, `coaches (user_id)`,
      `stripe_customers (stripe_customer_id)`.
- [ ] **`notifications.coach_id` key space** — user id or `coaches.id`? Currently a soft
      reference with no FK. Confirm during the chat-phase port, then add the FK.
- [ ] **`conversations.subscription_id` nullability** — assumed nullable (chats can start
      outside an accepted-subscription flow).
- [ ] **`payment_intents.amount` units** — Stripe cents or major currency units? Modeled as
      `DECIMAL(12,2)` either way; confirm when porting the webhook function.

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
