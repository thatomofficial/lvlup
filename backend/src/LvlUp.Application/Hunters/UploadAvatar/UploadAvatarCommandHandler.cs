using LvlUp.Application.Abstractions.Data;
using LvlUp.Application.Abstractions.Messaging;
using LvlUp.Application.Abstractions.Storage;
using LvlUp.Domain.Hunters;
using LvlUp.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace LvlUp.Application.Hunters.UploadAvatar;

internal sealed class UploadAvatarCommandHandler(IApplicationDbContext context, IFileStorage fileStorage)
    : ICommandHandler<UploadAvatarCommand, UploadAvatarResponse>
{
    private const string AvatarsFolder = "avatars";

    private static readonly Dictionary<string, string> Extensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ["image/jpeg"] = ".jpg",
        ["image/png"] = ".png",
        ["image/webp"] = ".webp",
    };

    public async Task<Result<UploadAvatarResponse>> HandleAsync(
        UploadAvatarCommand command,
        CancellationToken cancellationToken)
    {
        if (!Extensions.TryGetValue(command.ContentType, out string? extension))
        {
            return Result.Failure<UploadAvatarResponse>(HunterErrors.UnsupportedAvatarType);
        }

        Hunter? hunter = await context.Hunters
            .SingleOrDefaultAsync(h => h.Id == command.HunterId, cancellationToken);

        if (hunter is null)
        {
            return Result.Failure<UploadAvatarResponse>(HunterErrors.NotFound(command.HunterId));
        }

        string? previousPath = hunter.AvatarPath;

        // A fresh file name per upload busts client-side image caches.
        string fileName = $"{hunter.Id:N}-{Guid.NewGuid():N}{extension}";
        string storedPath = await fileStorage.SaveAsync(AvatarsFolder, fileName, command.Content, cancellationToken);

        hunter.SetAvatarPath(storedPath);

        await context.SaveChangesAsync(cancellationToken);

        if (previousPath is not null)
        {
            await fileStorage.DeleteAsync(previousPath, cancellationToken);
        }

        return new UploadAvatarResponse($"{StoragePaths.PublicRequestPath}/{storedPath}");
    }
}
