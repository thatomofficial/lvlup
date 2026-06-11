namespace LvlUp.Application.Hunters.GetHunter;

public sealed record HunterStatsResponse(int Strength, int Stamina, int Physique, int Looks, int WellBeing);

public sealed record HunterResponse
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required string Email { get; init; }

    public required int Level { get; init; }

    public required int CurrentXp { get; init; }

    public required int XpForNextLevel { get; init; }

    public required int TotalXp { get; init; }

    public required string Rank { get; init; }

    public required HunterStatsResponse Stats { get; init; }
}
