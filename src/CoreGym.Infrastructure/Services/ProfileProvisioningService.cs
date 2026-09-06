using CoreGym.Domain.Entities;
using CoreGym.Domain.Enums;
using CoreGym.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace CoreGym.Infrastructure.Services;

public class ProfileProvisioningService : IProfileProvisioningService
{
    private readonly CoreGymDbContext _db;

    public ProfileProvisioningService(CoreGymDbContext db)
    {
        _db = db;
    }

    public async Task<Profile> ProvisionAsync(Guid userId, string? email = null, string? name = null, CancellationToken cancellationToken = default)
    {
        var existing = await _db.Profiles.FirstOrDefaultAsync(p => p.Id == userId, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var profile = new Profile
        {
            Id = userId,
            Email = email,
            Name = name,
            Role = UserRole.Client,
        };
        _db.Profiles.Add(profile);
        await _db.SaveChangesAsync(cancellationToken);
        return profile;
    }
}
