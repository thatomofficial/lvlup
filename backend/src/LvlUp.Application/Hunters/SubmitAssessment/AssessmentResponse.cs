using LvlUp.Application.Hunters.GetHunter;
using LvlUp.Domain.Hunters;
using LvlUp.Domain.Quests;

namespace LvlUp.Application.Hunters.SubmitAssessment;

public sealed record AssessmentResponse
{
    public required HunterStatsResponse Stats { get; init; }

    public required IReadOnlyDictionary<StatCategory, QuestDifficulty> RecommendedDifficulties { get; init; }
}
