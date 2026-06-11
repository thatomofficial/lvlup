using LvlUp.Application.Abstractions.Authentication;
using LvlUp.Application.Abstractions.Data;
using LvlUp.Application.Abstractions.Messaging;
using LvlUp.Domain.Hunters;
using LvlUp.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace LvlUp.Application.Auth.Login;

internal sealed class LoginCommandHandler(
    IApplicationDbContext context,
    IPasswordHasher passwordHasher,
    ITokenProvider tokenProvider)
    : ICommandHandler<LoginCommand, AuthResponse>
{
    public async Task<Result<AuthResponse>> HandleAsync(LoginCommand command, CancellationToken cancellationToken)
    {
        string email = command.Email.Trim().ToLowerInvariant();

        Hunter? hunter = await context.Hunters
            .AsNoTracking()
            .SingleOrDefaultAsync(h => h.Email == email, cancellationToken);

        if (hunter is null || !passwordHasher.Verify(command.Password, hunter.PasswordHash))
        {
            return Result.Failure<AuthResponse>(HunterErrors.InvalidCredentials);
        }

        return new AuthResponse(tokenProvider.Create(hunter), hunter.Id);
    }
}
