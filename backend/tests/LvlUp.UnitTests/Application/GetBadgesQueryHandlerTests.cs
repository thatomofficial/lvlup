using LvlUp.Application.Hunters.GetBadges;
using LvlUp.Domain.Hunters;
using LvlUp.SharedKernel;
using LvlUp.UnitTests.TestInfrastructure;
using NSubstitute;
using Shouldly;

namespace LvlUp.UnitTests.Application;

public sealed class GetBadgesQueryHandlerTests : IDisposable
{
    private static readonly DateTimeOffset UtcNow = new(2026, 6, 12, 8, 0, 0, TimeSpan.Zero);

    private readonly TestApplicationDbContext _context = TestApplicationDbContext.Create();
    private readonly IBadgeProgressDataGateway _gateway = Substitute.For<IBadgeProgressDataGateway>();
    private readonly GetBadgesQueryHandler _handler;

    public GetBadgesQueryHandlerTests() => _handler = new GetBadgesQueryHandler(_context, _gateway);

    [Fact]
    public async Task HandleAsync_Should_ReturnNotFound_WhenHunterDoesNotExist()
    {
        var query = new GetBadgesQuery(Guid.NewGuid());

        Result<IReadOnlyList<BadgeResponse>> result = await _handler.HandleAsync(query, CancellationToken.None);

        result.Error.Code.ShouldBe("Hunters.NotFound");
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnFourBadgesPerCategory()
    {
        Hunter hunter = await SeedHunterAsync();
        SeedCompletions(hunter.Id);

        Result<IReadOnlyList<BadgeResponse>> result = await _handler.HandleAsync(
            new GetBadgesQuery(hunter.Id),
            CancellationToken.None);

        result.Value.Count.ShouldBe(Enum.GetValues<StatCategory>().Length * 4);
    }

    [Fact]
    public async Task HandleAsync_Should_MarkTiersEarned_WhenThresholdReached()
    {
        Hunter hunter = await SeedHunterAsync();
        SeedCompletions(hunter.Id, (StatCategory.Charisma, 30));

        Result<IReadOnlyList<BadgeResponse>> result = await _handler.HandleAsync(
            new GetBadgesQuery(hunter.Id),
            CancellationToken.None);

        // 30 completions: Iron (5) and Steel (25) earned, Mythril (75) not yet.
        result.Value
            .Where(badge => badge.Category == StatCategory.Charisma && badge.IsEarned)
            .Select(badge => badge.Tier)
            .ShouldBe([BadgeTier.Iron, BadgeTier.Steel]);
    }

    [Fact]
    public async Task HandleAsync_Should_ComputeProgressTowardNextTier()
    {
        Hunter hunter = await SeedHunterAsync();
        SeedCompletions(hunter.Id, (StatCategory.Strength, 30));

        Result<IReadOnlyList<BadgeResponse>> result = await _handler.HandleAsync(
            new GetBadgesQuery(hunter.Id),
            CancellationToken.None);

        BadgeResponse mythril = result.Value.Single(badge =>
            badge.Category == StatCategory.Strength && badge.Tier == BadgeTier.Mythril);

        mythril.ProgressPercent.ShouldBe(40);
    }

    [Fact]
    public async Task HandleAsync_Should_CapProgressAtOneHundred()
    {
        Hunter hunter = await SeedHunterAsync();
        SeedCompletions(hunter.Id, (StatCategory.Strength, 9999));

        Result<IReadOnlyList<BadgeResponse>> result = await _handler.HandleAsync(
            new GetBadgesQuery(hunter.Id),
            CancellationToken.None);

        result.Value
            .Where(badge => badge.Category == StatCategory.Strength)
            .ShouldAllBe(badge => badge.ProgressPercent == 100);
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnNoEarnedBadges_WithoutCompletions()
    {
        Hunter hunter = await SeedHunterAsync();
        SeedCompletions(hunter.Id);

        Result<IReadOnlyList<BadgeResponse>> result = await _handler.HandleAsync(
            new GetBadgesQuery(hunter.Id),
            CancellationToken.None);

        result.Value.ShouldAllBe(badge => !badge.IsEarned);
    }

    public void Dispose() => _context.Dispose();

    private void SeedCompletions(Guid hunterId, params (StatCategory Category, int Count)[] counts)
    {
        IReadOnlyList<CategoryCompletionRow> rows =
        [
            .. counts.Select(entry => new CategoryCompletionRow
            {
                Category = (int)entry.Category,
                Completions = entry.Count,
            }),
        ];

        _gateway.GetCompletionCountsByCategoryAsync(hunterId, Arg.Any<CancellationToken>()).Returns(rows);
    }

    private async Task<Hunter> SeedHunterAsync()
    {
        var hunter = Hunter.Create("hunter@lvlup.app", "hash", "Jin-Woo", "Sung", "shadow_monarch", UtcNow.UtcDateTime);
        _context.Hunters.Add(hunter);
        await _context.SaveChangesAsync();
        return hunter;
    }
}
