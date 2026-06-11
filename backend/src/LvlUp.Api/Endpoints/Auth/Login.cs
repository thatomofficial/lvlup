using LvlUp.Api.Infrastructure;
using LvlUp.Application.Abstractions.Messaging;
using LvlUp.Application.Auth;
using LvlUp.Application.Auth.Login;
using LvlUp.SharedKernel;

namespace LvlUp.Api.Endpoints.Auth;

internal sealed class Login : IEndpoint
{
    public sealed record Request(string Email, string Password);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/login", async (
            Request request,
            ICommandHandler<LoginCommand, AuthResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new LoginCommand(request.Email, request.Password);

            Result<AuthResponse> result = await handler.HandleAsync(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .AllowAnonymous()
        .WithTags(Tags.Auth);
    }
}
