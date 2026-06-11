using LvlUp.Application.Abstractions.Authentication;
using LvlUp.Application.Abstractions.Data;
using LvlUp.Application.Abstractions.Messaging;
using LvlUp.Domain.Hunters;
using LvlUp.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace LvlUp.Application.Auth.Register;

internal sealed class RegisterCommandHandler(
    IApplicationDbContext context,
    IPasswordHasher passwordHasher,
    ITokenProvider tokenProvider,
    TimeProvider timeProvider)
    : ICommandHandler<RegisterCommand, AuthResponse>
{
    public async Task<Result<AuthResponse>> HandleAsync(RegisterCommand command, CancellationToken cancellationToken)
    {
        string email = command.Email.Trim().ToLowerInvariant();
        string username = command.Username.Trim().ToLowerInvariant();

        if (await context.Hunters.AnyAsync(hunter => hunter.Email == email, cancellationToken))
        {
            return Result.Failure<AuthResponse>(HunterErrors.EmailNotUnique);
        }

        if (await context.Hunters.AnyAsync(hunter => hunter.Username == username, cancellationToken))
        {
            return Result.Failure<AuthResponse>(HunterErrors.UsernameNotUnique);
        }

        var hunter = Hunter.Create(
            email,
            passwordHasher.Hash(command.Password),
            command.Name.Trim(),
            command.Surname.Trim(),
            username,
            timeProvider.GetUtcNow().UtcDateTime);

        context.Hunters.Add(hunter);

        await context.SaveChangesAsync(cancellationToken);

        return new AuthResponse(tokenProvider.Create(hunter), hunter.Id);
    }
}
