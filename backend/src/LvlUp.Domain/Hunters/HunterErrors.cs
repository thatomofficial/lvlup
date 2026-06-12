using LvlUp.SharedKernel;

namespace LvlUp.Domain.Hunters;

public static class HunterErrors
{
    public static readonly Error EmailNotUnique = Error.Conflict(
        "Hunters.EmailNotUnique",
        "A hunter with the provided email already exists.");

    public static readonly Error UsernameNotUnique = Error.Conflict(
        "Hunters.UsernameNotUnique",
        "A hunter with the provided username already exists.");

    public static readonly Error AlreadyAssessed = Error.Conflict(
        "Hunters.AlreadyAssessed",
        "The awakening assessment has already been completed.");

    public static readonly Error AssessmentUnavailable = Error.Conflict(
        "Hunters.AssessmentUnavailable",
        "The awakening assessment is only available before any XP has been earned.");

    public static readonly Error GitHubUsernameNotConfigured = Error.Problem(
        "Hunters.GitHubUsernameNotConfigured",
        "A GitHub username must be configured before completing GitHub-verified quests.");

    public static readonly Error InvalidCredentials = Error.Unauthorized(
        "Hunters.InvalidCredentials",
        "The provided credentials are invalid.");

    public static Error NotFound(Guid hunterId) => Error.NotFound(
        "Hunters.NotFound",
        $"The hunter with the identifier '{hunterId}' was not found.");
}
