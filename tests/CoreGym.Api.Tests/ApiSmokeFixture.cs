using System.Net.Http.Headers;
using System.Text;
using CoreGym.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace CoreGym.Api.Tests;

/// <summary>
/// Boots the real API (WebApplicationFactory) against a fresh scratch LocalDB
/// database with migrations applied, and issues test JWTs (HS256, 'sub' claim)
/// signed with the key the host is configured to accept.
/// </summary>
public sealed class ApiSmokeFixture : IAsyncLifetime
{
    public const string Server = "(localdb)\\MSSQLLocalDB";
    public const string Issuer = "coregym-tests";
    public const string Audience = "coregym-api";

    public string DatabaseName { get; } = $"CoreGym_Api_{Guid.NewGuid():N}";

    public string ConnectionString =>
        $"Server={Server};Database={DatabaseName};Trusted_Connection=True;TrustServerCertificate=True";

    public string SigningKey { get; } = "coregym-test-signing-key-0123456789abcdef0123456789abcdef";

    public string StripeWebhookSecret { get; } = "whsec_coregym_test_secret";

    public WebApplicationFactory<Program> Factory { get; private set; } = null!;

    public Task InitializeAsync()
    {
        ExecuteOnMaster($"CREATE DATABASE [{DatabaseName}]");

        using (var ctx = CreateContext())
        {
            ctx.Database.Migrate();
        }

        Factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:CoreGym", ConnectionString);
            builder.UseSetting("Jwt:Issuer", Issuer);
            builder.UseSetting("Jwt:Audience", Audience);
            builder.UseSetting("Jwt:SigningKey", SigningKey);
            builder.UseSetting("Database:MigrateOnStartup", "false");
            builder.UseSetting("Stripe:WebhookSecret", StripeWebhookSecret);
        });

        return Task.CompletedTask;
    }

    public HttpClient CreateClient(Guid userId)
    {
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken(userId));
        return client;
    }

    public HttpClient CreateAnonymousClient() => Factory.CreateClient();

    public string CreateToken(Guid userId) => new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
    {
        Issuer = Issuer,
        Audience = Audience,
        Expires = DateTime.UtcNow.AddHours(1),
        SigningCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey)), SecurityAlgorithms.HmacSha256),
        Claims = new Dictionary<string, object> { ["sub"] = userId.ToString() },
    });

    public CoreGymDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<CoreGymDbContext>()
            .UseSqlServer(ConnectionString)
            .Options);

    private static void ExecuteOnMaster(string sql)
    {
        using var connection = new SqlConnection(
            $"Server={Server};Database=master;Trusted_Connection=True;TrustServerCertificate=True");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    public async Task DisposeAsync()
    {
        if (Factory is not null)
        {
            await Factory.DisposeAsync();
        }

        ExecuteOnMaster(
            $"IF DB_ID(N'{DatabaseName}') IS NOT NULL ALTER DATABASE [{DatabaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; " +
            $"IF DB_ID(N'{DatabaseName}') IS NOT NULL DROP DATABASE [{DatabaseName}];");
    }
}

[CollectionDefinition("api-smoke")]
public sealed class ApiSmokeCollection : ICollectionFixture<ApiSmokeFixture>
{
}
