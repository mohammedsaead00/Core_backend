using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CoreGym.Infrastructure;

/// <summary>
/// Design-time only (dotnet-ef). The connection string is a placeholder —
/// migrations are applied at runtime/test time with a real connection string.
/// </summary>
public class CoreGymDbContextFactory : IDesignTimeDbContextFactory<CoreGymDbContext>
{
    public CoreGymDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<CoreGymDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=CoreGym_DesignTime;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;

        return new CoreGymDbContext(options);
    }
}
