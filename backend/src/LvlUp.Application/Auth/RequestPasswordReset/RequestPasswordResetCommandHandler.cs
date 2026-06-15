using LvlUp.Application.Abstractions.Authentication;
using LvlUp.Application.Abstractions.Data;
using LvlUp.Application.Abstractions.Messaging;
using LvlUp.Application.Abstractions.Notifications;
using LvlUp.Domain.Hunters;
using LvlUp.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace LvlUp.Application.Auth.RequestPasswordReset;

internal sealed class RequestPasswordResetCommandHandler(
    IApplicationDbContext context,
    IOtpGenerator otpGenerator,
    IPasswordHasher passwordHasher,
    IPasswordResetNotifier notifier,
    TimeProvider timeProvider)
    : ICommandHandler<RequestPasswordResetCommand>
{
    private static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(15);

    public async Task<Result> HandleAsync(RequestPasswordResetCommand command, CancellationToken cancellationToken)
    {
        string email = command.Email.Trim().ToLowerInvariant();

        Hunter? hunter = await context.Hunters
            .SingleOrDefaultAsync(h => h.Email == email, cancellationToken);

        // Always succeed, even when the email is unknown, so the endpoint never
        // reveals which addresses have accounts.
        if (hunter is null)
        {
            return Result.Success();
        }

        string code = otpGenerator.Generate();
        DateTime expiresAtUtc = timeProvider.GetUtcNow().UtcDateTime.Add(CodeLifetime);

        hunter.RequestPasswordReset(passwordHasher.Hash(code), expiresAtUtc);

        await context.SaveChangesAsync(cancellationToken);

        await notifier.SendCodeAsync(hunter.Email, code, expiresAtUtc, cancellationToken);

        return Result.Success();
    }
}
