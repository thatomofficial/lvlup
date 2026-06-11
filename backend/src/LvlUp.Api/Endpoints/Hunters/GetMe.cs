using LvlUp.Api.Extensions;
using LvlUp.Api.Infrastructure;
using LvlUp.Application.Abstractions.Authentication;
using LvlUp.Application.Abstractions.Messaging;
using LvlUp.Application.Hunters.GetHunter;
using LvlUp.SharedKernel;

namespace LvlUp.Api.Endpoints.Hunters;

internal sealed class GetMe : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("hunters/me", async (
            IUserContext userContext,
            IQueryHandler<GetHunterQuery, HunterResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetHunterQuery(userContext.UserId);

            Result<HunterResponse> result = await handler.HandleAsync(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .HasPermission(Permissions.HuntersRead)
        .WithTags(Tags.Hunters);
    }
}
