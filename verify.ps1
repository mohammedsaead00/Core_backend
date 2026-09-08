#Requires -Version 5.1
<#
.SYNOPSIS
  One-command verification for the CoreGym backend.

.DESCRIPTION
  1. Checks prerequisites (.NET SDK, SQL Server LocalDB)
  2. Builds the solution
  3. Runs the full integration test suite (105 tests)
  4. Boots the real API against a scratch LocalDB database (migrations applied on boot)
  5. Walks the main user flows over HTTP with a live PASS/FAIL checklist
  6. Cleans up (stops the API, drops the scratch database)

.PARAMETER SkipTests
  Skips step 3 (faster).

.PARAMETER KeepDb
  Keeps the scratch database CoreGym_Verify after the run (default: dropped).

.PARAMETER Port
  HTTP port for the API (default 5080).

.EXAMPLE
  powershell -ExecutionPolicy Bypass -File verify.ps1
  powershell -ExecutionPolicy Bypass -File verify.ps1 -SkipTests -KeepDb
#>
param(
    [switch]$SkipTests,
    [switch]$KeepDb,
    [int]$Port = 5080
)

$ErrorActionPreference = "Stop"
$repo = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $repo

$dbName     = "CoreGym_Verify"
$server     = "(localdb)\MSSQLLocalDB"
$issuer     = "coregym-verify"
$audience   = "coregym-api"
$signingKey = "verify-signing-key-0123456789abcdef0123456789abcdef"
$baseUrl    = "http://localhost:$Port"
$dll        = Join-Path $repo "src\CoreGym.Api\bin\Debug\net10.0\CoreGym.Api.dll"
$logOut     = Join-Path $repo "verify-api.out.log"
$logErr     = Join-Path $repo "verify-api.err.log"
$today      = (Get-Date).ToUniversalTime().ToString("yyyy-MM-dd")

$script:checks = New-Object System.Collections.Generic.List[object]
$script:apiProcess = $null

function Add-Check([string]$name, [bool]$passed, [string]$detail) {
    $result = "PASS"
    if (-not $passed) { $result = "FAIL" }
    $script:checks.Add([pscustomobject]@{ Check = $name; Result = $result; Detail = $detail })
    if ($passed) { Write-Host "  [PASS] $name" -ForegroundColor Green }
    else { Write-Host "  [FAIL] $name  ($detail)" -ForegroundColor Red }
}

function Invoke-Sql([string]$query) {
    sqlcmd -S $server -d master -Q $query -b | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "sqlcmd failed: $query" }
}

function New-TestToken([string]$userId) {
    function b64url([byte[]]$bytes) {
        [Convert]::ToBase64String($bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_')
    }
    $header = b64url ([System.Text.Encoding]::UTF8.GetBytes('{"alg":"HS256","typ":"JWT"}'))
    $claims = @{ sub = $userId; iss = $issuer; aud = $audience; exp = [DateTimeOffset]::UtcNow.AddHours(2).ToUnixTimeSeconds() }
    $payload = b64url ([System.Text.Encoding]::UTF8.GetBytes(($claims | ConvertTo-Json -Compress)))
    $hmac = [System.Security.Cryptography.HMACSHA256]::new([System.Text.Encoding]::UTF8.GetBytes($signingKey))
    $signature = b64url ($hmac.ComputeHash([System.Text.Encoding]::UTF8.GetBytes("$header.$payload")))
    "$header.$payload.$signature"
}

function Invoke-Api([string]$method, [string]$path, [object]$body, [string]$token = "") {
    $headers = @{}
    if ($token) { $headers.Authorization = "Bearer $token" }
    try {
        if ($null -ne $body) {
            $json = [System.Text.Encoding]::UTF8.GetBytes(($body | ConvertTo-Json -Depth 6 -Compress))
            $response = Invoke-WebRequest -Uri "$baseUrl$path" -Method $method -Headers $headers -ContentType "application/json" -Body $json -UseBasicParsing
        }
        else {
            $response = Invoke-WebRequest -Uri "$baseUrl$path" -Method $method -Headers $headers -UseBasicParsing
        }
        return [pscustomobject]@{ Status = [int]$response.StatusCode; Content = [string]$response.Content }
    }
    catch {
        $status = 0
        if ($_.Exception.Response) { $status = [int]$_.Exception.Response.StatusCode }
        return [pscustomobject]@{ Status = $status; Content = "" }
    }
}

try {
    # ---------------------------------------------------------------- 1. prereqs
    Write-Host ""
    Write-Host "=== 1/6 Prerequisites ===" -ForegroundColor Cyan

    $dotnetVersion = $null
    try { $dotnetVersion = (dotnet --version).Trim() } catch { }
    Add-Check ".NET SDK available" ($null -ne $dotnetVersion) "install the .NET 10 SDK from https://dotnet.microsoft.com"

    sqllocaldb start MSSQLLocalDB 2>$null | Out-Null
    $localdbInfo = sqllocaldb info MSSQLLocalDB 2>$null | Out-String
    Add-Check "SQL Server LocalDB (MSSQLLocalDB)" ($localdbInfo -match "MSSQLLocalDB") "run: sqllocaldb create MSSQLLocalDB"

    # ---------------------------------------------------------------- 2. build
    Write-Host ""
    Write-Host "=== 2/6 Build ===" -ForegroundColor Cyan
    dotnet build CoreGym.sln --nologo -v q | Out-Null
    Add-Check "Solution builds" ($LASTEXITCODE -eq 0) "run 'dotnet build CoreGym.sln' to see errors"

    # ---------------------------------------------------------------- 3. tests
    Write-Host ""
    Write-Host "=== 3/6 Integration test suite ===" -ForegroundColor Cyan
    if ($SkipTests) {
        Write-Host "  skipped (-SkipTests)" -ForegroundColor Yellow
    }
    else {
        $testOutput = dotnet test CoreGym.sln --no-build --nologo 2>&1
        $testOutput | Select-String -Pattern "Passed!|Failed!" | ForEach-Object { Write-Host "  $_" }
        Add-Check "Integration tests (Infrastructure + Api)" ($LASTEXITCODE -eq 0) "run 'dotnet test CoreGym.sln' for details"
    }

    # ---------------------------------------------------------------- 4. scratch db
    Write-Host ""
    Write-Host "=== 4/6 Scratch database ===" -ForegroundColor Cyan
    Invoke-Sql "IF DB_ID('$dbName') IS NOT NULL BEGIN ALTER DATABASE [$dbName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$dbName]; END"
    Invoke-Sql "CREATE DATABASE [$dbName]"
    Add-Check "Scratch database '$dbName' created" $true "dropped again at the end unless -KeepDb"

    # ---------------------------------------------------------------- 5. boot api
    Write-Host ""
    Write-Host "=== 5/6 Boot API (migrations apply on startup) ===" -ForegroundColor Cyan
    $env:ConnectionStrings__CoreGym   = "Server=$server;Database=$dbName;Trusted_Connection=True;TrustServerCertificate=True"
    $env:Jwt__Issuer                  = $issuer
    $env:Jwt__Audience                = $audience
    $env:Jwt__SigningKey              = $signingKey
    $env:Database__MigrateOnStartup   = "true"
    $env:Stripe__WebhookSecret        = "whsec_verify_secret"
    $env:ASPNETCORE_URLS              = $baseUrl
    $env:ASPNETCORE_ENVIRONMENT       = "Development"

    $api = Start-Process -FilePath "dotnet" -ArgumentList "`"$dll`"" -PassThru `
        -WorkingDirectory $repo -WindowStyle Hidden `
        -RedirectStandardOutput $logOut -RedirectStandardError $logErr
    $script:apiProcess = $api

    $ready = $false
    for ($i = 0; $i -lt 60; $i++) {
        Start-Sleep -Milliseconds 800
        if ($api.HasExited) { break }
        try {
            $null = Invoke-WebRequest -Uri "$baseUrl/openapi/v1.json" -UseBasicParsing -TimeoutSec 2
            $ready = $true
            break
        }
        catch { }
    }
    Add-Check "API boots, applies migrations, serves OpenAPI" $ready "see verify-api.err.log / verify-api.out.log"

    if ($ready) {
        # ------------------------------------------------------------ 6. user flows
        Write-Host ""
        Write-Host "=== 6/6 User flow over HTTP ===" -ForegroundColor Cyan
        $userId = [Guid]::NewGuid().ToString()
        $token = New-TestToken $userId

        # --- onboarding flow
        $r = Invoke-Api "POST" "/api/me" @{ email = "verify@example.com"; name = "Verify User" } $token
        Add-Check "Sign-up: profile provisioned (POST /api/me -> 201)" ($r.Status -eq 201) "status $($r.Status)"

        $r = Invoke-Api "GET" "/api/me" $null $token
        $profile = $null
        try { $profile = $r.Content | ConvertFrom-Json } catch { }
        Add-Check "Profile readable, role defaults to client" ($r.Status -eq 200 -and $profile.role -eq "client") "status $($r.Status)"

        $r = Invoke-Api "PUT" "/api/me" @{ name = "Verified User"; age = 28; weightKg = 80.5 } $token
        Add-Check "Profile update (PUT /api/me -> 200)" ($r.Status -eq 200) "status $($r.Status)"

        $r = Invoke-Api "PUT" "/api/me/onboarding" @{ age = 28; gender = "male"; goal = "muscle_gain"; activityLevel = "moderately_active"; completed = $true } $token
        Add-Check "Onboarding upsert (PUT /api/me/onboarding -> 200)" ($r.Status -eq 200) "status $($r.Status)"

        $r = Invoke-Api "PUT" "/api/me/goals" @{ dailyCalories = 2200; dailyProteinG = 160; dailyWaterMl = 3000 } $token
        Add-Check "Daily goals upsert (PUT /api/me/goals -> 200)" ($r.Status -eq 200) "status $($r.Status)"

        # --- nutrition flow (summary auto-sync)
        $r = Invoke-Api "POST" "/api/nutrition/logs" @{ foodName = "Oats"; calories = 200; proteinG = 10; carbsG = 30; fatG = 4; date = $today } $token
        Add-Check "Log food #1 (POST /api/nutrition/logs -> 201)" ($r.Status -eq 201) "status $($r.Status)"
        $r = Invoke-Api "POST" "/api/nutrition/logs" @{ foodName = "Chicken"; calories = 300; proteinG = 40; carbsG = 0; fatG = 8; date = $today } $token
        Add-Check "Log food #2 (POST /api/nutrition/logs -> 201)" ($r.Status -eq 201) "status $($r.Status)"

        $r = Invoke-Api "GET" "/api/me/summary/$today" $null $token
        $summary = $null
        try { $summary = $r.Content | ConvertFrom-Json } catch { }
        Add-Check "Daily summary auto-synced (500 kcal total)" ($r.Status -eq 200 -and $summary.caloriesConsumed -eq 500) "status $($r.Status), calories $($summary.caloriesConsumed)"

        # --- streak flow
        $r = Invoke-Api "POST" "/api/me/streak/activity" @{ source = "workout" } $token
        $streak = $null
        try { $streak = $r.Content | ConvertFrom-Json } catch { }
        Add-Check "Streak activity recorded (currentStreak = 1)" ($r.Status -eq 200 -and $streak.currentStreak -eq 1) "status $($r.Status)"

        # --- workout flow
        $r = Invoke-Api "POST" "/api/workouts/sessions" @{
            muscleGroup = "chest"; sessionName = "Verify Push"; durationMin = 40; date = $today
            sets = @(@{ exerciseName = "Bench Press"; setNumber = 1; reps = 8; weightKg = 80 })
        } $token
        Add-Check "Workout session + sets created (POST -> 201)" ($r.Status -eq 201) "status $($r.Status)"

        $r = Invoke-Api "GET" "/api/me/summary/$today" $null $token
        $summary = $null
        try { $summary = $r.Content | ConvertFrom-Json } catch { }
        Add-Check "Workout synced into summary (done, 40 min)" ($r.Status -eq 200 -and $summary.workoutDone -eq $true -and $summary.workoutDuration -eq 40) "status $($r.Status)"

        # --- chat + notifications plumbing
        $r = Invoke-Api "GET" "/api/chat/unread" $null $token
        $unread = $null
        try { $unread = $r.Content | ConvertFrom-Json } catch { }
        Add-Check "Chat unread count readable (0)" ($r.Status -eq 200 -and $unread.count -eq 0) "status $($r.Status)"

        # --- public data
        $r = Invoke-Api "GET" "/api/foods" $null ""
        Add-Check "Public food catalog (anonymous -> 200)" ($r.Status -eq 200) "status $($r.Status)"
        $r = Invoke-Api "GET" "/api/coaches" $null ""
        Add-Check "Public coach directory (anonymous -> 200)" ($r.Status -eq 200) "status $($r.Status)"

        # --- security boundaries
        $r = Invoke-Api "GET" "/api/me" $null ""
        Add-Check "JWT required (anonymous /api/me -> 401)" ($r.Status -eq 401) "status $($r.Status)"
        $r = Invoke-Api "POST" "/api/webhooks/stripe" @{ bogus = $true } ""
        $r2 = Invoke-Api "POST" "/api/webhooks/stripe" @{ bogus = $true } ""
        $headers = @{ "Stripe-Signature" = "t=1,v1=deadbeef" }
        try {
            $null = Invoke-WebRequest -Uri "$baseUrl/api/webhooks/stripe" -Method POST -Headers $headers -ContentType "application/json" -Body "{}" -UseBasicParsing
            $badSig = 200
        }
        catch { $badSig = [int]$_.Exception.Response.StatusCode }
        Add-Check "Stripe webhook rejects bad signature (400)" ($badSig -eq 400) "status $badSig"

        # --- AI guard (no Gemini key configured in this run)
        $r = Invoke-Api "POST" "/api/ai/food/text" @{ text = "2 eggs" } $token
        Add-Check "AI endpoint guard (503 without Gemini key)" ($r.Status -eq 503) "status $($r.Status)"
    }
}
finally {
    if ($script:apiProcess -and -not $script:apiProcess.HasExited) {
        try { $script:apiProcess.Kill(); $script:apiProcess.WaitForExit(5000) | Out-Null } catch { }
    }
    if (-not $KeepDb) {
        try { Invoke-Sql "IF DB_ID('$dbName') IS NOT NULL BEGIN ALTER DATABASE [$dbName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$dbName]; END" } catch { }
    }
}

# ---------------------------------------------------------------- summary
Write-Host ""
$failed = @($script:checks | Where-Object { $_.Result -eq "FAIL" }).Count
$script:checks | Format-Table Check, Result, Detail -AutoSize
if (-not $KeepDb) { Write-Host "Scratch database dropped. API logs: verify-api.out.log / verify-api.err.log" -ForegroundColor DarkGray }
if ($failed -eq 0) {
    Write-Host "ALL CHECKS PASSED ($($script:checks.Count)) -- the backend is working end to end." -ForegroundColor Green
    exit 0
}
else {
    Write-Host "$failed of $($script:checks.Count) checks FAILED -- see the red lines above." -ForegroundColor Red
    exit 1
}
