using LvlUp.Api.Extensions;
using LvlUp.Api.Infrastructure;
using LvlUp.Application.Abstractions.Authentication;
using LvlUp.Application.Abstractions.Messaging;
using LvlUp.Application.Hunters.UploadAvatar;
using LvlUp.SharedKernel;

namespace LvlUp.Api.Endpoints.Hunters;

internal sealed class UploadAvatar : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("hunters/me/avatar", async (
            IFormFile file,
            IUserContext userContext,
            ICommandHandler<UploadAvatarCommand, UploadAvatarResponse> handler,
            CancellationToken cancellationToken) =>
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream, cancellationToken);

            var command = new UploadAvatarCommand(userContext.UserId, memoryStream.ToArray(), file.ContentType);

            Result<UploadAvatarResponse> result = await handler.HandleAsync(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .HasPermission(Permissions.HuntersRead)
        .WithTags(Tags.Hunters)
        .DisableAntiforgery();
    }
}
