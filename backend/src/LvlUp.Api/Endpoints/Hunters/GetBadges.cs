using LvlUp.Api.Extensions;
using LvlUp.Api.Infrastructure;
using LvlUp.Application.Abstractions.Authentication;
using LvlUp.Application.Abstractions.Messaging;
using LvlUp.Application.Hunters.GetBadges;
using LvlUp.SharedKernel;

namespace LvlUp.Api.Endpoints.Hunters;

internal sealed class GetBadges : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("hunters/me/badges", async (
            IUserContext userContext,
            IQueryHandler<GetBadgesQuery, IReadOnlyList<BadgeResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetBadgesQuery(userContext.UserId);

            Result<IReadOnlyList<BadgeResponse>> result = await handler.HandleAsync(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .HasPermission(Permissions.HuntersRead)
        .WithTags(Tags.Hunters);
    }
}
