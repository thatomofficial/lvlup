using LvlUp.Application.Abstractions.Data;
using LvlUp.Application.Abstractions.Messaging;
using LvlUp.Domain.Hunters;
using LvlUp.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace LvlUp.Application.Hunters.UpdateDisplayNamePreference;

internal sealed class UpdateDisplayNamePreferenceCommandHandler(IApplicationDbContext context)
    : ICommandHandler<UpdateDisplayNamePreferenceCommand>
{
    public async Task<Result> HandleAsync(
        UpdateDisplayNamePreferenceCommand command,
        CancellationToken cancellationToken)
    {
        Hunter? hunter = await context.Hunters
            .SingleOrDefaultAsync(h => h.Id == command.HunterId, cancellationToken);

        if (hunter is null)
        {
            return Result.Failure(HunterErrors.NotFound(command.HunterId));
        }

        hunter.SetDisplayNamePreference(command.Preference);

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
