# Running the project locally — terminal runbook

Copy-paste steps to run the API, see and try every endpoint in your browser,
and inspect the SQL Server database. All commands are PowerShell (run from the
repo root `F:\core_project` unless noted).

---

## Step 0 — Prerequisites check

```powershell
dotnet --version                     # expect 10.x
sqllocaldb info MSSQLLocalDB         # expect an instance named MSSQLLocalDB
dotnet ef --version                  # expect 10.x (dotnet tool install --global dotnet-ef)
```

If LocalDB is missing: `sqllocaldb create MSSQLLocalDB` then `sqllocaldb start MSSQLLocalDB`.

## Step 1 — The one-command sanity check (optional but recommended)

```powershell
powershell -ExecutionPolicy Bypass -File verify.ps1
```

Builds everything, runs all 140 tests, boots the real API on a scratch
database, walks the main user flows over HTTP, prints a PASS/FAIL checklist
and cleans up after itself. If this is green, everything below will work.

## Step 2 — Create the dev database

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "IF DB_ID('CoreGym_Dev') IS NULL CREATE DATABASE [CoreGym_Dev]"
```

(Delete it any time with `DROP DATABASE [CoreGym_Dev]` — the API recreates the
schema on boot via migrations.)

## Step 3 — Run the API

```powershell
$env:ConnectionStrings__CoreGym = "Server=(localdb)\MSSQLLocalDB;Database=CoreGym_Dev;Trusted_Connection=True;TrustServerCertificate=True"
$env:Jwt__SigningKey            = "dev-signing-key-0123456789abcdef0123456789abcdef"
$env:Jwt__Issuer                = "coregym-dev"
$env:Jwt__Audience              = "coregym-api"
$env:Database__MigrateOnStartup = "true"
$env:ASPNETCORE_ENVIRONMENT     = "Development"
$env:ASPNETCORE_URLS            = "http://localhost:5080"

dotnet run --project src/CoreGym.Api
```

Watch the log — EF applies all 6 migrations on boot, then `Now listening on:
http://localhost:5080`. Leave this terminal running (Ctrl+C stops the API).

> The signing key/issuer/audience above must match the token helper in Step 5.

## Step 4 — See all endpoints

| Where | What |
|---|---|
| **http://localhost:5080/swagger** | Swagger UI — every endpoint, with try-it-out (click Authorize, paste a token from Step 5) |
| http://localhost:5080/openapi/v1.json | Raw OpenAPI document |
| [docs/API.md](API.md) | The full documented endpoint tables with request shapes |

## Step 5 — Try endpoints from the terminal

Open a **second** PowerShell window (leave the API running in the first).

Mint a dev token (any GUID works — the API provisions profiles on demand):

```powershell
$userId = [guid]::NewGuid().ToString()
$token  = powershell -File scripts/dev-token.ps1 -UserId $userId
```

Then walk the happy path with `curl.exe`:

```powershell
# 1. Sign up (provisions the profile for the token's user)
curl.exe -s -X POST http://localhost:5080/api/me -H "Authorization: Bearer $token" -H "Content-Type: application/json" -d '{\"email\":\"me@example.com\",\"name\":\"Me\"}'

# 2. Complete onboarding (CHECK-constrained values!)
curl.exe -s -X PUT http://localhost:5080/api/me/onboarding -H "Authorization: Bearer $token" -H "Content-Type: application/json" -d '{\"age\":28,\"gender\":\"male\",\"goal\":\"muscle_gain\",\"activityLevel\":\"moderately_active\",\"completed\":true}'

# 3. Log food — the daily summary auto-syncs
$today = (Get-Date).ToUniversalTime().ToString("yyyy-MM-dd")
curl.exe -s -X POST http://localhost:5080/api/nutrition/logs -H "Authorization: Bearer $token" -H "Content-Type: application/json" -d \"{\\\"foodName\\\":\\\"Oats\\\",\\\"calories\\\":300,\\\"proteinG\\\":20,\\\"carbsG\\\":40,\\\"fatG\\\":8,\\\"date\\\":\\\"$today\\\"}\"

# 4. See the synced summary (caloriesConsumed = 300)
curl.exe -s http://localhost:5080/api/me/summary/$today -H "Authorization: Bearer $token"

# 5. Feed the streak
curl.exe -s -X POST http://localhost:5080/api/me/streak/activity -H "Authorization: Bearer $token" -H "Content-Type: application/json" -d '{\"source\":\"workout\"}'

# 6. Log a workout with sets
curl.exe -s -X POST http://localhost:5080/api/workouts/sessions -H "Authorization: Bearer $token" -H "Content-Type: application/json" -d \"{\\\"muscleGroup\\\":\\\"chest\\\",\\\"sessionName\\\":\\\"Push\\\",\\\"durationMin\\\":45,\\\"date\\\":\\\"$today\\\",\\\"sets\\\":[{\\\"exerciseName\\\":\\\"Bench Press\\\",\\\"setNumber\\\":1,\\\"reps\\\":8,\\\"weightKg\\\":80}]}\"

# 7. Read views: personal records / weekly progress / weight progress
curl.exe -s http://localhost:5080/api/me/personal-records -H "Authorization: Bearer $token"
curl.exe -s http://localhost:5080/api/me/weight-progress -H "Authorization: Bearer $token"

# 8. Anonymous public data (no token needed)
curl.exe -s http://localhost:5080/api/foods
curl.exe -s http://localhost:5080/api/coaches
```

Prefer readable JSON in PowerShell? Swap `curl.exe -s ...` for
`Invoke-RestMethod -Uri ... -Headers @{Authorization="Bearer $token"}`.

> Easier still: open http://localhost:5080/swagger, click **Authorize**,
> paste the token, and click **Try it out** on any endpoint.

## Step 6 — See the database

```powershell
# All tables (expect 42 + the migrations history table)
sqlcmd -S "(localdb)\MSSQLLocalDB" -d CoreGym_Dev -Q "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' ORDER BY TABLE_NAME"

# Row counts of the interesting ones
sqlcmd -S "(localdb)\MSSQLLocalDB" -d CoreGym_Dev -Q "SELECT 'profiles' t, COUNT(*) c FROM profiles UNION ALL SELECT 'nutrition_logs', COUNT(*) FROM nutrition_logs UNION ALL SELECT 'daily_summary', COUNT(*) FROM daily_summary UNION ALL SELECT 'user_streaks', COUNT(*) FROM user_streaks UNION ALL SELECT 'workout_sets', COUNT(*) FROM workout_sets"

# Sample data (from the flow you just ran)
sqlcmd -S "(localdb)\MSSQLLocalDB" -d CoreGym_Dev -Q "SELECT id, email, name, role, created_at FROM profiles"
sqlcmd -S "(localdb)\MSSQLLocalDB" -d CoreGym_Dev -Q "SELECT summary_date, calories_consumed, protein_g, workout_done, workout_duration FROM daily_summary"
sqlcmd -S "(localdb)\MSSQLLocalDB" -d CoreGym_Dev -Q "SELECT current_streak, longest_streak, last_active_date, freeze_available FROM user_streaks"

# Every CHECK constraint / FK / index in the schema
sqlcmd -S "(localdb)\MSSQLLocalDB" -d CoreGym_Dev -Q "SELECT name FROM sys.check_constraints ORDER BY name"
sqlcmd -S "(localdb)\MSSQLLocalDB" -d CoreGym_Dev -Q "SELECT name FROM sys.foreign_keys ORDER BY name"
```

Or open the database in a GUI — connection details for SSMS / Azure Data
Studio / DataGrip:

```
Server:   (localdb)\MSSQLLocalDB
Database: CoreGym_Dev
Auth:     Windows (Trusted Connection)
```

## Step 7 — Stop & clean up

- Stop the API: **Ctrl+C** in the terminal running it.
- Reset the dev data: `sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "DROP DATABASE [CoreGym_Dev]"`
  (recreate with Step 2; migrations reapply on next boot).
- Files you create while playing (test uploads) land in `file-storage/` — gitignored.

## Troubleshooting

| Symptom | Fix |
|---|---|
| `/swagger` is 404 | `ASPNETCORE_ENVIRONMENT=Development` wasn't set — set it and restart |
| 401 on everything | Token missing/expired, or the signing key/issuer/audience env vars don't match `scripts/dev-token.ps1` |
| 500 with a CHECK constraint error | You sent a value outside the prod-constrained lists (see `docs/ARCHITECTURE.md` → CHECK constraints) |
| `sqllocaldb` not found | Install "SQL Server Express LocalDB" via the Visual Studio installer or SQL Server Express |
| Port 5080 busy | Change `$env:ASPNETCORE_URLS` (and the URLs in the examples) |
