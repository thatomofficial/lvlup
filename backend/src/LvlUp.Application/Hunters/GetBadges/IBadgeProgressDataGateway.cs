namespace LvlUp.Application.Hunters.GetBadges;

/// <summary>
/// Reads a hunter's quest completion counts per stat category in a single
/// aggregated round trip. Implemented in Infrastructure with a raw SQL
/// GROUP BY query.
/// </summary>
public interface IBadgeProgressDataGateway
{
    Task<IReadOnlyList<CategoryCompletionRow>> GetCompletionCountsByCategoryAsync(
        Guid hunterId,
        CancellationToken cancellationToken = default);
}
