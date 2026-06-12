using LvlUp.SharedKernel;

namespace LvlUp.Domain.Quests;

public static class QuestErrors
{
    public static Error NotFound(Guid questId) => Error.NotFound(
        "Quests.NotFound",
        $"The quest with the identifier '{questId}' was not found.");

    public static Error AlreadyCompleted(Guid questId) => Error.Conflict(
        "Quests.AlreadyCompleted",
        $"The quest with the identifier '{questId}' has already been completed.");

    public static Error VerificationFailed(string gitHubUsername) => Error.Problem(
        "Quests.VerificationFailed",
        $"No GitHub push was found today for '{gitHubUsername}'. Push your work, then complete the quest.");
}
