namespace CoreGym.Domain.Services;

/// <summary>
/// File storage replacing the Supabase storage buckets. Bucket names mirror the
/// original: avatars, coach-media, coach-pdfs (public);
/// food-scans, voice-food-logs, chat-voice-notes, chat-images, chat-files (private).
/// </summary>
public interface IFileStorage
{
    /// <summary>Stores the content and returns the path in "{bucket}/{fileName}" form.</summary>
    Task<string> SaveAsync(string bucket, string fileName, Stream content, CancellationToken cancellationToken = default);

    /// <summary>Opens the stored file, or null when it does not exist.</summary>
    Task<Stream?> OpenAsync(string bucket, string fileName, CancellationToken cancellationToken = default);

    Task DeleteAsync(string bucket, string fileName, CancellationToken cancellationToken = default);
}
