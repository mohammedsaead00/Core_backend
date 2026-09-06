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

## Running

```bash
dotnet run --project src/CoreGym.Api
# required settings (env vars or appsettings):
#   ConnectionStrings__CoreGym  = "Server=...;Database=CoreGym;..."
#   Jwt__SigningKey             = "<≥32-char secret>"   (or Jwt__Authority for OIDC)
# optional:
#   Database__MigrateOnStartup  = true   # applies EF migrations on boot
```

## Not exposed yet (later Phase 3 units)

- Stripe webhook HTTP receiver (signature verification) — the DB side exists via
  `ISubscriptionLifecycleService`.
- AI endpoints replacing the Edge Functions (`analyze-food`, `log-food-voice`,
  `log-food-text`, `lookup-barcode`).
- OneSignal push dispatch and the meal-reminder scheduler.
- File upload endpoints for the Supabase storage buckets.
