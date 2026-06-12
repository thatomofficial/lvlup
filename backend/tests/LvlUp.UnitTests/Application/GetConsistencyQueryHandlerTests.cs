using LvlUp.Application.Hunters.GetConsistency;
using LvlUp.Domain.Hunters;
using LvlUp.SharedKernel;
using LvlUp.UnitTests.TestInfrastructure;
using NSubstitute;
using Shouldly;

namespace LvlUp.UnitTests.Application;

public sealed class GetConsistencyQueryHandlerTests : IDisposable
{
    private static readonly DateTimeOffset UtcNow = new(2026, 6, 11, 8, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Today = DateOnly.FromDateTime(UtcNow.UtcDateTime);

    private readonly TestApplicationDbContext _context = TestApplicationDbContext.Create();
    private readonly IConsistencyDataGateway _gateway = Substitute.For<IConsistencyDataGateway>();
    private readonly GetConsistencyQueryHandler _handler;

    public GetConsistencyQueryHandlerTests() =>
        _handler = new GetConsistencyQueryHandler(_context, _gateway, new FixedTimeProvider(UtcNow));

    [Fact]
    public async Task HandleAsync_Should_ReturnNotFound_WhenHunterDoesNotExist()
    {
        var query = new GetConsistencyQuery(Guid.NewGuid());

        Result<ConsistencyResponse> result = await _handler.HandleAsync(query, CancellationToken.None);

        result.Error.Code.ShouldBe("Hunters.NotFound");
    }

    [Fact]
    public async Task HandleAsync_Should_ComputeCurrentStreak_FromGatewayRows()
    {
        Hunter hunter = await SeedHunterAsync();
        SeedActiveDays(hunter.Id, daysBack: [2, 1, 0]);

        Result<ConsistencyResponse> result = await _handler.HandleAsync(
            new GetConsistencyQuery(hunter.Id),
            CancellationToken.None);

        result.Value.CurrentStreak.ShouldBe(3);
    }

    [Fact]
    public async Task HandleAsync_Should_EarnShield_AfterSevenActiveDays()
    {
        Hunter hunter = await SeedHunterAsync();
        SeedActiveDays(hunter.Id, daysBack: [6, 5, 4, 3, 2, 1, 0]);

        Result<ConsistencyResponse> result = await _handler.HandleAsync(
            new GetConsistencyQuery(hunter.Id),
            CancellationToken.None);

        result.Value.Shields.ShouldBe(1);
    }

    [Fact]
    public async Task HandleAsync_Should_MapCompletionCountsOntoHeatmapDays()
    {
        Hunter hunter = await SeedHunterAsync();
        _gateway.GetDailyCompletionCountsAsync(hunter.Id, Arg.Any<CancellationToken>())
            .Returns([new DailyCompletionRow { Day = Today, Completions = 4 }]);

        Result<ConsistencyResponse> result = await _handler.HandleAsync(
            new GetConsistencyQuery(hunter.Id),
            CancellationToken.None);

        result.Value.Days[^1].Completions.ShouldBe(4);
    }

    [Fact]
    public async Task HandleAsync_Should_ComputeDisciplineScore_OverThirtyDayWindow()
    {
        Hunter hunter = await SeedHunterAsync(createdDaysAgo: 60);
        SeedActiveDays(hunter.Id, daysBack: [.. Enumerable.Range(0, 15)]);

        Result<ConsistencyResponse> result = await _handler.HandleAsync(
            new GetConsistencyQuery(hunter.Id),
            CancellationToken.None);

        result.Value.DisciplineScore.ShouldBe(50);
    }

    public void Dispose() => _context.Dispose();

    private void SeedActiveDays(Guid hunterId, int[] daysBack)
    {
        IReadOnlyList<DailyCompletionRow> rows =
        [
            .. daysBack
                .OrderDescending()
                .Select(offset => new DailyCompletionRow { Day = Today.AddDays(-offset), Completions = 1 }),
        ];

        _gateway.GetDailyCompletionCountsAsync(hunterId, Arg.Any<CancellationToken>()).Returns(rows);
    }

    private async Task<Hunter> SeedHunterAsync(int createdDaysAgo = 60)
    {
        var hunter = Hunter.Create(
            "hunter@lvlup.app",
            "hash",
            "Jin-Woo",
            "Sung",
            "shadow_monarch",
            UtcNow.UtcDateTime.AddDays(-createdDaysAgo));

        _context.Hunters.Add(hunter);
        await _context.SaveChangesAsync();

        return hunter;
    }
}
