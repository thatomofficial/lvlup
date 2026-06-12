using LvlUp.Api.Extensions;
using LvlUp.Api.Infrastructure;
using LvlUp.Application.Abstractions.Authentication;
using LvlUp.Application.Abstractions.Messaging;
using LvlUp.Application.Hunters.SubmitAssessment;
using LvlUp.Domain.Hunters;
using LvlUp.SharedKernel;

namespace LvlUp.Api.Endpoints.Hunters;

internal sealed class SubmitAssessment : IEndpoint
{
    public sealed record Request(Dictionary<StatCategory, int> Scores);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("hunters/me/assessment", async (
            Request request,
            IUserContext userContext,
            ICommandHandler<SubmitAssessmentCommand, AssessmentResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new SubmitAssessmentCommand(userContext.UserId, request.Scores);

            Result<AssessmentResponse> result = await handler.HandleAsync(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .HasPermission(Permissions.HuntersRead)
        .WithTags(Tags.Hunters);
    }
}
