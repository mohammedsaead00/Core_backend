# Authorization

Phase 1 replaces Postgres Row Level Security with a single reusable .NET
authorization stack. The recurring pattern in the original schema was:

```sql
-- user can see/edit only their own data
USING (auth.uid() = user_id)
-- coach can see a client's data if there's an active subscription
USING (user_id = auth.uid() OR is_my_active_client(user_id))
```

## Components

### 1. `ICurrentUserService` — the `auth.uid()` equivalent

Resolves the current user's id from JWT claims. Reads both ASP.NET's mapped
`ClaimTypes.NameIdentifier` and the raw `sub` claim, so it works regardless of
the JWT handler's claim mapping. The final IdP that replaces Supabase Auth is
still an open decision — the mapping lives in exactly one place
(`UserIdClaimReader`) and can be adjusted there.

### 2. `IClientAccessService` — the `is_my_active_client()` replacement

```csharp
Task<bool> IsActiveClientOfCoach(Guid coachUserId, Guid clientUserId, CancellationToken ct = default);
```

One no-tracking query against the canonical `subscriptions` table, joined
through `coaches`, requiring `status = 'active'`:

```csharp
_db.Subscriptions.AsNoTracking()
    .Where(s => s.Status == "active"
             && s.ClientId == clientUserId
             && s.Coach!.UserId == coachUserId)
    .AnyAsync(ct);
```

### 3. `OwnDataOrActiveCoach` — the reusable resource-based policy

`OwnDataOrActiveCoachHandler` (an `IAuthorizationHandler`) succeeds when the
principal's user id equals the resource's `UserId`, or when
`IsActiveClientOfCoach` returns true. It is registered **once** and evaluated
against any entity implementing `IOwnedResource` — never duplicated per endpoint.

```csharp
// registration (Program.cs, Phase 3)
builder.Services.AddCoreGymAuthorization();

// usage on any endpoint that loads an IOwnedResource
var measurement = await db.BodyMeasurements.FindAsync(id);
var result = await authorizationService.AuthorizeAsync(user, measurement, "OwnDataOrActiveCoach");
if (!result.Succeeded) return Forbid();
```

### 4. `IOwnedResource`

Implemented by the six tables that used `is_my_active_client` in the original
RLS: `body_measurements`, `daily_summary`, `exercise_progress`, `nutrition_logs`,
`workout_sessions`, `workout_sets`. Other user-owned tables (e.g. `onboarding`,
`user_goals`) were own-only in the original and can use a simpler `OwnData`
policy in Phase 3 if desired.

## Test coverage

Verified end-to-end through the real `IAuthorizationService` pipeline against
SQL Server (`AuthorizationServiceTests`):

- ✅ owner can access their own resource
- ✅ coach with an **active** subscription can access the client's resource
- ❌ coach with `pending` / `cancelled` / `expired` subscription — denied
- ❌ unrelated authenticated user — denied
- ❌ principal without a user-id claim — denied
- ✅ `IsActiveClientOfCoach` matches only the exact active coach↔client pair

## Where the original RLS logic lives now

| Original mechanism | .NET replacement |
|---|---|
| `auth.uid()` | `ICurrentUserService` |
| `is_my_active_client()` | `IClientAccessService.IsActiveClientOfCoach` |
| RLS policies (own + coach reads) | `OwnDataOrActiveCoach` policy |
| `SECURITY DEFINER` functions writing `notifications` / `streaks` | server-side code in Phase 2 (no direct client writes, same as the original) |
| service-role writes (Edge Functions) | server-side service accounts in Phase 2/3 |
