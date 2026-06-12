using LvlUp.Application.Abstractions.Storage;
using Microsoft.Extensions.Options;

namespace LvlUp.Infrastructure.Storage;

/// <summary>
/// Stores files on the local file system under the configured root, which the
/// API serves at <see cref="StoragePaths.PublicRequestPath"/>. Swap this
/// implementation for an S3/Azure Blob adapter to move to cloud storage -
/// the database only ever holds the relative path.
/// </summary>
internal sealed class LocalFileStorage(IOptions<StorageOptions> options) : IFileStorage
{
    public async Task<string> SaveAsync(
        string folder,
        string fileName,
        ReadOnlyMemory<byte> content,
        CancellationToken cancellationToken = default)
    {
        string root = Path.GetFullPath(options.Value.Root);
        string directory = Path.Combine(root, folder);
        Directory.CreateDirectory(directory);

        string fullPath = Path.Combine(directory, fileName);

        await using FileStream stream = File.Create(fullPath);
        await stream.WriteAsync(content, cancellationToken);

        return $"{folder}/{fileName}";
    }

    public Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        string root = Path.GetFullPath(options.Value.Root);
        string fullPath = Path.GetFullPath(Path.Combine(root, relativePath));

        // Guard against path traversal: never delete outside the storage root.
        if (fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase) && File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }
}
