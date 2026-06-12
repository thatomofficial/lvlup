using LvlUp.Application.Abstractions.Messaging;

namespace LvlUp.Application.Hunters.UploadAvatar;

public sealed record UploadAvatarCommand(
    Guid HunterId,
    ReadOnlyMemory<byte> Content,
    string ContentType) : ICommand<UploadAvatarResponse>;

public sealed record UploadAvatarResponse(string AvatarUrl);
