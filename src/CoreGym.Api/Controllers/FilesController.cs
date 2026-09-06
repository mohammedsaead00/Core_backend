using CoreGym.Domain.Authorization;
using CoreGym.Domain.Services;
using CoreGym.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreGym.Api.Controllers;

[ApiController]
[Route("api/files")]
public class FilesController : CoreGymControllerBase
{
    /// <summary>Buckets that were public in the original Supabase project.</summary>
    private static readonly HashSet<string> PublicBuckets = new(StringComparer.OrdinalIgnoreCase)
    {
        "avatars", "coach-media", "coach-pdfs",
    };

    /// <summary>All buckets available for upload (authenticated).</summary>
    private static readonly HashSet<string> KnownBuckets = new(StringComparer.OrdinalIgnoreCase)
    {
        "avatars", "coach-media", "coach-pdfs",
        "food-scans", "voice-food-logs", "chat-voice-notes", "chat-images", "chat-files",
    };

    private readonly IFileStorage _storage;

    public FilesController(IFileStorage storage, ICurrentUserService currentUser) : base(currentUser)
    {
        _storage = storage;
    }

    /// <summary>
    /// Downloads a stored file. Public buckets are anonymous; private buckets
    /// require authentication (per-owner ACL is a future decision — see
    /// docs/REVIEW_CHECKLIST.md).
    /// </summary>
    [HttpGet("{bucket}/{**path}")]
    public async Task<IActionResult> Download(string bucket, string path, CancellationToken cancellationToken)
    {
        if (!PublicBuckets.Contains(bucket))
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return Unauthorized();
            }
        }

        var stream = await _storage.OpenAsync(bucket, path, cancellationToken);
        if (stream is null)
        {
            return NotFound();
        }

        return File(stream, ContentTypeFor(path));
    }

    /// <summary>Authenticated upload into any known bucket; returns the stored path.</summary>
    [HttpPost("{bucket}")]
    [Authorize]
    public async Task<IActionResult> Upload(string bucket, IFormFile file, CancellationToken cancellationToken)
    {
        if (!KnownBuckets.Contains(bucket))
        {
            return NotFound();
        }

        if (file is null || file.Length == 0)
        {
            throw new ArgumentException("A non-empty file is required.");
        }

        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        await using var stream = file.OpenReadStream();
        var storedPath = await _storage.SaveAsync(bucket, fileName, stream, cancellationToken);
        return Created($"api/files/{bucket}/{fileName}", new { path = storedPath });
    }

    private static string ContentTypeFor(string path) => Path.GetExtension(path).ToLowerInvariant() switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".webp" => "image/webp",
        ".pdf" => "application/pdf",
        ".m4a" => "audio/mp4",
        ".mp3" => "audio/mpeg",
        ".wav" => "audio/wav",
        ".mov" => "video/quicktime",
        ".mp4" => "video/mp4",
        _ => "application/octet-stream",
    };
}
