namespace LvlUp.Infrastructure.Storage;

public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    /// <summary>Root folder for locally stored files (absolute, or relative to the content root).</summary>
    public string Root { get; init; } = string.Empty;
}
