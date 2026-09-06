# CoreGym Backend

The .NET / SQL Server backend for **CoreGym** — a fitness, nutrition and coaching app — migrated from Supabase (PostgreSQL 17).

**Status: Phase 1 + Phase 2 + API layer complete** ✅ — full database schema, authorization service, business-logic services, and an authenticated HTTP API, verified by 93 integration tests.

| | |
|---|---|
| Stack | .NET 10 · ASP.NET Core Web API · EF Core 10 (SQL Server provider) · xUnit |
| Source platform | Supabase (PostgreSQL 17, project `mkrjvrnysuvtokqkyoll`) |
| Schema coverage | **42 of 44 tables ported** + 3 views (2 legacy tables intentionally dropped) |
| Authorization | Replaces Postgres RLS with a reusable .NET policy (`OwnDataOrActiveCoach`), enforced end-to-end through the API |
| Business logic | 7 application services replacing the original SECURITY DEFINER functions/triggers/RPCs |
| API | JWT-authenticated REST endpoints (Bearer; symmetric-key or OIDC modes) — see [docs/API.md](docs/API.md) |
| Tests | 93/93 passing — schema shape, defaults, constraints, triggers, views, authorization, business logic, HTTP end-to-end |

---

## What Phase 1 delivers

- **Complete SQL Server schema** matching the Supabase backend: 42 tables with FKs, performance indexes, `updated_at` triggers, and CHECK constraints (including newly added ones for `profiles.role` and `subscriptions.status`).
- **Three read views** — `personal_records`, `weekly_progress`, `weight_progress` — plus keyless EF read models to query them.
- **Authorization service** — the exact .NET replacement for the recurring RLS pattern *"a user can see their own data; a coach can see a client's data if there's an active subscription"* (`ICurrentUserService`, `IClientAccessService`, `OwnDataOrActiveCoach` policy).
- **Business-logic services (Phase 2)** — streaks with the monthly freeze, daily-summary sync, messaging (previews, unread counters, notifications), notification read-marking, subscription lifecycle (conversation + client counting), coach rating recalculation, and profile provisioning.
- **No endpoints yet** — by design. Phases 1–2 are data, auth and service layers; controllers come next.

## Solution structure

```
CoreGym.sln
├── src/
│   ├── CoreGym.Domain/             entities, enums, authorization abstractions, view read models (no EF references)
│   ├── CoreGym.Infrastructure/     DbContext, EF configurations, migrations, authorization + business-logic services
│   └── CoreGym.Api/                ASP.NET Core REST API (JWT bearer, controllers, OpenAPI)
├── tests/
│   ├── CoreGym.Infrastructure.Tests/  integration tests against a scratch SQL Server LocalDB database
│   └── CoreGym.Api.Tests/             end-to-end HTTP tests via WebApplicationFactory + test JWTs
├── SQL/                            generated idempotent SQL scripts (one per migration) + Supabase backport
├── docs/                           architecture, schema, authorization, business logic, API, review checklist
└── MIGRATION_PROGRESS.md           decision log & resume point (kept current after every unit of work)
```

## Getting started

**Prerequisites:** .NET 10 SDK, SQL Server (any edition; tests use LocalDB `(localdb)\MSSQLLocalDB`), `dotnet-ef` tool (`dotnet tool install --global dotnet-ef`).

```bash
# build + run the full integration test suite (creates & drops a scratch database automatically)
dotnet test CoreGym.sln

# apply migrations to your own SQL Server database
dotnet ef database update --project src/CoreGym.Infrastructure --connection "<your connection string>"

# or produce the full idempotent SQL script
dotnet ef migrations script --idempotent --project src/CoreGym.Infrastructure
```

Per-unit review scripts live in [`SQL/`](SQL/) (`001`–`005` + a Supabase backport for the `reviews` table).

## Documentation

| Document | Contents |
|---|---|
| [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) | Solution layout, type-mapping conventions, trigger handling, testing approach |
| [docs/SCHEMA.md](docs/SCHEMA.md) | Full table inventory by group, views, constraints, index strategy |
| [docs/AUTHORIZATION.md](docs/AUTHORIZATION.md) | The RLS → .NET authorization mapping and how to use the policy |
| [docs/BUSINESS_LOGIC.md](docs/BUSINESS_LOGIC.md) | Phase 2 services ↔ original SQL functions, inferred semantics, Phase 3 boundaries |
| [docs/API.md](docs/API.md) | HTTP API reference: auth modes, conventions, endpoint tables |
| [docs/REVIEW_CHECKLIST.md](docs/REVIEW_CHECKLIST.md) | Open items to confirm before production cutover |
| [MIGRATION_PROGRESS.md](MIGRATION_PROGRESS.md) | Full decision log, session-by-session |
| [docs/source-documents/](docs/source-documents/) | Original Supabase inventory + the Phase 1 migration brief |

## Key decisions (summary)

- `text[]` columns → `NVARCHAR(MAX)` JSON with `ISJSON` check constraints (identical API payload shape).
- `timestamptz` → `DATETIMEOFFSET` with UTC defaults (`SYSDATETIMEOFFSET()`).
- `reviews` (not `coach_reviews`) and `subscriptions` (not `coach_subscriptions`) are the canonical tables — the legacy duplicates are **not** ported.
- Stripe tables (`payment_intents`, `stripe_customers`) are ported now; the payment flow is live in the app.
- `coach_id` key spaces are intentional and locked by tests: `subscriptions.coach_id` → `coaches.id`, while `subscription_plans.coach_id` and `conversations.coach_id` → user id.

## Roadmap

- ~~**Phase 1** — database schema + authorization service~~ ✅
- ~~**Phase 2** — business-logic services (streaks, summary sync, messaging, notifications, subscription lifecycle, coach ratings, profile provisioning)~~ ✅
- ~~**Phase 3, unit 1** — REST API (JWT auth incl. OIDC mode, endpoints for all core flows, policy-guarded coach views)~~ ✅
- **Phase 3, remaining** — Stripe webhook HTTP receiver, OneSignal push, AI endpoints (food analysis, barcode lookup, meal reminders), scheduler for the freeze reset, file storage for the Supabase buckets, auth-provider decision (the API already accepts either symmetric or OIDC tokens).
