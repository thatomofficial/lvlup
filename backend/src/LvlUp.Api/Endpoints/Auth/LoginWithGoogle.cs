using LvlUp.Api.Infrastructure;
using LvlUp.Application.Abstractions.Messaging;
using LvlUp.Application.Auth;
using LvlUp.Application.Auth.LoginWithGoogle;
using LvlUp.SharedKernel;

namespace LvlUp.Api.Endpoints.Auth;

internal sealed class LoginWithGoogle : IEndpoint
{
    public sealed record Request(string IdToken);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/sso/google", async (
            Request request,
            ICommandHandler<LoginWithGoogleCommand, AuthResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new LoginWithGoogleCommand(request.IdToken);

            Result<AuthResponse> result = await handler.HandleAsync(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .AllowAnonymous()
        .WithTags(Tags.Auth);
    }
}
