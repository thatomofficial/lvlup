using LvlUp.Application.Abstractions.Authentication;
using LvlUp.Application.Abstractions.Data;
using LvlUp.Application.Abstractions.Integrations;
using LvlUp.Application.Abstractions.Messaging;
using LvlUp.Domain.Hunters;
using LvlUp.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace LvlUp.Application.Auth.LoginWithGoogle;

internal sealed class LoginWithGoogleCommandHandler(
    IApplicationDbContext context,
    IGoogleIdTokenVerifier googleIdTokenVerifier,
    IPasswordHasher passwordHasher,
    ITokenProvider tokenProvider,
    TimeProvider timeProvider)
    : ICommandHandler<LoginWithGoogleCommand, AuthResponse>
{
    public async Task<Result<AuthResponse>> HandleAsync(
        LoginWithGoogleCommand command,
        CancellationToken cancellationToken)
    {
        GoogleUserInfo? userInfo = await googleIdTokenVerifier.VerifyAsync(command.IdToken, cancellationToken);

        if (userInfo is null)
        {
            return Result.Failure<AuthResponse>(HunterErrors.InvalidSsoToken);
        }

        string email = userInfo.Email.Trim().ToLowerInvariant();

        Hunter? hunter = await context.Hunters
            .SingleOrDefaultAsync(h => h.Email == email, cancellationToken);

        if (hunter is null)
        {
            hunter = Hunter.Create(
                email,
                // SSO accounts get an unusable random password; they sign in via Google.
                passwordHasher.Hash(Guid.NewGuid().ToString("N")),
                string.IsNullOrWhiteSpace(userInfo.GivenName) ? "Hunter" : userInfo.GivenName.Trim(),
                userInfo.FamilyName?.Trim() ?? string.Empty,
                await GenerateUniqueUsernameAsync(email, cancellationToken),
                timeProvider.GetUtcNow().UtcDateTime);

            context.Hunters.Add(hunter);

            await context.SaveChangesAsync(cancellationToken);
        }

        return new AuthResponse(tokenProvider.Create(hunter), hunter.Id);
    }

    private async Task<string> GenerateUniqueUsernameAsync(string email, CancellationToken cancellationToken)
    {
        string localPart = email[..email.IndexOf('@', StringComparison.Ordinal)];
        string sanitized = new([.. localPart.Where(c => char.IsAsciiLetterOrDigit(c) || c == '_')]);

        string candidate = sanitized.Length >= 3 ? sanitized[..Math.Min(sanitized.Length, 30)] : "hunter";

        if (!await context.Hunters.AnyAsync(h => h.Username == candidate, cancellationToken))
        {
            return candidate;
        }

        string prefix = candidate[..Math.Min(candidate.Length, 21)];
        return $"{prefix}_{Guid.NewGuid():N}"[..Math.Min(prefix.Length + 9, 30)];
    }
}
