using CoreGym.Domain.Services;
using Microsoft.Extensions.Options;

namespace CoreGym.Infrastructure.Storage;

/// <summary>
/// Local-disk implementation of IFileStorage — the default while the hosting
/// decision is open; swap for Azure Blob/S3 implementations behind the same
/// interface without touching callers. Files live under {root}/{bucket}/{name}.
/// </summary>
public class LocalFileStorage : IFileStorage
{
    private readonly string _root;

    public LocalFileStorage(IOptions<StorageOptions> options)
    {
        var configured = string.IsNullOrWhiteSpace(options.Value.LocalRoot)
            ? "file-storage"
            : options.Value.LocalRoot;
        _root = Path.GetFullPath(configured);
    }

    public async Task<string> SaveAsync(string bucket, string fileName, Stream content, CancellationToken cancellationToken = default)
    {
        var fullPath = Resolve(bucket, fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        await using var file = File.Create(fullPath);
        await content.CopyToAsync(file, cancellationToken);
        return $"{SanitizeSegment(bucket)}/{SanitizeSegment(fileName).Replace('\\', '/')}";
    }

    public Task<Stream?> OpenAsync(string bucket, string fileName, CancellationToken cancellationToken = default)
    {
        var fullPath = Resolve(bucket, fileName);
        return Task.FromResult<Stream?>(File.Exists(fullPath) ? File.OpenRead(fullPath) : null);
    }

    public Task DeleteAsync(string bucket, string fileName, CancellationToken cancellationToken = default)
    {
        var fullPath = Resolve(bucket, fileName);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    private string Resolve(string bucket, string fileName)
    {
        var fullPath = Path.GetFullPath(Path.Combine(_root, SanitizeSegment(bucket), SanitizeSegment(fileName)));
        if (!fullPath.StartsWith(_root, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Path escapes the storage root.");
        }

        return fullPath;
    }

    private static string SanitizeSegment(string segment)
    {
        if (string.IsNullOrWhiteSpace(segment))
        {
            throw new ArgumentException("Storage path segments must not be empty.");
        }

        foreach (var part in segment.Split('/', '\\'))
        {
            if (part is ".." or "." || part.Contains(':'))
            {
                throw new ArgumentException("Storage paths must not contain traversal segments.");
            }
        }

        foreach (var invalid in Path.GetInvalidFileNameChars())
        {
            segment = segment.Replace(invalid, '_');
        }

        return segment;
    }
}
