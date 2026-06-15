using LvlUp.Api.Infrastructure;
using LvlUp.Application.Abstractions.Messaging;
using LvlUp.Application.Auth.RequestPasswordReset;
using LvlUp.SharedKernel;

namespace LvlUp.Api.Endpoints.Auth;

internal sealed class RequestPasswordReset : IEndpoint
{
    public sealed record Request(string Email);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/password-reset/request", async (
            Request request,
            ICommandHandler<RequestPasswordResetCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new RequestPasswordResetCommand(request.Email);

            Result result = await handler.HandleAsync(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .AllowAnonymous()
        .WithTags(Tags.Auth);
    }
}
