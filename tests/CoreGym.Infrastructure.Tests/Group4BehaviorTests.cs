using CoreGym.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CoreGym.Infrastructure.Tests;

/// <summary>
/// Group 4 (coaches) behavioral smoke tests. Locks in the decisions taken on
/// 2026-09-06: `reviews` and `subscriptions` are canonical; the role/status
/// CHECK constraints; and the coach_id key-space difference between
/// subscription_plans (user id) and subscriptions (coaches.id).
/// </summary>
[Collection("sql-smoke")]
public class Group4BehaviorTests
{
    private readonly SqlServerSmokeFixture _fx;

    public Group4BehaviorTests(SqlServerSmokeFixture fx) => _fx = fx;

    [Fact]
    public async Task Profiles_role_allows_client_coach_and_legacy_user_only()
    {
        var userId = await CreateUserAsync(_fx.Context);

        // The legacy 'user' value must remain valid (existing prod rows depend on it).
        await using var ctx = _fx.CreateContext();
        await ctx.Database.ExecuteSqlRawAsync(
            "UPDATE profiles SET role = {0} WHERE id = {1}", "user", userId);

        var raw = await ctx.Database
            .SqlQuery<string>($"SELECT role AS Value FROM profiles WHERE id = {userId}")
            .SingleAsync();
        Assert.Equal("user", raw);

        // Any other value must be rejected by the new CHECK constraint.
        var ex = await Assert.ThrowsAnyAsync<Exception>(() => ctx.Database.ExecuteSqlRawAsync(
            "UPDATE profiles SET role = {0} WHERE id = {1}", "admin", userId));
        Assert.Equal(547, UnwrapSqlException(ex)!.Number);
    }

    [Fact]
    public async Task Coach_defaults_and_specialization_json()
    {
        var userId = await CreateUserAsync(_fx.Context);

        var coach = new Coach
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Specialization = ["Strength", "التغذية"],
        };
        _fx.Context.Coaches.Add(coach);
        await _fx.Context.SaveChangesAsync();

        await using var ctx = _fx.CreateContext();
        var loaded = await ctx.Coaches.AsNoTracking().SingleAsync(c => c.Id == coach.Id);

        Assert.Equal(0m, loaded.PriceMonthly);
        Assert.Equal(0m, loaded.Rating);
        Assert.True(loaded.IsActive);
        Assert.Equal("", loaded.Bio);
        Assert.Equal(new List<string> { "Strength", "التغذية" }, loaded.Specialization);
    }

    [Fact]
    public async Task CoachProfile_pk_is_the_user_id_with_defaults()
    {
        var userId = await CreateUserAsync(_fx.Context);

        _fx.Context.CoachProfiles.Add(new CoachProfile { Id = userId });
        await _fx.Context.SaveChangesAsync();

        await using var ctx = _fx.CreateContext();
        var loaded = await ctx.CoachProfiles.AsNoTracking().SingleAsync(cp => cp.Id == userId);

        Assert.Equal(0, loaded.ExperienceYears);
        Assert.Equal(0m, loaded.PricePerMonth);
        Assert.Equal("EGP", loaded.Currency);
        Assert.Equal(0m, loaded.Rating);
        Assert.Equal(0, loaded.ReviewsCount);
        Assert.True(loaded.IsAvailable);
        Assert.False(loaded.IsVerified);
        Assert.Equal(20, loaded.MaxClients);
        Assert.Equal(0, loaded.CurrentClients);
    }

    [Fact]
    public async Task CoachOnboarding_languages_default_to_arabic_and_english()
    {
        var userId = await CreateUserAsync(_fx.Context);

        _fx.Context.CoachOnboardings.Add(new CoachOnboarding { Id = Guid.NewGuid(), UserId = userId });
        await _fx.Context.SaveChangesAsync();

        await using var ctx = _fx.CreateContext();
        var loaded = await ctx.CoachOnboardings.AsNoTracking().SingleAsync(co => co.UserId == userId);

        Assert.Equal(new List<string> { "Arabic", "English" }, loaded.Languages);
        Assert.Equal(0, loaded.YearsExperience);
        Assert.Equal(10, loaded.MaxClients);
        Assert.False(loaded.IsCompleted);
    }

    [Fact]
    public async Task Subscription_defaults_status_check_and_trigger()
    {
        var (profileId, coachId) = await CreateCoachAsync(_fx.Context);

        var subscription = new Subscription { Id = Guid.NewGuid(), ClientId = profileId, CoachId = coachId };
        _fx.Context.Subscriptions.Add(subscription);
        await _fx.Context.SaveChangesAsync();

        await using var ctx = _fx.CreateContext();
        var loaded = await ctx.Subscriptions.AsNoTracking().SingleAsync(s => s.Id == subscription.Id);

        Assert.Equal("active", loaded.Status);
        Assert.Equal("basic", loaded.Tier);
        Assert.Equal(DateTime.UtcNow.Date, loaded.StartDate!.Value);
        var updatedAtBefore = loaded.UpdatedAt!.Value;

        // Undocumented status value is rejected by the new CHECK constraint.
        await using var ctx2 = _fx.CreateContext();
        var ex = await Assert.ThrowsAnyAsync<Exception>(() => ctx2.Database.ExecuteSqlRawAsync(
            "UPDATE subscriptions SET status = {0} WHERE id = {1}", "paused", subscription.Id));
        Assert.Equal(547, UnwrapSqlException(ex)!.Number);

        // Valid status write restamps updated_at via the trigger.
        await using var ctx3 = _fx.CreateContext();
        (await ctx3.Subscriptions.SingleAsync(s => s.Id == subscription.Id)).Status = "cancelled";
        await ctx3.SaveChangesAsync();

        await using var ctx4 = _fx.CreateContext();
        var after = (await ctx4.Subscriptions.AsNoTracking().SingleAsync(s => s.Id == subscription.Id))
            .UpdatedAt!.Value;
        Assert.True(after > updatedAtBefore, "subscriptions.updated_at was not restamped by the trigger");
    }

    [Fact]
    public async Task SubscriptionPlan_coach_id_is_a_user_id_not_a_coaches_id()
    {
        var (profileId, coachId) = await CreateCoachAsync(_fx.Context);

        // The RLS policies compare subscription_plans.coach_id to auth.uid(),
        // i.e. the plan owner is keyed by USER id — this must keep working.
        var plan = new SubscriptionPlan
        {
            Id = Guid.NewGuid(),
            CoachId = profileId,
            Name = "Silver Plan",
            PriceUsd = 30m,
        };
        _fx.Context.SubscriptionPlans.Add(plan);
        await _fx.Context.SaveChangesAsync();

        // A coaches.id is a different key space: it must NOT satisfy the FK
        // to profiles (unless coincidentally a profile id, which fresh GUIDs are not).
        await using var ctx2 = _fx.CreateContext();
        ctx2.SubscriptionPlans.Add(new SubscriptionPlan
        {
            Id = Guid.NewGuid(),
            CoachId = coachId,
            Name = "Invalid Owner",
        });
        var ex = await Assert.ThrowsAnyAsync<Exception>(() => ctx2.SaveChangesAsync());
        Assert.Equal(547, UnwrapSqlException(ex)!.Number);
    }

    [Fact]
    public async Task Review_links_client_profile_and_coach()
    {
        var (profileId, coachId) = await CreateCoachAsync(_fx.Context);

        var review = new Review
        {
            Id = Guid.NewGuid(),
            ClientId = profileId,
            CoachId = coachId,
            Rating = 5,
            Comment = "أفضل مدرب",
        };
        _fx.Context.Reviews.Add(review);
        await _fx.Context.SaveChangesAsync();

        await using var ctx = _fx.CreateContext();
        var loaded = await ctx.Reviews
            .Include(r => r.Coach)
            .Include(r => r.Client)
            .AsNoTracking()
            .SingleAsync(r => r.Id == review.Id);

        Assert.NotNull(loaded.Coach);
        Assert.Equal(profileId, loaded.Client!.Id);
        Assert.Equal(5, loaded.Rating);
        Assert.Equal("أفضل مدرب", loaded.Comment);
    }

    [Fact]
    public async Task PaymentIntent_and_stripe_customer_defaults()
    {
        var (profileId, coachId) = await CreateCoachAsync(_fx.Context);

        _fx.Context.StripeCustomers.Add(new StripeCustomer
        {
            Id = Guid.NewGuid(),
            UserId = profileId,
            StripeCustomerId = "cus_QWE789",
        });
        _fx.Context.PaymentIntents.Add(new PaymentIntent
        {
            Id = Guid.NewGuid(),
            ClientId = profileId,
            CoachId = coachId,
            StripePaymentId = "pi_3ABC123",
            Amount = 2999m,
        });
        await _fx.Context.SaveChangesAsync();

        await using var ctx = _fx.CreateContext();
        var payment = await ctx.PaymentIntents.AsNoTracking()
            .SingleAsync(p => p.StripePaymentId == "pi_3ABC123");
        Assert.Equal("usd", payment.Currency);
        Assert.Equal("pending", payment.Status);
        Assert.Equal("standard", payment.Tier);
        Assert.Equal(2999m, payment.Amount);

        var customer = await ctx.StripeCustomers.AsNoTracking()
            .SingleAsync(sc => sc.StripeCustomerId == "cus_QWE789");
        Assert.Equal(profileId, customer.UserId);
    }

    [Fact]
    public async Task ClientAssignment_chain_and_restrict_delete()
    {
        var (profileId, coachId) = await CreateCoachAsync(_fx.Context);

        var content = new CoachContent
        {
            Id = Guid.NewGuid(),
            CoachId = coachId,
            Title = "Nutrition 101",
            Type = "pdf",
            FileUrl = "coach-pdfs/nutrition.pdf",
        };
        var assignment = new ClientAssignment
        {
            Id = Guid.NewGuid(),
            CoachId = coachId,
            ClientId = profileId,
            ContentId = content.Id,
        };
        _fx.Context.CoachContents.Add(content);
        _fx.Context.ClientAssignments.Add(assignment);
        await _fx.Context.SaveChangesAsync();

        await using var ctx = _fx.CreateContext();
        var loaded = await ctx.ClientAssignments
            .Include(ca => ca.Content)
            .Include(ca => ca.Client)
            .AsNoTracking()
            .SingleAsync(ca => ca.Id == assignment.Id);
        Assert.Equal("Nutrition 101", loaded.Content!.Title);
        Assert.Equal(profileId, loaded.Client!.Id);

        // Deleting content that is assigned is blocked (Restrict).
        await using var ctx2 = _fx.CreateContext();
        ctx2.CoachContents.Remove(await ctx2.CoachContents.SingleAsync(cc => cc.Id == content.Id));
        await Assert.ThrowsAnyAsync<DbUpdateException>(() => ctx2.SaveChangesAsync());
    }

    private static async Task<Guid> CreateUserAsync(CoreGymDbContext ctx, string name = "g4-user")
    {
        var profile = new Profile { Id = Guid.NewGuid(), Name = name };
        ctx.Profiles.Add(profile);
        await ctx.SaveChangesAsync();
        return profile.Id;
    }

    private static async Task<(Guid ProfileId, Guid CoachId)> CreateCoachAsync(CoreGymDbContext ctx)
    {
        var profileId = await CreateUserAsync(ctx);
        var coach = new Coach { Id = Guid.NewGuid(), UserId = profileId };
        ctx.Coaches.Add(coach);
        await ctx.SaveChangesAsync();
        return (profileId, coach.Id);
    }

    private static SqlException? UnwrapSqlException(Exception exception)
    {
        for (var current = (Exception?)exception; current is not null; current = current.InnerException)
        {
            if (current is SqlException sqlException)
            {
                return sqlException;
            }
        }

        return null;
    }
}
