using LvlUp.Application.Abstractions.Data;
using LvlUp.Application.Abstractions.Integrations;
using LvlUp.Application.Abstractions.Messaging;
using LvlUp.Domain.Hunters;
using LvlUp.Domain.Quests;
using LvlUp.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace LvlUp.Application.Quests.CompleteQuest;

internal sealed class CompleteQuestCommandHandler(
    IApplicationDbContext context,
    TimeProvider timeProvider,
    IGitHubActivityVerifier gitHubActivityVerifier)
    : ICommandHandler<CompleteQuestCommand, CompleteQuestResponse>
{
    public async Task<Result<CompleteQuestResponse>> HandleAsync(
        CompleteQuestCommand command,
        CancellationToken cancellationToken)
    {
        Quest? quest = await context.Quests
            .SingleOrDefaultAsync(
                q => q.Id == command.QuestId && q.HunterId == command.HunterId,
                cancellationToken);

        if (quest is null)
        {
            return Result.Failure<CompleteQuestResponse>(QuestErrors.NotFound(command.QuestId));
        }

        Hunter? hunter = await context.Hunters
            .SingleOrDefaultAsync(h => h.Id == command.HunterId, cancellationToken);

        if (hunter is null)
        {
            return Result.Failure<CompleteQuestResponse>(HunterErrors.NotFound(command.HunterId));
        }

        DateTime utcNow = timeProvider.GetUtcNow().UtcDateTime;

        if (quest.Verification == QuestVerification.GitHubPush)
        {
            if (string.IsNullOrWhiteSpace(hunter.GitHubUsername))
            {
                return Result.Failure<CompleteQuestResponse>(HunterErrors.GitHubUsernameNotConfigured);
            }

            bool hasPushed = await gitHubActivityVerifier.HasPushedOnDateAsync(
                hunter.GitHubUsername,
                DateOnly.FromDateTime(utcNow),
                cancellationToken);

            if (!hasPushed)
            {
                return Result.Failure<CompleteQuestResponse>(QuestErrors.VerificationFailed(hunter.GitHubUsername));
            }
        }

        Result completeResult = quest.Complete(utcNow);
        if (completeResult.IsFailure)
        {
            return Result.Failure<CompleteQuestResponse>(completeResult.Error);
        }

        bool leveledUp = hunter.GainXp(quest.XpReward);
        hunter.IncreaseStat(quest.Category, quest.StatReward);

        context.QuestCompletions.Add(QuestCompletion.Create(quest, utcNow, command.Note));

        await context.SaveChangesAsync(cancellationToken);

        return new CompleteQuestResponse
        {
            XpGained = quest.XpReward,
            StatCategory = quest.Category,
            StatGained = quest.StatReward,
            LeveledUp = leveledUp,
            NewLevel = hunter.Level,
            CurrentXp = hunter.CurrentXp,
            XpForNextLevel = hunter.XpForNextLevel,
        };
    }
}
