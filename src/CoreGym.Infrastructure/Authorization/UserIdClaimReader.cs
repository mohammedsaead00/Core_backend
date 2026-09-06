using System.Security.Claims;

namespace CoreGym.Infrastructure.Authorization;

/// <summary>
/// Resolves the auth user id from a ClaimsPrincipal. Reads both ASP.NET's
/// mapped ClaimTypes.NameIdentifier and the raw 'sub' claim so it works
/// regardless of the JWT handler's inbound claim mapping (Supabase JWTs carry
/// the user id in 'sub'; the final IdP decision is still open).
/// </summary>
public static class UserIdClaimReader
{
    public static Guid? GetUserId(ClaimsPrincipal? principal)
    {
        var value =
            principal?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal?.FindFirstValue("sub");

        return Guid.TryParse(value, out var id) ? id : null;
    }
}
