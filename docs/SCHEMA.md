# Schema inventory

42 of the original 44 Supabase tables are ported. Two legacy duplicates were
**deliberately dropped** (see [MIGRATION_PROGRESS.md](../MIGRATION_PROGRESS.md)):
`coach_reviews` and `coach_subscriptions` — any rows living in them in prod will
not migrate.

## Group 1 — Foundations (6)

| Table | Notes |
|---|---|
| `profiles` | One row per user; `id` = auth user id. `role` CHECK (`client`/`coach`/`user`). |
| `foods` | Food catalog; `created_by` FK for custom foods. |
| `exercises` | Reference data; `secondary_muscles` JSON. |
| `training_programs` | Program templates. |
| `program_days` | `program_id` FK; `muscle_groups` JSON. |
| `program_day_exercises` | Ordered prescription; composite index `(program_day_id, order_index)`. |

## Group 2 — User data (4)

| Table | Notes |
|---|---|
| `onboarding` | Signup questionnaire. |
| `user_goals` | Daily nutrition/activity targets with defaults (2000 kcal, 150 g protein, …). |
| `user_streaks` | PK = `user_id`; written only by the streak logic (Phase 2). |
| `body_measurements` | Indexed `(user_id, measured_date)`. |

## Group 3 — Daily nutrition & workouts (16)

| Table | Notes |
|---|---|
| `nutrition_logs` | `quantity` NOT NULL default 1; optional `food_id`. |
| `daily_summary` | Per-day rollup; fed by sync triggers (Phase 2); `updated_at` trigger. |
| `daily_activity` | Health Connect sync; no delete by design. |
| `streak_activity_log` | Append-only; `source` CHECK (`workout`/`nutrition`). |
| `workout_sessions` | |
| `workout_sets` | Denormalized by `exercise_name`; index `(user_id, exercise_name)` serves personal records. |
| `exercise_progress` | Per-session bests. |
| `user_active_program` | Pointer to the followed template; `updated_at` trigger. |
| `user_programs` | Custom user programs. |
| `barcode_products` | PK = barcode string; `source` CHECK; `updated_at` trigger. |
| `barcode_scan_history` | FKs to products + nutrition logs. |
| `food_scans` / `food_scan_items` | AI photo-scan output. |
| `voice_food_logs` / `voice_food_log_items` | AI voice-log output. |
| `weekly_activity` | Weekly chart data. |

## Group 4 — Coaches, subscriptions, payments (11)

| Table | Notes |
|---|---|
| `coaches` | `user_id` FK; `specialization` JSON; `updated_at` trigger. |
| `coach_profiles` | **PK = user id**; aggregates rating/current clients. |
| `coach_onboarding` | 7 JSON array columns; `languages` defaults to `["Arabic","English"]`. |
| `coach_content` | Coach-published files. |
| `client_assignments` | Content assignments. |
| `reviews` | **Canonical** (legacy `coach_reviews` dropped); read by dashboard + coach detail. |
| `subscriptions` | **Canonical** (legacy `coach_subscriptions` dropped); `status` CHECK; `coach_id` → `coaches.id`; `updated_at` trigger. Queried by the Authorization Service. |
| `subscription_plans` | `coach_id` → **user id** (per original RLS). |
| `subscription_phases` | |
| `payment_intents` | Stripe webhook target; indexed by `stripe_payment_id`. |
| `stripe_customers` | Written by checkout flow. |

## Group 5 — Chat & notifications (5)

| Table | Notes |
|---|---|
| `conversations` | Both `client_id` and `coach_id` are **user ids**; optional `subscription_id`. |
| `messages` | `type` CHECK (`text`/`voice`/`image`/`file`); chat-history index. |
| `notifications` | `coach_id` has no FK (key space unverified — review checklist). |
| `notification_preferences` | PK = `user_id`; quiet hours as `TIME`; `updated_at` trigger. |
| `notification_log` | jsonb `data` → `ISJSON`-checked JSON; insert is server-side only. |

## Views (keyless EF read models)

| View | Definition (inferred — see review checklist) |
|---|---|
| `personal_records` | Per (user, exercise): best weight / reps / set volume / Epley 1RM / last logged, warmups excluded. |
| `weekly_progress` | Per (user, week): days logged, days goal met, average actual/goal %. |
| `weight_progress` | Weight history with `LAG`-based change per measurement. |

Mapped in `Domain/ReadModels` + `ReadModelConfigurations` — query via
`context.PersonalRecords` etc. No row-level filtering inside the views;
caller filtering belongs to the Authorization Service.

## Integrity summary

- **58 foreign keys** — all `ON DELETE NO ACTION` (Restrict) pending confirmation of the
  original cascade rules (review checklist). Plausible cascade candidates: the
  `*_scan_items` / `*_log_items` child tables.
- **57 indexes** — every `user_id`/FK column, plus composites matching real query patterns:
  `(user_id, logged_date)`, `(user_id, summary_date)`, `(user_id, exercise_name)`,
  `(conversation_id, created_at)`, `(client_id, coach_id)`, …
- **11 `updated_at` triggers** replicating the original stamper.
- **6 value CHECKs + 9 JSON `ISJSON` checks.**
