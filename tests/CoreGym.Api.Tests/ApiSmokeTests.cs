using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CoreGym.Domain.Entities;
using CoreGym.Infrastructure;
using CoreGym.Infrastructure.Authorization;
using CoreGym.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace CoreGym.Api.Tests;

/// <summary>
/// End-to-end HTTP tests: routing → JWT authentication → OwnDataOrActiveCoach
/// authorization → application services → SQL Server, through the real pipeline.
/// </summary>
[Collection("api-smoke")]
public class ApiSmokeTests
{
    private readonly ApiSmokeFixture _fx;

    public ApiSmokeTests(ApiSmokeFixture fx) => _fx = fx;

    [Fact]
    public async Task Me_requires_authentication()
    {
        var response = await _fx.CreateAnonymousClient().GetAsync("/api/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_me_returns_the_provisioned_profile()
    {
        var userId = await CreateUserAsync();

        var response = await _fx.CreateClient(userId).GetAsync("/api/me");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("seeded@example.com", doc.RootElement.GetProperty("email").GetString());
        Assert.Equal("client", doc.RootElement.GetProperty("role").GetString());
    }

    [Fact]
    public async Task Update_me_persists_changes()
    {
        var userId = await CreateUserAsync();
        var client = _fx.CreateClient(userId);

        var put = await client.PutAsJsonAsync("/api/me", new { name = "Renamed", age = 30, weightKg = 82.5m });
        Assert.Equal(HttpStatusCode.OK, put.StatusCode);

        var get = await client.GetAsync("/api/me");
        using var doc = JsonDocument.Parse(await get.Content.ReadAsStringAsync());
        Assert.Equal("Renamed", doc.RootElement.GetProperty("name").GetString());
        Assert.Equal(30, doc.RootElement.GetProperty("age").GetInt32());
    }

    [Fact]
    public async Task Nutrition_log_end_to_end_syncs_the_daily_summary()
    {
        var userId = await CreateUserAsync();
        var client = _fx.CreateClient(userId);
        var today = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");

        var post = await client.PostAsJsonAsync("/api/nutrition/logs", new
        {
            foodName = "Rice",
            calories = 300m,
            proteinG = 10m,
            carbsG = 50m,
            fatG = 2m,
            date = today,
        });
        Assert.Equal(HttpStatusCode.Created, post.StatusCode);

        var summary = await client.GetAsync($"/api/me/summary/{today}");
        Assert.Equal(HttpStatusCode.OK, summary.StatusCode);
        using var doc = JsonDocument.Parse(await summary.Content.ReadAsStringAsync());
        Assert.Equal(300m, doc.RootElement.GetProperty("caloriesConsumed").GetDecimal());
        Assert.Equal(10m, doc.RootElement.GetProperty("proteinG").GetDecimal());
    }

    [Fact]
    public async Task Streak_end_to_end_records_activity_and_reports_status()
    {
        var userId = await CreateUserAsync();
        var client = _fx.CreateClient(userId);

        var post = await client.PostAsJsonAsync("/api/me/streak/activity", new { source = "workout" });
        Assert.Equal(HttpStatusCode.OK, post.StatusCode);
        using var postDoc = JsonDocument.Parse(await post.Content.ReadAsStringAsync());
        Assert.Equal(1, postDoc.RootElement.GetProperty("currentStreak").GetInt32());
        Assert.False(postDoc.RootElement.GetProperty("atRisk").GetBoolean());

        var get = await client.GetAsync("/api/me/streak");
        using var getDoc = JsonDocument.Parse(await get.Content.ReadAsStringAsync());
        Assert.True(getDoc.RootElement.GetProperty("loggedWorkoutToday").GetBoolean());
    }

    [Fact]
    public async Task Workout_end_to_end_syncs_summary_and_delete_resyncs()
    {
        var userId = await CreateUserAsync();
        var client = _fx.CreateClient(userId);
        var today = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");

        var post = await client.PostAsJsonAsync("/api/workouts/sessions", new
        {
            muscleGroup = "chest",
            sessionName = "Push",
            durationMin = 45,
            date = today,
            sets = new[] { new { exerciseName = "Bench Press", setNumber = 1, reps = 8, weightKg = 80m } },
        });
        Assert.Equal(HttpStatusCode.Created, post.StatusCode);
        using var postDoc = JsonDocument.Parse(await post.Content.ReadAsStringAsync());
        var sessionId = postDoc.RootElement.GetProperty("session").GetProperty("id").GetGuid();

        var summary = await client.GetAsync($"/api/me/summary/{today}");
        using var summaryDoc = JsonDocument.Parse(await summary.Content.ReadAsStringAsync());
        Assert.True(summaryDoc.RootElement.GetProperty("workoutDone").GetBoolean());
        Assert.Equal(45, summaryDoc.RootElement.GetProperty("workoutDuration").GetInt32());

        var delete = await client.DeleteAsync($"/api/workouts/sessions/{sessionId}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var summaryAfter = await client.GetAsync($"/api/me/summary/{today}");
        using var afterDoc = JsonDocument.Parse(await summaryAfter.Content.ReadAsStringAsync());
        Assert.False(afterDoc.RootElement.GetProperty("workoutDone").GetBoolean());
    }

    [Fact]
    public async Task Coach_client_views_are_gated_by_the_own_data_or_active_coach_policy()
    {
        var clientId = await CreateUserAsync("client-in-policy@example.com");
        var coachUserId = await CreateCoachWithSubscriptionAsync(clientId, "active");
        var strangerId = await CreateUserAsync("stranger@example.com");
        var coachClient = _fx.CreateClient(coachUserId);

        await using (var ctx = _fx.CreateContext())
        {
            ctx.DailySummaries.Add(new DailySummary
            {
                Id = Guid.NewGuid(),
                UserId = clientId,
                SummaryDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Steps = 6000,
            });
            await ctx.SaveChangesAsync();
        }

        // Coach with an active subscription: allowed and sees the client's data.
        var allowed = await coachClient.GetAsync("/api/coach/clients/{clientId}/summary/2026-01-01"
            .Replace("{clientId}", clientId.ToString()));
        Assert.Equal(HttpStatusCode.OK, allowed.StatusCode);
        using var allowedDoc = JsonDocument.Parse(await allowed.Content.ReadAsStringAsync());
        Assert.Equal(6000, allowedDoc.RootElement.GetProperty("steps").GetInt32());

        // Unrelated user: forbidden.
        var strangerClient = _fx.CreateClient(strangerId);
        var forbidden = await strangerClient.GetAsync($"/api/coach/clients/{clientId}/summary/2026-01-01");
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);

        // Subscription lapses: the same coach is now forbidden.
        await using (var ctx = _fx.CreateContext())
        {
            var subscription = await ctx.Subscriptions.SingleAsync(s => s.ClientId == clientId);
            subscription.Status = "cancelled";
            await ctx.SaveChangesAsync();
        }

        var afterCancel = await coachClient.GetAsync($"/api/coach/clients/{clientId}/summary/2026-01-01");
        Assert.Equal(HttpStatusCode.Forbidden, afterCancel.StatusCode);
    }

    [Fact]
    public async Task Chat_end_to_end_sends_notifies_and_marks_read()
    {
        var (clientId, coachId, conversationId) = await CreateConversationAsync();

        var chatClient = _fx.CreateClient(clientId);
        var post = await chatClient.PostAsJsonAsync($"/api/chat/conversations/{conversationId}/messages", new { content = "hi coach" });
        Assert.Equal(HttpStatusCode.Created, post.StatusCode);

        var coachClient = _fx.CreateClient(coachId);
        var conversations = await coachClient.GetAsync("/api/chat/conversations");
        using var conversationsDoc = JsonDocument.Parse(await conversations.Content.ReadAsStringAsync());
        Assert.Equal(1, conversationsDoc.RootElement.GetArrayLength());
        Assert.Equal(1, conversationsDoc.RootElement[0].GetProperty("coachUnread").GetInt32());

        var unreadBefore = await coachClient.GetAsync("/api/chat/unread");
        using var unreadDoc = JsonDocument.Parse(await unreadBefore.Content.ReadAsStringAsync());
        Assert.Equal(1, unreadDoc.RootElement.GetProperty("count").GetInt32());

        var read = await coachClient.PostAsync($"/api/chat/conversations/{conversationId}/read", null);
        Assert.Equal(HttpStatusCode.OK, read.StatusCode);
        using var readDoc = JsonDocument.Parse(await read.Content.ReadAsStringAsync());
        Assert.Equal(1, readDoc.RootElement.GetProperty("marked").GetInt32());

        var unreadAfter = await coachClient.GetAsync("/api/chat/unread");
        using var unreadAfterDoc = JsonDocument.Parse(await unreadAfter.Content.ReadAsStringAsync());
        Assert.Equal(0, unreadAfterDoc.RootElement.GetProperty("count").GetInt32());
    }

    [Fact]
    public async Task Non_participant_cannot_read_conversation_messages()
    {
        var (clientId, _, conversationId) = await CreateConversationAsync();
        var strangerId = await CreateUserAsync("chat-stranger@example.com");

        var response = await _fx.CreateClient(strangerId).GetAsync($"/api/chat/conversations/{conversationId}/messages");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Review_updates_the_coach_rating_over_http()
    {
        var clientId = await CreateUserAsync("reviewer@example.com");
        var coachId = await CreateCoachAsync();

        var post = await _fx.CreateClient(clientId).PostAsJsonAsync($"/api/coaches/{coachId}/reviews", new { rating = 5, comment = "great" });
        Assert.Equal(HttpStatusCode.Created, post.StatusCode);

        var detail = await _fx.CreateAnonymousClient().GetAsync($"/api/coaches/{coachId}");
        using var doc = JsonDocument.Parse(await detail.Content.ReadAsStringAsync());
        Assert.Equal(5m, doc.RootElement.GetProperty("rating").GetDecimal());
    }

    [Fact]
    public async Task Public_reference_data_is_anonymous()
    {
        await using (var ctx = _fx.CreateContext())
        {
            ctx.Foods.Add(new Food { Id = Guid.NewGuid(), Name = "Rice Bowl", Calories = 400m });
            await ctx.SaveChangesAsync();
        }

        var response = await _fx.CreateAnonymousClient().GetAsync("/api/foods?search=rice");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(1, doc.RootElement.GetArrayLength());
    }

    [Fact]
    public async Task Domain_errors_map_to_problem_details()
    {
        var userId = await CreateUserAsync("errors@example.com");

        var notFound = await _fx.CreateClient(userId).DeleteAsync($"/api/nutrition/logs/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, notFound.StatusCode);

        var badRequest = await _fx.CreateClient(userId).PostAsJsonAsync("/api/coaches/{0}/reviews".Replace("{0}", Guid.NewGuid().ToString()), new { rating = 9 });
        Assert.Equal(HttpStatusCode.BadRequest, badRequest.StatusCode);
        using var doc = JsonDocument.Parse(await badRequest.Content.ReadAsStringAsync());
        Assert.Equal("Invalid request", doc.RootElement.GetProperty("title").GetString());
    }

    private async Task<Guid> CreateUserAsync(string email = "seeded@example.com")
    {
        await using var ctx = _fx.CreateContext();
        var profile = await new ProfileProvisioningService(ctx).ProvisionAsync(Guid.NewGuid(), email: email, name: "Api User");
        return profile.Id;
    }

    private async Task<Guid> CreateCoachAsync()
    {
        await using var ctx = _fx.CreateContext();
        var coachUser = await new ProfileProvisioningService(ctx).ProvisionAsync(Guid.NewGuid(), name: "Coach");
        var coach = new Coach { Id = Guid.NewGuid(), UserId = coachUser.Id };
        ctx.Coaches.Add(coach);
        ctx.CoachProfiles.Add(new CoachProfile { Id = coachUser.Id });
        await ctx.SaveChangesAsync();
        return coach.Id;
    }

    private async Task<Guid> CreateCoachWithSubscriptionAsync(Guid clientId, string status)
    {
        await using var ctx = _fx.CreateContext();
        var coachUser = await new ProfileProvisioningService(ctx).ProvisionAsync(Guid.NewGuid(), name: "Policy Coach");
        var coach = new Coach { Id = Guid.NewGuid(), UserId = coachUser.Id };
        ctx.Coaches.Add(coach);
        ctx.Subscriptions.Add(new Subscription
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            CoachId = coach.Id,
            Status = status,
        });
        await ctx.SaveChangesAsync();
        return coachUser.Id;
    }

    private async Task<(Guid ClientId, Guid CoachId, Guid ConversationId)> CreateConversationAsync()
    {
        await using var ctx = _fx.CreateContext();
        var client = await new ProfileProvisioningService(ctx).ProvisionAsync(Guid.NewGuid(), name: "chat-client");
        var coach = await new ProfileProvisioningService(ctx).ProvisionAsync(Guid.NewGuid(), name: "chat-coach");
        var conversation = new Conversation { Id = Guid.NewGuid(), ClientId = client.Id, CoachId = coach.Id };
        ctx.Conversations.Add(conversation);
        await ctx.SaveChangesAsync();
        return (client.Id, coach.Id, conversation.Id);
    }
}
