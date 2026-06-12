using LvlUp.Application.Abstractions.Data;
using LvlUp.Application.Abstractions.Messaging;
using LvlUp.Application.Hunters.GetHunter;
using LvlUp.Domain.Hunters;
using LvlUp.Domain.Quests;
using LvlUp.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace LvlUp.Application.Hunters.SubmitAssessment;

internal sealed class SubmitAssessmentCommandHandler(IApplicationDbContext context, TimeProvider timeProvider)
    : ICommandHandler<SubmitAssessmentCommand, AssessmentResponse>
{
    public async Task<Result<AssessmentResponse>> HandleAsync(
        SubmitAssessmentCommand command,
        CancellationToken cancellationToken)
    {
        Hunter? hunter = await context.Hunters
            .SingleOrDefaultAsync(h => h.Id == command.HunterId, cancellationToken);

        if (hunter is null)
        {
            return Result.Failure<AssessmentResponse>(HunterErrors.NotFound(command.HunterId));
        }

        Result assessmentResult = hunter.ApplyAssessment(command.Scores, timeProvider.GetUtcNow().UtcDateTime);
        if (assessmentResult.IsFailure)
        {
            return Result.Failure<AssessmentResponse>(assessmentResult.Error);
        }

        await context.SaveChangesAsync(cancellationToken);

        return new AssessmentResponse
        {
            Stats = new HunterStatsResponse(
                hunter.Stats.Strength,
                hunter.Stats.Stamina,
                hunter.Stats.Physique,
                hunter.Stats.Looks,
                hunter.Stats.WellBeing,
                hunter.Stats.Intelligence,
                hunter.Stats.Charisma),
            RecommendedDifficulties = command.Scores.ToDictionary(
                score => score.Key,
                score => RecommendDifficulty(score.Value)),
        };
    }

    private static QuestDifficulty RecommendDifficulty(int score) => score switch
    {
        <= 2 => QuestDifficulty.Easy,
        3 => QuestDifficulty.Medium,
        _ => QuestDifficulty.Hard,
    };
}
