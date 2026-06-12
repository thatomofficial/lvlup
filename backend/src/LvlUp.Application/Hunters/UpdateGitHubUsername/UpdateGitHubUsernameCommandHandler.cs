using LvlUp.Application.Abstractions.Data;
using LvlUp.Application.Abstractions.Messaging;
using LvlUp.Domain.Hunters;
using LvlUp.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace LvlUp.Application.Hunters.UpdateGitHubUsername;

internal sealed class UpdateGitHubUsernameCommandHandler(IApplicationDbContext context)
    : ICommandHandler<UpdateGitHubUsernameCommand>
{
    public async Task<Result> HandleAsync(UpdateGitHubUsernameCommand command, CancellationToken cancellationToken)
    {
        Hunter? hunter = await context.Hunters
            .SingleOrDefaultAsync(h => h.Id == command.HunterId, cancellationToken);

        if (hunter is null)
        {
            return Result.Failure(HunterErrors.NotFound(command.HunterId));
        }

        hunter.SetGitHubUsername(command.Username);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
