using LvlUp.Api.Extensions;
using LvlUp.Api.Infrastructure;
using LvlUp.Application.Abstractions.Authentication;
using LvlUp.Application.Abstractions.Messaging;
using LvlUp.Application.Hunters.GetConsistency;
using LvlUp.SharedKernel;

namespace LvlUp.Api.Endpoints.Hunters;

internal sealed class GetConsistency : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("hunters/me/consistency", async (
            IUserContext userContext,
            IQueryHandler<GetConsistencyQuery, ConsistencyResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetConsistencyQuery(userContext.UserId);

            Result<ConsistencyResponse> result = await handler.HandleAsync(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .HasPermission(Permissions.HuntersRead)
        .WithTags(Tags.Hunters);
    }
}
