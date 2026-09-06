# CoreGym — Supabase Backend Inventory (for .NET Migration)
Generated directly from the live project via Supabase MCP on 2026-09-05.
Project ref: `mkrjvrnysuvtokqkyoll` | Region: eu-west-1 | Postgres 17

---

## 1. Tables (44 total) — full column schema

> Format: `column_name — type — nullable — default`

### profiles
- id — uuid — NOT NULL
- name — text — nullable — `''`
- email — text — nullable — `''`
- gender — text — nullable
- age — integer — nullable
- weight_kg — numeric — nullable
- height_cm — numeric — nullable
- fitness_goal — text — nullable
- avatar_url — text — nullable
- created_at — timestamptz — nullable — `now()`
- updated_at — timestamptz — nullable — `now()`
- role — `user_role` (enum) — NOT NULL — `'client'`
- full_name — text — nullable

### onboarding
- id, user_id (uuid), age, gender, height_cm, weight_kg, goal, activity_level, target_weight, weekly_workouts (int), completed (bool, default false), created_at, updated_at

### user_goals
- id, user_id, daily_calories (int, default 2000), daily_protein_g (default 150), daily_carbs_g (default 250), daily_fat_g (default 65), daily_water_ml (default 2500), daily_steps (default 10000), daily_sleep_hours (numeric, default 8), weekly_workouts (default 4), target_weight_kg, created_at, updated_at

### nutrition_logs
- id, user_id, food_id, food_name (NOT NULL), meal_type, quantity (NOT NULL, default 1), serving_unit (default 'g'), calories (NOT NULL), protein_g/carbs_g/fat_g (default 0), logged_date (date, default CURRENT_DATE), logged_at (timestamptz, default now())

### foods
- id, name (NOT NULL), name_ar, calories (NOT NULL), protein_g/carbs_g/fat_g/fiber_g (default 0), serving_size (default 100), serving_unit (default 'g'), is_custom (bool, default false), created_by (uuid), created_at, category (default 'other'), image_url

### daily_summary
- id, user_id, summary_date (date, default CURRENT_DATE), steps, active_minutes, calories_burned (int), water_ml, sleep_hours, calories_consumed (numeric), protein_g/carbs_g/fat_g, workout_done (bool), workout_duration (int), mood (int), notes, created_at, updated_at

### body_measurements
- id, user_id, weight_kg, body_fat_pct, muscle_mass, chest_cm, waist_cm, hips_cm, arms_cm, thighs_cm, measured_date (date, default CURRENT_DATE), notes, created_at

### daily_activity
- id, user_id (NOT NULL), activity_date (date, default CURRENT_DATE), steps (int, default 0), active_calories_burned (numeric, default 0), heart_rate_avg, exercise_minutes, source (default 'health_connect'), synced_at, created_at

### exercises
- id, name (NOT NULL), name_ar, muscle_group, secondary_muscles (text[]), equipment, category, instructions, instructions_ar, tips, created_at, image_url, youtube_video_id, gif_url

### workout_sessions
- id, user_id, muscle_group (NOT NULL), session_name, duration_min (default 0), notes, session_date (date, default CURRENT_DATE), started_at, ended_at

### workout_sets
- id, session_id, user_id, exercise_name (NOT NULL), set_number (NOT NULL), reps, weight_kg, duration_sec, rest_sec (default 60), is_warmup (bool, default false), logged_at

### exercise_progress
- id, user_id, exercise_id, session_date (default CURRENT_DATE), best_set_weight, best_set_reps, total_volume, one_rm_estimate, session_id, created_at

### training_programs
- id, name (NOT NULL), name_ar, description, description_ar, level, goal, days_per_week, duration_weeks, split_type, is_active (bool, default true), created_at

### program_days
- id, program_id, day_number (NOT NULL), name (NOT NULL), name_ar, muscle_groups (text[]), notes

### program_day_exercises
- id, program_day_id, exercise_id, order_index (NOT NULL), sets, reps_min, reps_max, rest_seconds (default 90), notes, notes_ar, is_main_lift (bool, default false)

### user_active_program
- id, user_id, program_id, started_at (default now()), current_week (default 1), current_day (default 1), notes, updated_at

### user_programs
- id, user_id, program_name (NOT NULL), muscle_group (NOT NULL), is_active (default true), started_at

### streak_activity_log
- id, user_id, activity_date (date, NOT NULL), source (text, NOT NULL — 'workout'|'nutrition'), created_at

### user_streaks
- user_id (PK), current_streak (default 0), longest_streak (default 0), last_active_date (date), freeze_available (default 1), updated_at

### barcode_products
- barcode (PK, text, NOT NULL), product_name (NOT NULL), product_name_ar, brand, serving_size_g, calories/protein_g/carbs_g/fat_g (NOT NULL, default 0), source (NOT NULL — 'openfoodfacts'|'gemini_estimate'), confidence (default 'high'), lookup_count (default 1), created_at, updated_at

### barcode_scan_history
- id, user_id, barcode, quantity_g, nutrition_log_id, scanned_at

### food_scans
- id, user_id, image_path, is_food (bool, default true), confidence (default 'medium'), notes, scanned_at, created_at

### food_scan_items
- id, scan_id (NOT NULL), name (NOT NULL), name_ar, estimated_weight_g, calories, protein_g, carbs_g, fat_g (all NOT NULL, default 0), nutrition_log_id, created_at

### voice_food_logs
- id, user_id, audio_path, transcript, is_food (default true), confidence (default 'medium'), notes, logged_at, created_at

### voice_food_log_items
- id, log_id (NOT NULL), name (NOT NULL), name_ar, estimated_weight_g/calories/protein_g/carbs_g/fat_g (NOT NULL, default 0), nutrition_log_id, created_at

### coaches
- id, user_id (NOT NULL), bio (default ''), price_monthly (default 0), specialization (text[]), rating (default 0), is_active (default true), stripe_account_id, created_at, updated_at

### coach_profiles
- id (PK, = auth user id), bio, bio_ar, specialties (text[]), certifications (text[]), experience_years (default 0), price_per_month (default 0), currency (default 'EGP'), rating (default 0), reviews_count (default 0), is_available (default true), is_verified (default false), cover_image_url, instagram_url, youtube_url, max_clients (default 20), current_clients (default 0), created_at, updated_at

### coach_onboarding
- id, user_id (NOT NULL), display_name, years_experience (int, default 0), certifications (text[]), specialization (text[]), bio, price_monthly, price_premium, languages (text[], default {Arabic,English}), max_clients (default 10), profile_image_url, intro_video_url, is_completed (bool, default false), created_at, updated_at, phone_number, city, gender, gallery_images (text[]), pdf_urls (text[]), certificate_files (text[]), transformation_images (text[])

### coach_content
- id, coach_id (NOT NULL), title (NOT NULL), description, type (NOT NULL), file_url (NOT NULL), is_public (bool, default false), created_at, thumbnail_url, file_size_kb, sort_order (default 0)

### client_assignments
- id, coach_id (NOT NULL), client_id (NOT NULL), content_id (NOT NULL), note, assigned_at

### coach_reviews
- id, coach_id, client_id, rating (int), comment, created_at

### reviews
- id, client_id (NOT NULL), coach_id (NOT NULL), rating (int, NOT NULL), comment, created_at

### coach_subscriptions
- id, client_id, coach_id, status (default 'pending'), started_at, expires_at, price (default 0), currency (default 'EGP'), notes, created_at, updated_at

### subscriptions
- id, client_id (NOT NULL), coach_id (NOT NULL), status (`subscription_status` enum, NOT NULL, default 'active'), tier (default 'basic'), start_date (date, default CURRENT_DATE), end_date, stripe_sub_id, created_at, updated_at, plan_id, payment_status, started_at, expires_at, goals, notes

### subscription_plans
- id, coach_id, name (NOT NULL), price_usd, duration_days, max_clients, created_at

### subscription_phases
- id, subscription_id (NOT NULL), phase_number (NOT NULL), title (NOT NULL), type, description, duration_weeks, status (default 'upcoming'), started_at, completed_at, created_at

### payment_intents
- id, client_id (NOT NULL), coach_id (NOT NULL), stripe_payment_id (NOT NULL), stripe_customer_id, amount (NOT NULL), currency (default 'usd'), status (default 'pending'), tier (default 'standard'), created_at

### stripe_customers
- id, user_id (NOT NULL), stripe_customer_id (NOT NULL), created_at

### conversations
- id, client_id, coach_id, subscription_id, last_message, last_message_at (default now()), client_unread (default 0), coach_unread (default 0), is_active (default true), created_at

### messages
- id, conversation_id, sender_id, content (NOT NULL), type (default 'text' — text|voice|image|file), file_url, is_read (default false), is_deleted (default false), created_at

### notifications
- id, user_id (NOT NULL), type (NOT NULL), title (NOT NULL), body (NOT NULL), conversation_id, plan_id, coach_id, is_read (default false), created_at

### notification_preferences
- user_id (PK), meal_reminders_enabled/water_reminders_enabled/calorie_alerts_enabled/chat_notifications_enabled (all NOT NULL, default true), quiet_hours_start/quiet_hours_end (time), updated_at

### notification_log
- id, user_id (NOT NULL), type (NOT NULL), title (NOT NULL), body (NOT NULL), data (jsonb), sent_at (default now()), read_at

### weekly_activity
- id, user_id, week_start (date, NOT NULL), day_index (int, NOT NULL), actual_pct, goal_pct (default 0)

**Views (read-only, computed from the tables above — no separate RLS needed):** `personal_records`, `weekly_progress`, `weight_progress`.

---

## 2. Row Level Security — status & gaps

**RLS is enabled (`relrowsecurity = true`) on all 44 tables.** No table is fully unprotected. However, several tables are missing policies for specific operations — meaning that operation is effectively **denied to everyone** (fails silently) rather than "insecure," but this must be replicated correctly in .NET or those features will silently stop working:

| Table | Has policies for | Missing |
|---|---|---|
| `coach_reviews` | INSERT, UPDATE (client) | **no SELECT policy** (public read gap — likely intended to be public, check with app behavior) |
| `notifications` | SELECT (own) | INSERT, UPDATE, DELETE — all writes happen via `SECURITY DEFINER` functions (`mark_notification_read`, `mark_all_notifications_read`) and the `notify_new_message` trigger, not direct client writes |
| `notification_log` | SELECT (own), UPDATE (own) | INSERT — writes only ever happen server-side (service role in Edge Functions), by design |
| `notification_preferences` | SELECT/INSERT/UPDATE (own) | DELETE — not needed, preferences are never deleted, only updated |
| `daily_activity` | SELECT/INSERT/UPDATE (own) | DELETE — not exposed by design |
| `streak_activity_log` | SELECT (own) | INSERT/UPDATE/DELETE — all writes go through the `record_daily_activity()` SECURITY DEFINER function, never direct table access |
| `user_streaks` | SELECT (own) | INSERT/UPDATE — same as above, only written via `record_daily_activity()` |
| `payment_intents` | SELECT (own client, or coach on their own) | INSERT/UPDATE — presumably written by a Stripe webhook handler with the service role (not found among the 7 listed Edge Functions — **verify this exists or is planned**) |
| `barcode_products` | SELECT (authenticated) | INSERT/UPDATE — writes only via `lookup-barcode` Edge Function (service role) |
| `stripe_customers` | SELECT (own) | INSERT/UPDATE — presumably service-role-only from a Stripe flow |

**Full RLS policy list (all 44 tables), exact `qual`/`with_check` SQL:**

```sql
-- barcode_products
CREATE POLICY barcode_products_read_authenticated ON barcode_products FOR SELECT
  USING (auth.role() = 'authenticated');

-- barcode_scan_history
CREATE POLICY barcode_history_select_own ON barcode_scan_history FOR SELECT USING (user_id = auth.uid());
CREATE POLICY barcode_history_insert_own ON barcode_scan_history FOR INSERT WITH CHECK (user_id = auth.uid());
CREATE POLICY barcode_history_update_own ON barcode_scan_history FOR UPDATE USING (user_id = auth.uid()) WITH CHECK (user_id = auth.uid());
CREATE POLICY barcode_history_delete_own ON barcode_scan_history FOR DELETE USING (user_id = auth.uid());

-- body_measurements
CREATE POLICY measurements_select ON body_measurements FOR SELECT USING (auth.uid() = user_id);
CREATE POLICY measurements_insert ON body_measurements FOR INSERT WITH CHECK (auth.uid() = user_id);
CREATE POLICY measurements_update ON body_measurements FOR UPDATE USING (auth.uid() = user_id) WITH CHECK (auth.uid() = user_id);
CREATE POLICY measurements_delete ON body_measurements FOR DELETE USING (auth.uid() = user_id);
CREATE POLICY coach_reads_client_measurements ON body_measurements FOR SELECT
  TO authenticated USING (user_id = auth.uid() OR is_my_active_client(user_id));

-- client_assignments
CREATE POLICY assignments_client_read ON client_assignments FOR SELECT TO authenticated USING (client_id = auth.uid());
CREATE POLICY assignments_coach_manage ON client_assignments FOR ALL TO authenticated
  USING (coach_id IN (SELECT id FROM coaches WHERE user_id = auth.uid()))
  WITH CHECK (coach_id IN (SELECT id FROM coaches WHERE user_id = auth.uid()));

-- coach_content
CREATE POLICY coach_content_manage_own ON coach_content FOR ALL TO authenticated
  USING (coach_id IN (SELECT id FROM coaches WHERE user_id = auth.uid()))
  WITH CHECK (coach_id IN (SELECT id FROM coaches WHERE user_id = auth.uid()));
CREATE POLICY coach_content_subscriber_read ON coach_content FOR SELECT TO authenticated
  USING (is_public = true OR id IN (SELECT content_id FROM client_assignments WHERE client_id = auth.uid()));

-- coach_onboarding
CREATE POLICY coach_onboarding_select ON coach_onboarding FOR SELECT TO authenticated USING (user_id = auth.uid());
CREATE POLICY coach_onboarding_public_read ON coach_onboarding FOR SELECT TO authenticated USING (is_completed = true);
CREATE POLICY coach_onboarding_insert ON coach_onboarding FOR INSERT TO authenticated WITH CHECK (user_id = auth.uid());
CREATE POLICY coach_onboarding_update ON coach_onboarding FOR UPDATE TO authenticated USING (user_id = auth.uid()) WITH CHECK (user_id = auth.uid());
CREATE POLICY coach_onboarding_delete ON coach_onboarding FOR DELETE TO authenticated USING (user_id = auth.uid());

-- coach_profiles
CREATE POLICY coach_profiles_public_read ON coach_profiles FOR SELECT USING (true);
CREATE POLICY coach_profiles_owner_insert ON coach_profiles FOR INSERT WITH CHECK (auth.uid() = id);
CREATE POLICY coach_profiles_owner_update ON coach_profiles FOR UPDATE USING (auth.uid() = id) WITH CHECK (auth.uid() = id);

-- coach_reviews
CREATE POLICY reviews_public_read ON coach_reviews FOR SELECT USING (true);
CREATE POLICY reviews_insert ON coach_reviews FOR INSERT WITH CHECK (auth.uid() = client_id);
CREATE POLICY reviews_update ON coach_reviews FOR UPDATE USING (auth.uid() = client_id) WITH CHECK (auth.uid() = client_id);

-- coach_subscriptions
CREATE POLICY sub_select ON coach_subscriptions FOR SELECT USING (auth.uid() = client_id OR auth.uid() = coach_id);
CREATE POLICY sub_insert ON coach_subscriptions FOR INSERT WITH CHECK (auth.uid() = client_id);
CREATE POLICY sub_update ON coach_subscriptions FOR UPDATE USING (auth.uid() = client_id OR auth.uid() = coach_id) WITH CHECK (auth.uid() = client_id OR auth.uid() = coach_id);

-- coaches
CREATE POLICY coaches_read_all ON coaches FOR SELECT USING (is_active = true);
CREATE POLICY coaches_select_own ON coaches FOR SELECT TO authenticated USING (user_id = auth.uid());
CREATE POLICY coaches_insert_own ON coaches FOR INSERT TO authenticated WITH CHECK (user_id = auth.uid());
CREATE POLICY coaches_update_own ON coaches FOR UPDATE USING (user_id = auth.uid());

-- conversations (duplicated/overlapping policies from two migrations — safe to consolidate in .NET)
CREATE POLICY conv_select ON conversations FOR SELECT USING (auth.uid() = client_id OR auth.uid() = coach_id);
CREATE POLICY participants_can_read_conversations ON conversations FOR SELECT USING (client_id = auth.uid() OR coach_id = auth.uid());
CREATE POLICY conv_insert ON conversations FOR INSERT WITH CHECK (auth.uid() = client_id OR auth.uid() = coach_id);
CREATE POLICY authenticated_can_create_conversations ON conversations FOR INSERT WITH CHECK (client_id = auth.uid() OR coach_id = auth.uid());
CREATE POLICY conv_update ON conversations FOR UPDATE USING (auth.uid() = client_id OR auth.uid() = coach_id) WITH CHECK (auth.uid() = client_id OR auth.uid() = coach_id);
CREATE POLICY participants_can_update_conversations ON conversations FOR UPDATE USING (client_id = auth.uid() OR coach_id = auth.uid());

-- daily_activity
CREATE POLICY "Users can view their own daily activity" ON daily_activity FOR SELECT USING (auth.uid() = user_id);
CREATE POLICY "Users can insert their own daily activity" ON daily_activity FOR INSERT WITH CHECK (auth.uid() = user_id);
CREATE POLICY "Users can update their own daily activity" ON daily_activity FOR UPDATE USING (auth.uid() = user_id) WITH CHECK (auth.uid() = user_id);

-- daily_summary
CREATE POLICY summary_select ON daily_summary FOR SELECT USING (auth.uid() = user_id);
CREATE POLICY summary_insert ON daily_summary FOR INSERT WITH CHECK (auth.uid() = user_id);
CREATE POLICY summary_update ON daily_summary FOR UPDATE USING (auth.uid() = user_id) WITH CHECK (auth.uid() = user_id);
CREATE POLICY summary_delete ON daily_summary FOR DELETE USING (auth.uid() = user_id);
CREATE POLICY coach_reads_client_summary ON daily_summary FOR SELECT TO authenticated USING (user_id = auth.uid() OR is_my_active_client(user_id));

-- exercise_progress
CREATE POLICY user_select_progress ON exercise_progress FOR SELECT USING (auth.uid() = user_id);
CREATE POLICY user_insert_progress ON exercise_progress FOR INSERT WITH CHECK (auth.uid() = user_id);
CREATE POLICY user_update_progress ON exercise_progress FOR UPDATE USING (auth.uid() = user_id) WITH CHECK (auth.uid() = user_id);
CREATE POLICY user_delete_progress ON exercise_progress FOR DELETE USING (auth.uid() = user_id);
CREATE POLICY coach_reads_client_progress ON exercise_progress FOR SELECT TO authenticated USING (user_id = auth.uid() OR is_my_active_client(user_id));

-- exercises / program_days / program_day_exercises / training_programs (public reference data)
CREATE POLICY public_read_exercises ON exercises FOR SELECT USING (true);
CREATE POLICY public_read_program_days ON program_days FOR SELECT USING (true);
CREATE POLICY public_read_program_day_exercises ON program_day_exercises FOR SELECT USING (true);
CREATE POLICY public_read_programs ON training_programs FOR SELECT USING (true);

-- food_scans / food_scan_items
CREATE POLICY food_scans_select_own ON food_scans FOR SELECT USING (auth.uid() = user_id);
CREATE POLICY food_scans_insert_own ON food_scans FOR INSERT WITH CHECK (auth.uid() = user_id);
CREATE POLICY food_scans_update_own ON food_scans FOR UPDATE USING (auth.uid() = user_id) WITH CHECK (auth.uid() = user_id);
CREATE POLICY food_scans_delete_own ON food_scans FOR DELETE USING (auth.uid() = user_id);
CREATE POLICY food_scan_items_select_own ON food_scan_items FOR SELECT
  USING (EXISTS (SELECT 1 FROM food_scans s WHERE s.id = food_scan_items.scan_id AND s.user_id = auth.uid()));
CREATE POLICY food_scan_items_insert_own ON food_scan_items FOR INSERT
  WITH CHECK (EXISTS (SELECT 1 FROM food_scans s WHERE s.id = food_scan_items.scan_id AND s.user_id = auth.uid()));
CREATE POLICY food_scan_items_update_own ON food_scan_items FOR UPDATE
  USING (EXISTS (SELECT 1 FROM food_scans s WHERE s.id = food_scan_items.scan_id AND s.user_id = auth.uid()));
CREATE POLICY food_scan_items_delete_own ON food_scan_items FOR DELETE
  USING (EXISTS (SELECT 1 FROM food_scans s WHERE s.id = food_scan_items.scan_id AND s.user_id = auth.uid()));

-- foods
CREATE POLICY foods_select ON foods FOR SELECT USING (true);
CREATE POLICY foods_insert ON foods FOR INSERT WITH CHECK (auth.uid() = created_by OR is_custom = false);

-- messages
CREATE POLICY msg_select ON messages FOR SELECT
  USING (EXISTS (SELECT 1 FROM conversations c WHERE c.id = messages.conversation_id AND (c.client_id = auth.uid() OR c.coach_id = auth.uid())));
CREATE POLICY conversation_participants_can_read_messages ON messages FOR SELECT
  USING (EXISTS (SELECT 1 FROM conversations c WHERE c.id = messages.conversation_id AND (c.client_id = auth.uid() OR c.coach_id = auth.uid())));
CREATE POLICY msg_insert ON messages FOR INSERT
  WITH CHECK (auth.uid() = sender_id AND EXISTS (SELECT 1 FROM conversations c WHERE c.id = messages.conversation_id AND (c.client_id = auth.uid() OR c.coach_id = auth.uid())));
CREATE POLICY sender_can_insert_messages ON messages FOR INSERT WITH CHECK (auth.uid() = sender_id);
CREATE POLICY msg_update ON messages FOR UPDATE USING (auth.uid() = sender_id);
CREATE POLICY participants_can_update_messages ON messages FOR UPDATE
  USING (EXISTS (SELECT 1 FROM conversations c WHERE c.id = messages.conversation_id AND (c.client_id = auth.uid() OR c.coach_id = auth.uid())));

-- notification_log
CREATE POLICY nlog_select_own ON notification_log FOR SELECT USING (auth.uid() = user_id);
CREATE POLICY nlog_update_own ON notification_log FOR UPDATE USING (auth.uid() = user_id);

-- notification_preferences
CREATE POLICY prefs_select_own ON notification_preferences FOR SELECT USING (auth.uid() = user_id);
CREATE POLICY prefs_insert_own ON notification_preferences FOR INSERT WITH CHECK (auth.uid() = user_id);
CREATE POLICY prefs_update_own ON notification_preferences FOR UPDATE USING (auth.uid() = user_id);

-- notifications
CREATE POLICY notifications_select_own ON notifications FOR SELECT USING (user_id = auth.uid());

-- nutrition_logs
CREATE POLICY nutrition_logs_select ON nutrition_logs FOR SELECT USING (auth.uid() = user_id);
CREATE POLICY nutrition_logs_insert ON nutrition_logs FOR INSERT WITH CHECK (auth.uid() = user_id);
CREATE POLICY nutrition_logs_update ON nutrition_logs FOR UPDATE USING (auth.uid() = user_id) WITH CHECK (auth.uid() = user_id);
CREATE POLICY nutrition_logs_delete ON nutrition_logs FOR DELETE USING (auth.uid() = user_id);
CREATE POLICY coach_reads_client_nutrition ON nutrition_logs FOR SELECT TO authenticated USING (user_id = auth.uid() OR is_my_active_client(user_id));

-- onboarding
CREATE POLICY onboarding_select ON onboarding FOR SELECT USING (auth.uid() = user_id);
CREATE POLICY onboarding_insert ON onboarding FOR INSERT WITH CHECK (auth.uid() = user_id);
CREATE POLICY onboarding_update ON onboarding FOR UPDATE USING (auth.uid() = user_id) WITH CHECK (auth.uid() = user_id);
CREATE POLICY onboarding_delete ON onboarding FOR DELETE USING (auth.uid() = user_id);

-- payment_intents
CREATE POLICY payment_intents_select_own ON payment_intents FOR SELECT TO authenticated USING (client_id = auth.uid());
CREATE POLICY payment_intents_coach_read ON payment_intents FOR SELECT TO authenticated USING (coach_id IN (SELECT id FROM coaches WHERE user_id = auth.uid()));

-- profiles
CREATE POLICY profiles_select ON profiles FOR SELECT USING (auth.uid() = id);
CREATE POLICY profiles_select_public ON profiles FOR SELECT
  USING (auth.uid() = id OR role = 'coach' OR EXISTS (SELECT 1 FROM subscriptions s JOIN coaches c ON c.id = s.coach_id WHERE s.client_id = profiles.id AND c.user_id = auth.uid()));
CREATE POLICY profiles_insert ON profiles FOR INSERT WITH CHECK (auth.uid() = id);
CREATE POLICY profiles_update ON profiles FOR UPDATE TO authenticated USING (auth.uid() = id) WITH CHECK (auth.uid() = id);

-- reviews
CREATE POLICY reviews_read_all ON reviews FOR SELECT USING (true);
CREATE POLICY reviews_client_insert ON reviews FOR INSERT WITH CHECK (client_id = auth.uid());
CREATE POLICY reviews_client_update ON reviews FOR UPDATE USING (client_id = auth.uid());

-- streak_activity_log
CREATE POLICY streak_activity_select_own ON streak_activity_log FOR SELECT USING (user_id = auth.uid());

-- stripe_customers
CREATE POLICY stripe_customers_select_own ON stripe_customers FOR SELECT TO authenticated USING (user_id = auth.uid());

-- subscription_phases
CREATE POLICY client_read_phases ON subscription_phases FOR SELECT
  USING (EXISTS (SELECT 1 FROM subscriptions s WHERE s.id = subscription_phases.subscription_id AND s.client_id = auth.uid()));
CREATE POLICY coach_manage_phases ON subscription_phases FOR ALL
  USING (EXISTS (SELECT 1 FROM subscriptions s JOIN coaches c ON c.id = s.coach_id WHERE s.id = subscription_phases.subscription_id AND c.user_id = auth.uid()))
  WITH CHECK (EXISTS (SELECT 1 FROM subscriptions s JOIN coaches c ON c.id = s.coach_id WHERE s.id = subscription_phases.subscription_id AND c.user_id = auth.uid()));

-- subscription_plans
CREATE POLICY client_read_subscribed_plans ON subscription_plans FOR SELECT
  USING (EXISTS (SELECT 1 FROM subscriptions s WHERE s.plan_id = subscription_plans.id AND s.client_id = auth.uid()));
CREATE POLICY coach_manage_own_plans ON subscription_plans FOR ALL USING (coach_id = auth.uid()) WITH CHECK (coach_id = auth.uid());

-- subscriptions
CREATE POLICY subscriptions_client_read ON subscriptions FOR SELECT USING (client_id = auth.uid());
CREATE POLICY subscriptions_client_insert ON subscriptions FOR INSERT WITH CHECK (client_id = auth.uid());
CREATE POLICY subscriptions_client_update ON subscriptions FOR UPDATE USING (client_id = auth.uid());
CREATE POLICY subscriptions_coach_read ON subscriptions FOR SELECT USING (coach_id IN (SELECT id FROM coaches WHERE user_id = auth.uid()));
CREATE POLICY subscriptions_coach_update ON subscriptions FOR UPDATE USING (coach_id IN (SELECT id FROM coaches WHERE user_id = auth.uid()));

-- user_active_program
CREATE POLICY user_select_active_program ON user_active_program FOR SELECT USING (auth.uid() = user_id);
CREATE POLICY user_insert_active_program ON user_active_program FOR INSERT WITH CHECK (auth.uid() = user_id);
CREATE POLICY user_update_active_program ON user_active_program FOR UPDATE USING (auth.uid() = user_id) WITH CHECK (auth.uid() = user_id);
CREATE POLICY user_delete_active_program ON user_active_program FOR DELETE USING (auth.uid() = user_id);

-- user_goals
CREATE POLICY goals_select ON user_goals FOR SELECT USING (auth.uid() = user_id);
CREATE POLICY goals_insert ON user_goals FOR INSERT WITH CHECK (auth.uid() = user_id);
CREATE POLICY goals_update ON user_goals FOR UPDATE USING (auth.uid() = user_id) WITH CHECK (auth.uid() = user_id);
CREATE POLICY goals_delete ON user_goals FOR DELETE USING (auth.uid() = user_id);

-- user_programs
CREATE POLICY user_programs_select ON user_programs FOR SELECT USING (auth.uid() = user_id);
CREATE POLICY user_programs_insert ON user_programs FOR INSERT WITH CHECK (auth.uid() = user_id);
CREATE POLICY user_programs_update ON user_programs FOR UPDATE USING (auth.uid() = user_id) WITH CHECK (auth.uid() = user_id);
CREATE POLICY user_programs_delete ON user_programs FOR DELETE USING (auth.uid() = user_id);

-- user_streaks
CREATE POLICY streaks_select_own ON user_streaks FOR SELECT USING (user_id = auth.uid());

-- voice_food_logs / voice_food_log_items
CREATE POLICY voice_food_logs_select_own ON voice_food_logs FOR SELECT USING (auth.uid() = user_id);
CREATE POLICY voice_food_logs_insert_own ON voice_food_logs FOR INSERT WITH CHECK (auth.uid() = user_id);
CREATE POLICY voice_food_logs_update_own ON voice_food_logs FOR UPDATE USING (auth.uid() = user_id) WITH CHECK (auth.uid() = user_id);
CREATE POLICY voice_food_logs_delete_own ON voice_food_logs FOR DELETE USING (auth.uid() = user_id);
CREATE POLICY voice_food_log_items_select_own ON voice_food_log_items FOR SELECT
  USING (EXISTS (SELECT 1 FROM voice_food_logs l WHERE l.id = voice_food_log_items.log_id AND l.user_id = auth.uid()));
CREATE POLICY voice_food_log_items_insert_own ON voice_food_log_items FOR INSERT
  WITH CHECK (EXISTS (SELECT 1 FROM voice_food_logs l WHERE l.id = voice_food_log_items.log_id AND l.user_id = auth.uid()));
CREATE POLICY voice_food_log_items_update_own ON voice_food_log_items FOR UPDATE
  USING (EXISTS (SELECT 1 FROM voice_food_logs l WHERE l.id = voice_food_log_items.log_id AND l.user_id = auth.uid()));
CREATE POLICY voice_food_log_items_delete_own ON voice_food_log_items FOR DELETE
  USING (EXISTS (SELECT 1 FROM voice_food_logs l WHERE l.id = voice_food_log_items.log_id AND l.user_id = auth.uid()));

-- weekly_activity
CREATE POLICY weekly_activity_select ON weekly_activity FOR SELECT USING (auth.uid() = user_id);
CREATE POLICY weekly_activity_insert ON weekly_activity FOR INSERT WITH CHECK (auth.uid() = user_id);
CREATE POLICY weekly_activity_update ON weekly_activity FOR UPDATE USING (auth.uid() = user_id) WITH CHECK (auth.uid() = user_id);
CREATE POLICY weekly_activity_delete ON weekly_activity FOR DELETE USING (auth.uid() = user_id);

-- workout_sessions
CREATE POLICY sessions_select ON workout_sessions FOR SELECT USING (auth.uid() = user_id);
CREATE POLICY sessions_insert ON workout_sessions FOR INSERT WITH CHECK (auth.uid() = user_id);
CREATE POLICY sessions_update ON workout_sessions FOR UPDATE USING (auth.uid() = user_id) WITH CHECK (auth.uid() = user_id);
CREATE POLICY sessions_delete ON workout_sessions FOR DELETE USING (auth.uid() = user_id);
CREATE POLICY coach_reads_client_workouts ON workout_sessions FOR SELECT TO authenticated USING (user_id = auth.uid() OR is_my_active_client(user_id));

-- workout_sets
CREATE POLICY sets_select ON workout_sets FOR SELECT USING (auth.uid() = user_id);
CREATE POLICY sets_insert ON workout_sets FOR INSERT WITH CHECK (auth.uid() = user_id);
CREATE POLICY sets_update ON workout_sets FOR UPDATE USING (auth.uid() = user_id) WITH CHECK (auth.uid() = user_id);
CREATE POLICY sets_delete ON workout_sets FOR DELETE USING (auth.uid() = user_id);
CREATE POLICY coach_reads_client_workout_sets ON workout_sets FOR SELECT TO authenticated USING (user_id = auth.uid() OR is_my_active_client(user_id));
```

**Key cross-cutting pattern to preserve in .NET authorization:** the `is_my_active_client(client_uid)` SQL function is used everywhere a **coach needs read access to a client's data**. It checks: does an active subscription exist linking the current authenticated user (as a coach) to that client? This must become a reusable authorization policy/service in .NET, not duplicated logic per endpoint.

---

## 3. Database functions & triggers (SECURITY DEFINER logic — must be reproduced exactly)

| Function | Type | Purpose |
|---|---|---|
| `handle_new_user()` | Trigger (on auth.users insert) | Auto-creates a `profiles` row on signup |
| `is_my_active_client(client_uid)` | Helper (SQL, STABLE, SECURITY DEFINER) | Returns true if the calling coach has an active subscription with this client — used across 6+ RLS policies |
| `record_daily_activity(p_source)` | RPC (called by app) | Logs today's activity (`workout`/`nutrition`), updates streak: increments on consecutive days, applies a once-a-month "freeze" that forgives a single missed day, resets to 1 otherwise |
| `get_streak_status()` | RPC (called by app) | Returns current/longest streak, whether logged today, "at risk" flag, freeze availability |
| `sync_nutrition_to_summary()` | Trigger (on nutrition_logs insert/update/delete) | Rolls up daily nutrition totals into `daily_summary` |
| `sync_workout_to_summary()` | Trigger (on workout_sessions insert/update/delete) | Rolls up workout completion/duration into `daily_summary` |
| `notify_new_message()` | Trigger (on messages insert) | Builds a type-aware preview (text/voice/image/file), inserts into `notifications`, and calls the `send-chat-push` Edge Function via `net.http_post` (using a Vault-stored cron secret) |
| `update_conversation_on_message()` | Trigger (on messages insert) | Updates `conversations.last_message`/`last_message_at` and increments the recipient's unread counter |
| `mark_conversation_read(conversation_id, user_id)` | RPC | Marks messages read + resets the caller's unread counter |
| `mark_notification_read` / `mark_all_notifications_read` | RPC | Marks notification(s) read |
| `unread_count(user_id)` | RPC | Sums unread counts across all the user's conversations |
| `handle_subscription_accepted()` | Trigger (on subscriptions update) | On status → 'active': creates/updates a conversation and increments `coach_profiles.current_clients`; on cancel/expire: decrements it |
| `update_coach_rating()` / `refresh_coach_rating()` | Trigger (on coach_reviews/reviews insert/update/delete) | Recomputes `coach_profiles.rating` and `coaches.rating` as an average |
| `update_updated_at()` / `update_coach_onboarding_updated_at()` | Triggers | Generic `updated_at = now()` stampers |

**Scheduled jobs (`pg_cron`):**
| Job | Schedule (UTC) | What it does |
|---|---|---|
| `streak-freeze-monthly-reset` | `0 0 1 * *` (1st of month) | Resets `freeze_available = 1` for anyone below it |
| `coregym-meal-reminders` | `0 6,12,18 * * *` (08:00/14:00/20:00 Cairo) | Calls `send-meal-reminders` Edge Function via `net.http_post` with a Vault-stored secret header |

---

## 4. Edge Functions (7 total) — purpose & full source

All use Deno + `@supabase/supabase-js@2`. All AI calls use **Gemini 3.6 Flash** (`gemini-3.6-flash`) via direct REST calls (no SDK).

### `analyze-food` (verify_jwt: true)
Takes a base64 image + user JWT → calls Gemini Vision with a strict JSON-schema prompt → uploads the image to the private `food-scans` bucket → inserts `food_scans` + `food_scan_items` rows → returns the structured nutrition breakdown.

### `log-food-voice` (verify_jwt: true)
Same pattern as `analyze-food` but for audio: base64 audio → Gemini's native audio understanding transcribes + extracts food items → uploads to `voice-food-logs` bucket → inserts `voice_food_logs` + `voice_food_log_items`.

### `log-food-text` (verify_jwt: true)
Stateless — no persistence. Takes free-text (Arabic/English/mixed) describing food → Gemini extracts structured items with macros → returns them directly for the client to confirm/save via normal `nutrition_logs` insert.

### `lookup-barcode` (verify_jwt: true)
3-tier lookup: (1) `barcode_products` cache table → (2) Open Food Facts public API → (3) Gemini text-only estimate as fallback. Successful lookups from tiers 2/3 are cached back into `barcode_products` via upsert.

### `send-notification` (verify_jwt: true)
Admin/test endpoint for manually firing a OneSignal push. Accepts either a signed-in user JWT (can only push to themselves) or a service-role/`x-admin-key` secret (can push to anyone — used by other server flows). Logs every push to `notification_log`.

### `send-meal-reminders` (verify_jwt: false — auth via `x-cron-secret` header)
Invoked by `pg_cron` 3x/day. For every profile: skip if in quiet hours, skip if already logged food today (Cairo timezone), skip if already reminded in this AM/PM/evening window, otherwise send a bilingual push and log it.

### `send-chat-push` (verify_jwt: false — auth via `x-cron-secret` header)
Invoked by the `notify_new_message()` DB trigger via `net.http_post`. Checks `chat_notifications_enabled` preference, sends a push with sender name + type-aware preview, deep-link data (`conversation_id`).

**Shared helper** (`_shared/onesignal.ts`, embedded in the last 3 functions): `sendOneSignalPush()` — posts to `https://onesignal.com/api/v1/notifications`, targets by OneSignal's `external_id` alias (= Supabase `user_id`, set via `OneSignal.login()` client-side), supports bilingual `headings`/`contents`.

**Full TypeScript source for all 7 functions has been retrieved from the live project and can be pasted in on request** — omitted here to keep this file manageable; ask if the .NET engineer needs the exact code to port function-by-function.

---

## 5. Storage buckets (8 total)

| Bucket | Public | Size limit | MIME restriction |
|---|---|---|---|
| `avatars` | public | none | none |
| `coach-media` | public | 50 MB | jpeg, png, webp, pdf, mp4, mov |
| `coach-pdfs` | public | 20 MB | pdf only |
| `food-scans` | private | none | none |
| `voice-food-logs` | private | none | none |
| `chat-voice-notes` | private | none | none |
| `chat-images` | private | none | none |
| `chat-files` | private | none | none |

*(The 5 private buckets have no explicit file-size/MIME limit set at the bucket level — if any validation exists it's client-side only; worth adding server-side limits in the .NET replacement.)*

---

## 6. Secrets currently in use (names only — values stay in Supabase)
- `GEMINI_API_KEY`
- `SUPABASE_URL`, `SUPABASE_SERVICE_ROLE_KEY` (platform-provided)
- `ONESIGNAL_APP_ID`, `ONESIGNAL_REST_API_KEY`
- `ADMIN_API_KEY` (server-to-server calls to `send-notification`)
- `CRON_SECRET` (Vault-stored, read via `vault.decrypted_secrets`, used as `x-cron-secret` by cron- and trigger-invoked functions)

---

## 7. Known open items to resolve during migration (flag to the engineer, not blockers)
1. **`coach_reviews` has no SELECT policy.** There appear to be two parallel review systems — `reviews`/`coaches` and `coach_reviews`/`coach_profiles` — likely from two different feature-build passes. Confirm which one the Flutter app actually reads from; pick one as canonical during the .NET rewrite rather than porting both.
2. **`conversations` and `messages` have duplicate/overlapping RLS policies** from what look like two separate migrations. Functionally redundant — safe to consolidate into one clean policy per operation in .NET.
3. **No Edge Function found for Stripe webhook handling** among the 7 listed. `payment_intents`/`stripe_customers` have SELECT-only RLS with no visible INSERT path — either a webhook handler exists outside standard Edge Functions, or this part of the payment flow isn't finished. **Confirm current Stripe integration status before assuming it needs porting.**
4. **Chat/notification system is very recent** (all related migrations dated 2026-09-04) — the newest, least battle-tested part of the schema. Allocate extra testing time here during the .NET port.
