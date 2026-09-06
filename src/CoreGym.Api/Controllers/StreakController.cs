using CoreGym.Api.Contracts;
using CoreGym.Domain.Authorization;
using CoreGym.Domain.ReadModels;
using CoreGym.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreGym.Api.Controllers;

[ApiController]
[Route("api/me/streak")]
[Authorize]
public class StreakController : CoreGymControllerBase
{
    private readonly IStreakService _streaks;

    public StreakController(IStreakService streaks, ICurrentUserService currentUser) : base(currentUser)
    {
        _streaks = streaks;
    }

    /// <summary>get_streak_status replacement; zeroed for users without a streak row yet.</summary>
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var status = await _streaks.GetStreakStatusAsync(UserId, cancellationToken);
        return Ok(status ?? new StreakStatus { UserId = UserId, FreezeAvailable = 1 });
    }

    /// <summary>record_daily_activity replacement: 'workout' or 'nutrition'.</summary>
    [HttpPost("activity")]
    public async Task<IActionResult> RecordActivity([FromBody] StreakActivityRequest request, CancellationToken cancellationToken)
    {
        await _streaks.RecordDailyActivityAsync(UserId, request.Source, cancellationToken: cancellationToken);
        var status = await _streaks.GetStreakStatusAsync(UserId, cancellationToken);
        return Ok(status);
    }
}
