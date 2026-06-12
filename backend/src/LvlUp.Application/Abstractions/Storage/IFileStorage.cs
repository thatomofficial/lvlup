namespace LvlUp.Application.Abstractions.Storage;

/// <summary>
/// Stores binary files outside the database (local disk, S3, Azure Blob, ...).
/// Only the returned relative path is persisted in the database.
/// </summary>
public interface IFileStorage
{
    /// <summary>Saves the content and returns the storage-relative path (e.g. "avatars/abc.png").</summary>
    Task<string> SaveAsync(
        string folder,
        string fileName,
        ReadOnlyMemory<byte> content,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default);
}
