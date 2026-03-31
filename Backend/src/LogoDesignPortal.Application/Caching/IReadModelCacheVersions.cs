namespace LogoDesignPortal.Application.Caching;

/// <summary>
/// Epoch tokens for read-model cache keys. Incrementing an epoch invalidates all entries
/// for that slice without scanning the cache (stale keys still expire via TTL).
/// Use Redis-backed implementation in multi-instance deployments so bumps are visible everywhere.
/// </summary>
public interface IReadModelCacheVersions
{
    long UsersEpoch { get; }
    long OrdersEpoch { get; }
    long AnalyticsEpoch { get; }
    long RolesEpoch { get; }

    void BumpUsers();
    void BumpOrders();
    void BumpAnalytics();
    void BumpRoles();
}
