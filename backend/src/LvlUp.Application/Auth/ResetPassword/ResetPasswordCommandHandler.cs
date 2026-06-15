using LvlUp.Application.Abstractions.Authentication;
using LvlUp.Application.Abstractions.Data;
using LvlUp.Application.Abstractions.Messaging;
using LvlUp.Domain.Hunters;
using LvlUp.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace LvlUp.Application.Auth.ResetPassword;

internal sealed class ResetPasswordCommandHandler(
    IApplicationDbContext context,
    IPasswordHasher passwordHasher,
    TimeProvider timeProvider)
    : ICommandHandler<ResetPasswordCommand>
{
    public async Task<Result> HandleAsync(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        string email = command.Email.Trim().ToLowerInvariant();
        DateTime utcNow = timeProvider.GetUtcNow().UtcDateTime;

        Hunter? hunter = await context.Hunters
            .SingleOrDefaultAsync(h => h.Email == email, cancellationToken);

        // A generic error throughout avoids revealing whether the email exists
        // or whether a reset is in progress.
        if (hunter is null || !hunter.IsPasswordResetActive(utcNow))
        {
            return Result.Failure(HunterErrors.InvalidPasswordResetCode);
        }

        if (hunter.HasExhaustedPasswordResetAttempts())
        {
            hunter.ClearPasswordReset();
            await context.SaveChangesAsync(cancellationToken);
            return Result.Failure(HunterErrors.InvalidPasswordResetCode);
        }

        if (!passwordHasher.Verify(command.Code, hunter.PasswordResetCodeHash!))
        {
            hunter.RecordFailedPasswordResetAttempt();
            await context.SaveChangesAsync(cancellationToken);
            return Result.Failure(HunterErrors.InvalidPasswordResetCode);
        }

        hunter.CompletePasswordReset(passwordHasher.Hash(command.NewPassword));

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
