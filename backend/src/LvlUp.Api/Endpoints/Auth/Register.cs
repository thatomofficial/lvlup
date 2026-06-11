using LvlUp.Api.Infrastructure;
using LvlUp.Application.Abstractions.Messaging;
using LvlUp.Application.Auth;
using LvlUp.Application.Auth.Register;
using LvlUp.SharedKernel;

namespace LvlUp.Api.Endpoints.Auth;

internal sealed class Register : IEndpoint
{
    public sealed record Request(string Email, string Password, string Name, string Surname, string Username);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/register", async (
            Request request,
            ICommandHandler<RegisterCommand, AuthResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new RegisterCommand(
                request.Email,
                request.Password,
                request.Name,
                request.Surname,
                request.Username);

            Result<AuthResponse> result = await handler.HandleAsync(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .AllowAnonymous()
        .WithTags(Tags.Auth);
    }
}
