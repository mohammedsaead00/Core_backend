namespace CoreGym.Infrastructure.Storage;

public class StorageOptions
{
    public const string SectionName = "Storage";

    /// <summary>Root folder for the local file storage implementation.</summary>
    public string LocalRoot { get; set; } = "file-storage";
}
