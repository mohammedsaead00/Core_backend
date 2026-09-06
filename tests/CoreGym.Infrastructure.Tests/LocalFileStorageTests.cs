using CoreGym.Domain.Services;
using CoreGym.Infrastructure.Storage;
using Microsoft.Extensions.Options;

namespace CoreGym.Infrastructure.Tests;

public class LocalFileStorageTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"coregym-storage-{Guid.NewGuid():N}");

    private LocalFileStorage CreateStorage() =>
        new(Options.Create(new StorageOptions { LocalRoot = _root }));

    [Fact]
    public async Task Save_open_delete_roundtrip()
    {
        var storage = CreateStorage();
        var bytes = new byte[] { 1, 2, 3, 4 };

        var path = await storage.SaveAsync("food-scans", "abc123.jpg", new MemoryStream(bytes));

        Assert.Equal("food-scans/abc123.jpg", path);
        Assert.True(File.Exists(Path.Combine(_root, "food-scans", "abc123.jpg")));

        await using (var stream = await storage.OpenAsync("food-scans", "abc123.jpg"))
        {
            Assert.NotNull(stream);
            using var memory = new MemoryStream();
            await stream!.CopyToAsync(memory);
            Assert.Equal(bytes, memory.ToArray());
        } // dispose before deleting — Windows keeps deleted-but-open files locked

        await storage.DeleteAsync("food-scans", "abc123.jpg");
        Assert.Null(await storage.OpenAsync("food-scans", "abc123.jpg"));
    }

    [Fact]
    public async Task Missing_file_opens_null()
    {
        var storage = CreateStorage();
        Assert.Null(await storage.OpenAsync("avatars", "does-not-exist.png"));
    }

    [Theory]
    [InlineData("food-scans", "../../evil.jpg")]
    [InlineData("../etc", "passwd")]
    [InlineData("food-scans", "C:\\evil.jpg")]
    public async Task Path_traversal_is_rejected(string bucket, string fileName)
    {
        var storage = CreateStorage();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            storage.SaveAsync(bucket, fileName, new MemoryStream([1]), default));
    }

    [Fact]
    public async Task Empty_segments_are_rejected()
    {
        var storage = CreateStorage();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            storage.SaveAsync("avatars", "  ", new MemoryStream([1]), default));
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }
}
