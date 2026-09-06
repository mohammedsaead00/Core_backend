# Business logic (Phase 2)

The original Supabase backend implemented its business rules as Postgres
`SECURITY DEFINER` functions, triggers and RPCs. Phase 2 reproduces them as
**testable .NET application services** over the `DbContext` (registered via
`ApplicationServiceSetup.AddCoreGymApplicationServices()`).

**Design decision:** business logic lives in the service layer, not in SQL
Server triggers — the same choice as the Authorization Service. It keeps the
rules testable and inspectable, and several behaviors (HTTP push, for example)
cannot live inside a database trigger at all. Database triggers remain only for
the mechanical `updated_at` stampers (see [ARCHITECTURE.md](ARCHITECTURE.md)).

## Service ↔ original function mapping

| Original (Postgres) | .NET replacement | Notes |
|---|---|---|
| `record_daily_activity(p_source)` RPC | `IStreakService.RecordDailyActivityAsync` | Appends to `streak_activity_log`, updates `user_streaks`. Idempotent per source per day. |
| `get_streak_status()` RPC | `IStreakService.GetStreakStatusAsync` | Returns current/longest streak, per-source logged-today flags, `AtRisk`, freeze availability. |
| `streak-freeze-monthly-reset` pg_cron | `IStreakService.ResetMonthlyFreezesAsync` | Call from a scheduler (IHostedService / cron) in Phase 3. |
| `sync_nutrition_to_summary()` trigger | `IDailySummaryService.SyncNutritionAsync` | Recomputes calories/macros per user+day from `nutrition_logs`; upserts `daily_summary` (creates a row only when data exists). |
| `sync_workout_to_summary()` trigger | `IDailySummaryService.SyncWorkoutAsync` | Recomputes `workout_done` / `workout_duration` from `workout_sessions`. |
| `update_conversation_on_message()` trigger | `IMessagingService.SendMessageAsync` | Updates preview + `last_message_at` and increments the recipient's unread counter. |
| `notify_new_message()` trigger | `IMessagingService.SendMessageAsync` | In-app notification for the recipient with a type-aware preview. The OneSignal push side is Phase 3. |
| `mark_conversation_read()` RPC | `IMessagingService.MarkConversationReadAsync` | Participant-only; marks the partner's messages read, resets the caller's counter. |
| `unread_count()` RPC | `IMessagingService.GetUnreadCountAsync` | Sums both roles' unread counters. |
| `mark_notification_read()` / `mark_all_notifications_read()` RPCs | `INotificationService` | Own-only writes (mirrors the original SECURITY DEFINER semantics). |
| `handle_subscription_accepted()` trigger | `ISubscriptionLifecycleService.ProcessSubscriptionStatusAsync` | Applies the status change; on →`active` creates/updates the conversation and increments `coach_profiles.current_clients`; on `active`→`cancelled`/`expired` decrements (floored at zero); repeat transitions are no-ops. |
| `update_coach_rating()` / `refresh_coach_rating()` triggers | `ICoachRatingService.RecalculateCoachRatingAsync` | Recomputes `coaches.rating` and `coach_profiles.rating` + `reviews_count` as the reviews average (0 when no reviews). |
| `handle_new_user()` trigger on `auth.users` | `IProfileProvisioningService.ProvisionAsync` | Idempotently provisions the `profiles` row; called by the future auth flow. |
| `is_my_active_client()` | `IClientAccessService` | Done in Phase 1 — see [AUTHORIZATION.md](AUTHORIZATION.md). |
| `update_updated_at()` triggers | SQL Server triggers | Kept as DB triggers (mechanical), Phase 1. |

## Usage

```csharp
builder.Services.AddCoreGymApplicationServices();   // + AddCoreGymAuthorization() from Phase 1

// e.g. the future "log food" endpoint:
await dailySummaryService.SyncNutritionAsync(userId, DateTime.UtcNow);

// the future Stripe webhook endpoint:
await subscriptionLifecycleService.ProcessSubscriptionStatusAsync(subscriptionId, statusFromStripeEvent);
```

## Inferred semantics — validate against prod

The inventory describes these behaviors in prose; the exact original SQL was
not captured. Confirm with `SELECT pg_get_functiondef(...)` on prod and adjust:

1. **Streak rules** — inferred as: consecutive day ⇒ +1; a gap of exactly one
   missed day consumes the monthly freeze (if available) and still counts as
   +1; any other gap resets to 1; `AtRisk` = streak alive but nothing logged
   today.
2. **Chat notification type** — service writes `type = 'chat_message'` into
   `notifications`; the original's exact value is unconfirmed.
3. **Type-aware previews** — `Voice message` / `Photo` / `File` for non-text
   messages; the original may use bilingual wording (the notification `title`
   here is the sender's profile name).

## Push integration (OneSignal)

`IPushNotificationService` is the HTTP side of the original `notify_new_message`
trigger. `OneSignalPushService` posts to OneSignal's REST API targeting users by
`external_id` alias (= user id), with bilingual `headings`/`contents` and deep-link
`data` (`conversationId`, `type: chat_message`); it is a **no-op when
`OneSignal:AppId` is not configured**, so local development and tests never hit
the network. `MessagingService.SendMessageAsync` fires it best-effort after the
message and in-app notification are persisted — a push failure never fails the
send. The exact wording/data of the original push is unconfirmed (see the
inferred-semantics list above).

## Deliberately out of scope (remaining Phase 3 units)

- AI endpoints (`analyze-food`, `log-food-voice`, `log-food-text`,
  `lookup-barcode`) and the meal-reminder scheduler (`send-meal-reminders`).
- File upload endpoints for the Supabase storage buckets.

(Done in earlier units: Stripe webhook HTTP receiver — signature verification +
`IStripeWebhookService`; OneSignal push; `StreakFreezeResetJob` scheduler.)
