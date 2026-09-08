# API reference (Phase 3, unit 1)

ASP.NET Core host (`src/CoreGym.Api`) exposing the Phase 1 schema and Phase 2
services over authenticated HTTP. OpenAPI document at `/openapi/v1.json` in
development.

## Authentication

All endpoints require a Bearer JWT except the public reference-data ones
(foods, exercises, programs, coach directory). The user id always comes from
the token's `sub`/`NameIdentifier` claim — never from the client.

Two interchangeable JWT configuration modes (see `Jwt` section in
`appsettings.json`) — whichever the final IdP decision lands on:

| Mode | Config | Use |
|---|---|---|
| Symmetric | `Jwt:SigningKey` (+ optional `Jwt:Issuer`, `Jwt:Audience`) | Local dev / tests, or any HS256 issuer |
| OIDC | `Jwt:Authority` (+ optional `Jwt:Audience`) | Supabase Auth, Auth0, Keycloak, … |

## Conventions

- JSON: camelCase; enums serialize as camelCase strings (`"client"`).
- Errors: RFC 7807 problem details — `KeyNotFoundException` → 404,
  `UnauthorizedAccessException` → 403, `ArgumentException` → 400.
- List endpoints accept `page`/`limit`-style paging, clamped server-side.
- The user id is resolved from the JWT in every endpoint; a user can only
  address their own data except through the coach endpoints below, which are
  guarded by the reusable `OwnDataOrActiveCoach` policy (403 when the caller
  is neither the owner nor a coach with an active subscription).

## Endpoints

### Identity & profile
| Method | Route | Notes |
|---|---|---|
| POST | `/api/me` | Provision the profile for the token's user (idempotent; `handle_new_user` replacement) |
| GET | `/api/me` | Own profile |
| PUT | `/api/me` | Update own profile fields |
| GET/PUT | `/api/me/onboarding` | Own onboarding (PUT upserts) |
| GET/PUT | `/api/me/goals` | Own daily targets (PUT upserts) |

### Daily tracking
| Method | Route | Notes |
|---|---|---|
| GET | `/api/me/summary/{date}` · `/api/me/summary?from&to` | Daily summaries |
| POST/DELETE | `/api/nutrition/logs` · GET `/api/nutrition/logs?date` | Writes re-sync the summary (`sync_nutrition_to_summary` replacement) |
| GET/POST | `/api/me/measurements` | Body measurements |
| POST/DELETE | `/api/workouts/sessions` · GET `?from&to` | Session incl. sets; writes re-sync the summary |
| GET | `/api/workouts/sets?sessionId` | Own sets of a session |
| GET | `/api/me/streak` | `get_streak_status` replacement |
| POST | `/api/me/streak/activity` | `{ "source": "workout" \| "nutrition" }` — `record_daily_activity` replacement |

### Reference data (anonymous)
| Method | Route | Notes |
|---|---|---|
| GET | `/api/foods?search&category&page&pageSize` · `/api/foods/{id}` | Food catalog |
| GET | `/api/exercises?search&muscleGroup&page&pageSize` · `/api/exercises/{id}` | Exercise catalog |
| GET | `/api/programs` · `/api/programs/{id}` | Program catalog incl. days + exercises |

### Programs
| Method | Route | Notes |
|---|---|---|
| GET/PUT | `/api/me/active-program` | Own active-program pointer (PUT upserts) |

### Chat
| Method | Route | Notes |
|---|---|---|
| GET | `/api/chat/conversations` | Own conversations (either role), newest first |
| GET | `/api/chat/conversations/{id}/messages` | Participant only |
| POST | `/api/chat/conversations/{id}/messages` | `{ content, type?, fileUrl? }` — updates preview/unread + in-app notification |
| POST | `/api/chat/conversations/{id}/read` | `mark_conversation_read` replacement |
| GET | `/api/chat/unread` | `unread_count` replacement |

### Notifications
| Method | Route | Notes |
|---|---|---|
| GET | `/api/notifications` | Own notifications |
| POST | `/api/notifications/{id}/read` | Own-only |
| POST | `/api/notifications/read-all` | Own-only |
| GET/PUT | `/api/notifications/preferences` | Quiet hours + toggles (PUT upserts) |

### Coaches & subscriptions
| Method | Route | Notes |
|---|---|---|
| GET | `/api/coaches` | Public directory (anonymous), rating order |
| GET | `/api/coaches/{id}` | Public detail |
| GET | `/api/coaches/{id}/reviews` | Public reviews (canonical `reviews` table) |
| POST | `/api/coaches/{id}/reviews` | Authenticated client; rating 1–5; recalculates coach ratings |
| GET | `/api/me/subscriptions` | Own subscriptions as a client |
| GET | `/api/coach/subscriptions` | Subscriptions of the calling coach |
| GET | `/api/coach/clients/{clientUserId}/summary/{date}` | Policy-guarded |
| GET | `/api/coach/clients/{clientUserId}/nutrition/{date}` | Policy-guarded |
| GET | `/api/coach/clients/{clientUserId}/measurements` | Policy-guarded |
| GET | `/api/coach/clients/{clientUserId}/workouts?from&to` | Policy-guarded |

### AI & files
| Method | Route | Notes |
|---|---|---|
| POST | `/api/ai/food/image` | `{ imageBase64, mimeType?, notes? }` — Gemini vision → persists `food_scans` + items (media stored in `food-scans` bucket) |
| POST | `/api/ai/food/voice` | `{ audioBase64, mimeType?, notes? }` — Gemini audio → persists `voice_food_logs` (with transcript) + items |
| POST | `/api/ai/food/text` | `{ text }` — stateless extraction; the client confirms and logs via `POST /api/nutrition/logs` |
| GET | `/api/ai/barcode/{barcode}` | 3-tier lookup: cache (increments `lookup_count`) → Open Food Facts → Gemini estimate; successful fills are cached |
| GET | `/api/files/{bucket}/{**path}` | Public buckets (`avatars`, `coach-media`, `coach-pdfs`) anonymous; private buckets require a token |
| POST | `/api/files/{bucket}` | Authenticated upload (multipart form `file`); returns `{ path }` |

AI endpoints answer **503** when `Gemini:ApiKey` is not configured. The Gemini
model defaults to the one the original project used (`Gemini:Model`), and the
food-analysis prompt asks for the same strict JSON contract the original Edge
Functions used (items with `name`, `name_ar`, `estimated_weight_g`, calories
and macros, plus `is_food`/`confidence`).

### Insights (read views)
| Method | Route | Notes |
|---|---|---|
| GET | `/api/me/personal-records?exerciseName&limit` | Per-exercise bests from the `personal_records` view |
| GET | `/api/me/weekly-progress?limit` | Weekly goal-completion from the `weekly_progress` view |
| GET | `/api/me/weight-progress?limit` | Weight history + change from the `weight_progress` view |
| GET | `/api/coach/clients/{clientUserId}/personal-records` | Policy-guarded coach view |

### Daily & weekly activity, progress, programs, barcode history
| Method | Route | Notes |
|---|---|---|
| GET/POST/PUT | `/api/me/daily-activity` (+ `/{id}`) | Health Connect ingest; create + update own (no delete, per prod RLS) |
| GET/PUT | `/api/me/weekly-activity` | PUT upserts per (weekStart, dayIndex) for the charts |
| GET/POST | `/api/me/exercise-progress?sessionId&exerciseId&date` | Per-session bests |
| GET/POST/PUT/DELETE | `/api/me/user-programs` (+ `/{id}`) | Full own CRUD; muscle group CHECK-validated |
| GET/POST | `/api/me/barcode-scans` | Scan history (barcode nullable) |

### Coach dashboard, content & subscription management
| Method | Route | Notes |
|---|---|---|
| GET | `/api/coach/clients` | Active subscriptions ⋈ client profiles (dashboard list) |
| GET/POST/PUT/DELETE | `/api/coach/content` (+ `/{id}`) | Own content library; DELETE returns 409 when assigned |
| GET | `/api/coaches/{coachId}/content` | Client-readable: public items + items assigned to the caller |
| POST | `/api/coach/assignments` | Assign content to a client (content must be the coach's) |
| GET | `/api/coach/assignments` · `/api/me/assignments` | Both sides' assignment lists incl. content info |
| GET/POST/PUT/DELETE | `/api/coach/subscriptions/{id}/phases` (+ `/{phaseId}`) | Coach manages phases of own subscriptions |
| GET | `/api/me/subscriptions/{id}/phases` | Client reads own subscription's phases |
| GET/PUT | `/api/coach/onboarding` | Own coach-onboarding row (PUT upserts) |
| POST | `/api/coach/subscriptions` | Coach creates a subscription (pending; tier basic/standard/premium) |
| PATCH | `/api/coach/subscriptions/{id}/status` | Lifecycle status change — evicts the authorization cache so access changes take effect immediately |

`IClientAccessService` results are cached for 60 s and evicted by the
subscription lifecycle service on every status transition.

### Webhooks
| Method | Route | Notes |
|---|---|---|
| POST | `/api/webhooks/stripe` | Signature-verified (Stripe `t`/`v1` HMAC scheme); unauthenticated by design. Handles `checkout.session.completed` (activates the subscription via `ISubscriptionLifecycleService`, records `payment_intents`, maps `stripe_customers`, stores `stripe_sub_id`) and `customer.subscription.updated`/`deleted` (status transitions). Idempotent on Stripe's retries. Requires `Stripe:WebhookSecret`; 503 when unconfigured. |

## Running

```bash
dotnet run --project src/CoreGym.Api
# required settings (env vars or appsettings):
#   ConnectionStrings__CoreGym  = "Server=...;Database=CoreGym;..."
#   Jwt__SigningKey             = "<≥32-char secret>"   (or Jwt__Authority for OIDC)
# optional:
#   Database__MigrateOnStartup  = true    # applies EF migrations on boot
#   Stripe__WebhookSecret       = "whsec_..."       # webhook endpoint returns 503 without it
#   OneSignal__AppId / OneSignal__RestApiKey    # push is a no-op without them
#   Gemini__ApiKey / Gemini__Model              # AI endpoints return 503 without a key
#   Storage__LocalRoot          = "file-storage" # local file storage root
```

Background jobs: `StreakFreezeResetJob` (hosted service) replaces the
`streak-freeze-monthly-reset` pg_cron job — it probes hourly and resets
`freeze_available` on the 1st of each month (UTC). `MealReminderJob` replaces
`coregym-meal-reminders` — it probes every 10 minutes and runs the reminder
pass at 06:00/12:00/18:00 UTC (08:00/14:00/20:00 Cairo), skipping users who
disabled reminders, are in quiet hours, already logged food today (Cairo), or
were already reminded in the current window.

## Not exposed yet

- File storage behind cloud providers (Azure Blob/S3) — `IFileStorage` is the
  seam; `LocalFileStorage` ships as the default implementation.
- Per-owner ACL on private bucket downloads (currently: any authenticated user).
