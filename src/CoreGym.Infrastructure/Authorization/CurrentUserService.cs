using CoreGym.Domain.Authorization;
using Microsoft.AspNetCore.Http;

namespace CoreGym.Infrastructure.Authorization;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId => UserIdClaimReader.GetUserId(_httpContextAccessor.HttpContext?.User);
}
