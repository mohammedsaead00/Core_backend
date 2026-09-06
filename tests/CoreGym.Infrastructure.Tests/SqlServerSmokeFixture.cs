using CoreGym.Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CoreGym.Infrastructure.Tests;

/// <summary>
/// Applies the migrations to a fresh, uniquely-named scratch database on the
/// local SQL Server LocalDB instance and drops it afterwards.
/// </summary>
public sealed class SqlServerSmokeFixture : IAsyncLifetime
{
    public const string Server = "(localdb)\\MSSQLLocalDB";

    public string DatabaseName { get; } = $"CoreGym_Smoke_{Guid.NewGuid():N}";

    public string ConnectionString =>
        $"Server={Server};Database={DatabaseName};Trusted_Connection=True;TrustServerCertificate=True";

    public CoreGymDbContext Context { get; private set; } = null!;

    public CoreGymDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<CoreGymDbContext>()
            .UseSqlServer(ConnectionString)
            .Options);

    public async Task InitializeAsync()
    {
        await ExecuteOnMasterAsync($"CREATE DATABASE [{DatabaseName}]");

        Context = CreateContext();
        await Context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        if (Context is not null)
        {
            await Context.DisposeAsync();
        }

        await ExecuteOnMasterAsync(
            $"IF DB_ID(N'{DatabaseName}') IS NOT NULL ALTER DATABASE [{DatabaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; " +
            $"IF DB_ID(N'{DatabaseName}') IS NOT NULL DROP DATABASE [{DatabaseName}];");
    }

    private static async Task ExecuteOnMasterAsync(string sql)
    {
        await using var connection = new SqlConnection(
            $"Server={Server};Database=master;Trusted_Connection=True;TrustServerCertificate=True");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync();
    }
}

[CollectionDefinition("sql-smoke")]
public sealed class SqlSmokeCollection : ICollectionFixture<SqlServerSmokeFixture>
{
}
