using LvlUp.Domain.Hunters;
using Shouldly;

namespace LvlUp.UnitTests.Domain;

public class BadgeCatalogTests
{
    [Fact]
    public void All_Should_ContainFourBadgesPerCategory()
    {
        BadgeCatalog.All.Count.ShouldBe(Enum.GetValues<StatCategory>().Length * 4);
    }

    [Fact]
    public void All_Should_HaveDistinctNames()
    {
        BadgeCatalog.All.Select(badge => badge.Name).ShouldBeUnique();
    }

    [Fact]
    public void All_Should_HaveAscendingThresholdsWithinEachCategory()
    {
        foreach (StatCategory category in Enum.GetValues<StatCategory>())
        {
            List<int> thresholds =
            [
                .. BadgeCatalog.All
                    .Where(badge => badge.Category == category)
                    .OrderBy(badge => badge.Tier)
                    .Select(badge => badge.RequiredCompletions),
            ];

            thresholds.ShouldBeInOrder(SortDirection.Ascending);
        }
    }
}
