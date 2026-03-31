using System.Threading;

namespace LogoDesignPortal.Application.Caching;

public sealed class ReadModelCacheVersions : IReadModelCacheVersions
{
    private long _users;
    private long _orders;
    private long _analytics;
    private long _roles;

    public long UsersEpoch => Interlocked.Read(ref _users);
    public long OrdersEpoch => Interlocked.Read(ref _orders);
    public long AnalyticsEpoch => Interlocked.Read(ref _analytics);
    public long RolesEpoch => Interlocked.Read(ref _roles);

    public void BumpUsers() => Interlocked.Increment(ref _users);

    public void BumpOrders() => Interlocked.Increment(ref _orders);

    public void BumpAnalytics() => Interlocked.Increment(ref _analytics);

    public void BumpRoles() => Interlocked.Increment(ref _roles);
}
