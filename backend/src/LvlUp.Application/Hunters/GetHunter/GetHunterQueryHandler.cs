using LvlUp.Application.Abstractions.Data;
using LvlUp.Application.Abstractions.Messaging;
using LvlUp.Domain.Hunters;
using LvlUp.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace LvlUp.Application.Hunters.GetHunter;

internal sealed class GetHunterQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetHunterQuery, HunterResponse>
{
    public async Task<Result<HunterResponse>> HandleAsync(GetHunterQuery query, CancellationToken cancellationToken)
    {
        Hunter? hunter = await context.Hunters
            .AsNoTracking()
            .SingleOrDefaultAsync(h => h.Id == query.HunterId, cancellationToken);

        if (hunter is null)
        {
            return Result.Failure<HunterResponse>(HunterErrors.NotFound(query.HunterId));
        }

        return new HunterResponse
        {
            Id = hunter.Id,
            Name = hunter.Name,
            Surname = hunter.Surname,
            Username = hunter.Username,
            DisplayName = hunter.DisplayName,
            DisplayNamePreference = hunter.DisplayNamePreference,
            Email = hunter.Email,
            Level = hunter.Level,
            CurrentXp = hunter.CurrentXp,
            XpForNextLevel = hunter.XpForNextLevel,
            TotalXp = hunter.TotalXp,
            Rank = hunter.Rank.ToString(),
            Stats = new HunterStatsResponse(
                hunter.Strength,
                hunter.Stamina,
                hunter.Physique,
                hunter.Looks,
                hunter.WellBeing,
                hunter.Intelligence,
                hunter.Charisma),
        };
    }
}
