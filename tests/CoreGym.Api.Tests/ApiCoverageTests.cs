using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CoreGym.Domain.Entities;
using CoreGym.Infrastructure;
using CoreGym.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace CoreGym.Api.Tests;

/// <summary>
/// End-to-end tests for the coverage endpoints: view reads, user-side writes,
/// coach content/assignments/phases/onboarding and non-Stripe subscriptions.
/// </summary>
[Collection("api-smoke")]
public class ApiCoverageTests
{
    private readonly ApiSmokeFixture _fx;

    public ApiCoverageTests(ApiSmokeFixture fx) => _fx = fx;

    [Fact]
    public async Task Personal_records_endpoint_serves_the_view()
    {
        var userId = await CreateUserAsync();
        await SeedSetsAsync(userId, ("Bench Press", 80m, 8), ("Squat", 140m, 3), ("Bench Press", 90m, 5));

        var client = _fx.CreateClient(userId);
        var all = await client.GetAsync("/api/me/personal-records");
        Assert.Equal(HttpStatusCode.OK, all.StatusCode);
        using var allDoc = JsonDocument.Parse(await all.Content.ReadAsStringAsync());
        Assert.Equal(2, allDoc.RootElement.GetArrayLength());

        var filtered = await client.GetAsync("/api/me/personal-records?exerciseName=Bench%20Press");
        using var filteredDoc = JsonDocument.Parse(await filtered.Content.ReadAsStringAsync());
        Assert.Equal(1, filteredDoc.RootElement.GetArrayLength());
        Assert.Equal(90m, filteredDoc.RootElement[0].GetProperty("bestWeightKg").GetDecimal());
    }

    [Fact]
    public async Task Weekly_and_weight_progress_endpoints_serve_the_views()
    {
        var userId = await CreateUserAsync();
        await using (var ctx = _fx.CreateContext())
        {
            ctx.WeeklyActivities.AddRange(
                new WeeklyActivity { Id = Guid.NewGuid(), UserId = userId, WeekStart = new DateTime(2026, 8, 31, 0, 0, 0, DateTimeKind.Utc), DayIndex = 0, ActualPct = 100, GoalPct = 100 },
                new WeeklyActivity { Id = Guid.NewGuid(), UserId = userId, WeekStart = new DateTime(2026, 9, 7, 0, 0, 0, DateTimeKind.Utc), DayIndex = 1, ActualPct = 50, GoalPct = 100 });
            ctx.BodyMeasurements.AddRange(
                new BodyMeasurement { Id = Guid.NewGuid(), UserId = userId, MeasuredDate = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc), WeightKg = 80.0m },
                new BodyMeasurement { Id = Guid.NewGuid(), UserId = userId, MeasuredDate = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc), WeightKg = 78.5m });
            await ctx.SaveChangesAsync();
        }

        var client = _fx.CreateClient(userId);

        var weekly = await client.GetAsync("/api/me/weekly-progress");
        using var weeklyDoc = JsonDocument.Parse(await weekly.Content.ReadAsStringAsync());
        Assert.Equal(2, weeklyDoc.RootElement.GetArrayLength());

        var weight = await client.GetAsync("/api/me/weight-progress");
        using var weightDoc = JsonDocument.Parse(await weight.Content.ReadAsStringAsync());
        Assert.Equal(2, weightDoc.RootElement.GetArrayLength());
        Assert.Equal(-1.5m, weightDoc.RootElement[0].GetProperty("weightChangeKg").GetDecimal());
    }

    [Fact]
    public async Task Coach_view_of_client_personal_records_is_policy_gated()
    {
        var clientId = await CreateUserAsync("cov-client@example.com");
        var (coachId, coachUserId) = await CreateCoachAsync();
        await CreateSubscriptionAsync(clientId, coachId, "active");
        await SeedSetsAsync(clientId, ("Row", 70m, 10));
        var strangerId = await CreateUserAsync("cov-stranger@example.com");

        var allowed = await _fx.CreateClient(coachUserId).GetAsync($"/api/coach/clients/{clientId}/personal-records");
        Assert.Equal(HttpStatusCode.OK, allowed.StatusCode);
        using var doc = JsonDocument.Parse(await allowed.Content.ReadAsStringAsync());
        Assert.Equal(1, doc.RootElement.GetArrayLength());

        var forbidden = await _fx.CreateClient(strangerId).GetAsync($"/api/coach/clients/{clientId}/personal-records");
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
    }

    [Fact]
    public async Task Daily_activity_ingest_and_update()
    {
        var userId = await CreateUserAsync();
        var client = _fx.CreateClient(userId);
        var today = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");

        var post = await client.PostAsJsonAsync("/api/me/daily-activity", new { steps = 5000, activeCaloriesBurned = 300.5m, date = today });
        Assert.Equal(HttpStatusCode.Created, post.StatusCode);
        using var postDoc = JsonDocument.Parse(await post.Content.ReadAsStringAsync());
        var id = postDoc.RootElement.GetProperty("id").GetGuid();

        var put = await client.PutAsJsonAsync($"/api/me/daily-activity/{id}", new { steps = 9000 });
        Assert.Equal(HttpStatusCode.OK, put.StatusCode);

        var list = await client.GetAsync($"/api/me/daily-activity?date={today}");
        using var listDoc = JsonDocument.Parse(await list.Content.ReadAsStringAsync());
        Assert.Equal(1, listDoc.RootElement.GetArrayLength());
        Assert.Equal(9000, listDoc.RootElement[0].GetProperty("steps").GetInt32());
    }

    [Fact]
    public async Task Weekly_activity_upserts_per_week_and_day()
    {
        var userId = await CreateUserAsync();
        var client = _fx.CreateClient(userId);
        var week = "2026-09-07";

        await client.PutAsJsonAsync("/api/me/weekly-activity", new { weekStart = week, dayIndex = 0, actualPct = 100 });
        await client.PutAsJsonAsync("/api/me/weekly-activity", new { weekStart = week, dayIndex = 0, actualPct = 80 });
        await client.PutAsJsonAsync("/api/me/weekly-activity", new { weekStart = week, dayIndex = 1, actualPct = 50 });

        var list = await client.GetAsync($"/api/me/weekly-activity?weekStart={week}");
        using var doc = JsonDocument.Parse(await list.Content.ReadAsStringAsync());
        Assert.Equal(2, doc.RootElement.GetArrayLength());
        Assert.Equal(80, doc.RootElement[0].GetProperty("actualPct").GetInt32());
    }

    [Fact]
    public async Task Exercise_progress_create_and_list()
    {
        var userId = await CreateUserAsync();
        var client = _fx.CreateClient(userId);
        var today = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");

        var post = await client.PostAsJsonAsync("/api/me/exercise-progress", new
        {
            bestSetWeight = 100m,
            bestSetReps = 5,
            totalVolume = 2400m,
            oneRmEstimate = 116.7m,
            sessionDate = today,
        });
        Assert.Equal(HttpStatusCode.Created, post.StatusCode);

        var list = await client.GetAsync($"/api/me/exercise-progress?date={today}");
        using var doc = JsonDocument.Parse(await list.Content.ReadAsStringAsync());
        Assert.Equal(1, doc.RootElement.GetArrayLength());
        Assert.Equal(116.7m, doc.RootElement[0].GetProperty("oneRmEstimate").GetDecimal());
    }

    [Fact]
    public async Task User_programs_crud()
    {
        var userId = await CreateUserAsync();
        var client = _fx.CreateClient(userId);

        var post = await client.PostAsJsonAsync("/api/me/user-programs", new { programName = "My Push", muscleGroup = "chest" });
        Assert.Equal(HttpStatusCode.Created, post.StatusCode);
        using var postDoc = JsonDocument.Parse(await post.Content.ReadAsStringAsync());
        var id = postDoc.RootElement.GetProperty("id").GetGuid();

        var put = await client.PutAsJsonAsync($"/api/me/user-programs/{id}", new { programName = "My Push v2", muscleGroup = "chest", isActive = false });
        Assert.Equal(HttpStatusCode.OK, put.StatusCode);

        var delete = await client.DeleteAsync($"/api/me/user-programs/{id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var list = await client.GetAsync("/api/me/user-programs");
        using var listDoc = JsonDocument.Parse(await list.Content.ReadAsStringAsync());
        Assert.Equal(0, listDoc.RootElement.GetArrayLength());

        // user_programs.muscle_group is CHECK-constrained to chest/arms/legs/core in prod.
        var invalid = await client.PostAsJsonAsync("/api/me/user-programs", new { programName = "Cardio", muscleGroup = "cardio" });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
    }

    [Fact]
    public async Task Barcode_scan_history_allows_null_barcode()
    {
        var userId = await CreateUserAsync();
        var client = _fx.CreateClient(userId);

        await client.PostAsJsonAsync("/api/me/barcode-scans", new { barcode = "6221031492888", quantityG = 50.5m });
        await client.PostAsJsonAsync("/api/me/barcode-scans", new { barcode = (string?)null });

        var list = await client.GetAsync("/api/me/barcode-scans");
        using var doc = JsonDocument.Parse(await list.Content.ReadAsStringAsync());
        Assert.Equal(2, doc.RootElement.GetArrayLength());
    }

    [Fact]
    public async Task Coach_clients_lists_only_active_subscriptions()
    {
        var activeClientId = await CreateUserAsync("cov-active-client@example.com");
        var pendingClientId = await CreateUserAsync("cov-pending-client@example.com");
        var (coachId, coachUserId) = await CreateCoachAsync();
        await CreateSubscriptionAsync(activeClientId, coachId, "active");
        await CreateSubscriptionAsync(pendingClientId, coachId, "pending");

        var response = await _fx.CreateClient(coachUserId).GetAsync("/api/coach/clients");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(1, doc.RootElement.GetArrayLength());
        Assert.Equal("cov-active-client", doc.RootElement[0].GetProperty("clientName").GetString());
    }

    [Fact]
    public async Task Coach_content_crud_and_client_visibility()
    {
        var clientId = await CreateUserAsync("cov-content-client@example.com");
        var (coachId, coachUserId) = await CreateCoachAsync();
        var coachClient = _fx.CreateClient(coachUserId);

        var createPrivate = await coachClient.PostAsJsonAsync("/api/coach/content", new { title = "Secret Plan", type = "pdf", fileUrl = "coach-pdfs/secret.pdf", isPublic = false });
        Assert.Equal(HttpStatusCode.Created, createPrivate.StatusCode);
        using var privateDoc = JsonDocument.Parse(await createPrivate.Content.ReadAsStringAsync());
        var privateId = privateDoc.RootElement.GetProperty("id").GetGuid();

        var createPublic = await coachClient.PostAsJsonAsync("/api/coach/content", new { title = "Public Guide", type = "video", fileUrl = "coach-media/guide.mp4", isPublic = true });
        using var publicDoc = JsonDocument.Parse(await createPublic.Content.ReadAsStringAsync());
        var publicId = publicDoc.RootElement.GetProperty("id").GetGuid();

        // Client sees only the public item.
        var clientView = await _fx.CreateClient(clientId).GetAsync($"/api/coaches/{coachId}/content");
        using var clientDoc = JsonDocument.Parse(await clientView.Content.ReadAsStringAsync());
        Assert.Equal(1, clientDoc.RootElement.GetArrayLength());
        Assert.Equal("Public Guide", clientDoc.RootElement[0].GetProperty("title").GetString());

        var update = await coachClient.PutAsJsonAsync($"/api/coach/content/{publicId}", new { title = "Public Guide v2" });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);

        // Deleting assigned content is blocked with 409.
        await coachClient.PostAsJsonAsync("/api/coach/assignments", new { clientId, contentId = privateId });
        var deleteAssigned = await coachClient.DeleteAsync($"/api/coach/content/{privateId}");
        Assert.Equal(HttpStatusCode.Conflict, deleteAssigned.StatusCode);

        var deletePublic = await coachClient.DeleteAsync($"/api/coach/content/{publicId}");
        Assert.Equal(HttpStatusCode.NoContent, deletePublic.StatusCode);
    }

    [Fact]
    public async Task Assignment_flow_connects_coach_content_to_client()
    {
        var clientId = await CreateUserAsync("cov-assign-client@example.com");
        var (coachId, coachUserId) = await CreateCoachAsync();

        var create = await _fx.CreateClient(coachUserId).PostAsJsonAsync("/api/coach/content", new { title = "Nutrition 101", type = "pdf", fileUrl = "coach-pdfs/n101.pdf", isPublic = false });
        using var createDoc = JsonDocument.Parse(await create.Content.ReadAsStringAsync());
        var contentId = createDoc.RootElement.GetProperty("id").GetGuid();

        var assign = await _fx.CreateClient(coachUserId).PostAsJsonAsync("/api/coach/assignments", new { clientId, contentId });
        Assert.Equal(HttpStatusCode.Created, assign.StatusCode);

        var clientList = await _fx.CreateClient(clientId).GetAsync("/api/me/assignments");
        using var clientDoc = JsonDocument.Parse(await clientList.Content.ReadAsStringAsync());
        Assert.Equal(1, clientDoc.RootElement.GetArrayLength());
        Assert.Equal("Nutrition 101", clientDoc.RootElement[0].GetProperty("contentTitle").GetString());

        // The assignment makes the private content visible to the client.
        var clientView = await _fx.CreateClient(clientId).GetAsync($"/api/coaches/{coachId}/content");
        using var viewDoc = JsonDocument.Parse(await clientView.Content.ReadAsStringAsync());
        Assert.Equal(1, viewDoc.RootElement.GetArrayLength());
    }

    [Fact]
    public async Task Subscription_create_status_and_phases_flow()
    {
        var clientId = await CreateUserAsync("cov-phases-client@example.com");
        var (coachId, coachUserId) = await CreateCoachAsync();
        var coachClient = _fx.CreateClient(coachUserId);

        var create = await coachClient.PostAsJsonAsync("/api/coach/subscriptions", new { clientId, tier = "premium" });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        using var createDoc = JsonDocument.Parse(await create.Content.ReadAsStringAsync());
        var subscriptionId = createDoc.RootElement.GetProperty("id").GetGuid();

        // Invalid tier is rejected by the CHECK-backed validation.
        var badTier = await coachClient.PostAsJsonAsync("/api/coach/subscriptions", new { clientId, tier = "gold" });
        Assert.Equal(HttpStatusCode.BadRequest, badTier.StatusCode);

        // Status transition applies lifecycle side effects (conversation creation).
        var patch = await coachClient.PatchAsJsonAsync($"/api/coach/subscriptions/{subscriptionId}/status", new { status = "active" });
        Assert.Equal(HttpStatusCode.OK, patch.StatusCode);
        await using (var ctx = _fx.CreateContext())
        {
            Assert.NotNull(await ctx.Conversations.SingleOrDefaultAsync(c => c.SubscriptionId == subscriptionId));
        }

        var createPhase = await coachClient.PostAsJsonAsync($"/api/coach/subscriptions/{subscriptionId}/phases", new { phaseNumber = 1, title = "Foundation", type = "combined", durationWeeks = 4 });
        Assert.Equal(HttpStatusCode.Created, createPhase.StatusCode);
        using var phaseDoc = JsonDocument.Parse(await createPhase.Content.ReadAsStringAsync());
        var phaseId = phaseDoc.RootElement.GetProperty("id").GetGuid();

        var clientPhases = await _fx.CreateClient(clientId).GetAsync($"/api/me/subscriptions/{subscriptionId}/phases");
        Assert.Equal(HttpStatusCode.OK, clientPhases.StatusCode);
        using var clientPhasesDoc = JsonDocument.Parse(await clientPhases.Content.ReadAsStringAsync());
        Assert.Equal(1, clientPhasesDoc.RootElement.GetArrayLength());
        Assert.Equal("Foundation", clientPhasesDoc.RootElement[0].GetProperty("title").GetString());

        // Another user cannot read this subscription's phases.
        var strangerId = await CreateUserAsync("cov-phases-stranger@example.com");
        var strangerView = await _fx.CreateClient(strangerId).GetAsync($"/api/me/subscriptions/{subscriptionId}/phases");
        Assert.Equal(HttpStatusCode.NotFound, strangerView.StatusCode);

        var updatePhase = await coachClient.PutAsJsonAsync($"/api/coach/subscriptions/{subscriptionId}/phases/{phaseId}", new { status = "completed", completedAt = DateTime.UtcNow });
        Assert.Equal(HttpStatusCode.OK, updatePhase.StatusCode);

        var deletePhase = await coachClient.DeleteAsync($"/api/coach/subscriptions/{subscriptionId}/phases/{phaseId}");
        Assert.Equal(HttpStatusCode.NoContent, deletePhase.StatusCode);
    }

    [Fact]
    public async Task Coach_onboarding_upserts_own_row()
    {
        var coachUserId = await CreateUserAsync("cov-coach-onboarding@example.com");
        var client = _fx.CreateClient(coachUserId);

        var put = await client.PutAsJsonAsync("/api/coach/onboarding", new { displayName = "Coach Ali", yearsExperience = 5, isCompleted = true });
        Assert.Equal(HttpStatusCode.OK, put.StatusCode);
        using var putDoc = JsonDocument.Parse(await put.Content.ReadAsStringAsync());
        // Languages keep their ["Arabic","English"] default when not supplied.
        Assert.Equal("Arabic", putDoc.RootElement.GetProperty("languages")[0].GetString());

        var get = await client.GetAsync("/api/coach/onboarding");
        using var getDoc = JsonDocument.Parse(await get.Content.ReadAsStringAsync());
        Assert.Equal("Coach Ali", getDoc.RootElement.GetProperty("displayName").GetString());
        Assert.Equal(5, getDoc.RootElement.GetProperty("yearsExperience").GetInt32());
    }

    private async Task<Guid> CreateUserAsync(string email = "cov-user@example.com")
    {
        await using var ctx = _fx.CreateContext();
        var profile = await new ProfileProvisioningService(ctx).ProvisionAsync(Guid.NewGuid(), email: email, name: email.Split('@')[0]);
        return profile.Id;
    }

    private async Task<(Guid CoachId, Guid CoachUserId)> CreateCoachAsync()
    {
        await using var ctx = _fx.CreateContext();
        var coachUser = await new ProfileProvisioningService(ctx).ProvisionAsync(Guid.NewGuid(), name: "cov-coach");
        var coach = new Coach { Id = Guid.NewGuid(), UserId = coachUser.Id };
        ctx.Coaches.Add(coach);
        await ctx.SaveChangesAsync();
        return (coach.Id, coachUser.Id);
    }

    private async Task<Guid> CreateSubscriptionAsync(Guid clientId, Guid coachId, string status)
    {
        await using var ctx = _fx.CreateContext();
        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            CoachId = coachId,
            Status = status,
        };
        ctx.Subscriptions.Add(subscription);
        await ctx.SaveChangesAsync();
        return subscription.Id;
    }

    private async Task SeedSetsAsync(Guid userId, params (string ExerciseName, decimal WeightKg, int Reps)[] sets)
    {
        await using var ctx = _fx.CreateContext();
        var now = DateTimeOffset.UtcNow;
        foreach (var set in sets)
        {
            ctx.WorkoutSets.Add(new WorkoutSet
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ExerciseName = set.ExerciseName,
                SetNumber = 1,
                Reps = set.Reps,
                WeightKg = set.WeightKg,
                LoggedAt = now,
            });
        }

        await ctx.SaveChangesAsync();
    }
}
