using LogoDesignPortal.Application.Caching;
using StackExchange.Redis;

namespace LogoDesignPortal.Infrastructure.Redis;

/// <summary>
/// Distributed cache epoch counters in Redis (shared across API instances).
/// </summary>
public sealed class RedisReadModelCacheVersions : IReadModelCacheVersions
{
    private readonly IDatabase _db;
    private const string UsersKey = "ldp:readmodel:epoch:users";
    private const string OrdersKey = "ldp:readmodel:epoch:orders";
    private const string AnalyticsKey = "ldp:readmodel:epoch:analytics";
    private const string RolesKey = "ldp:readmodel:epoch:roles";

    public RedisReadModelCacheVersions(IConnectionMultiplexer mux)
    {
        _db = mux.GetDatabase();
    }

    public long UsersEpoch => ReadLong(UsersKey);

    public long OrdersEpoch => ReadLong(OrdersKey);

    public long AnalyticsEpoch => ReadLong(AnalyticsKey);

    public long RolesEpoch => ReadLong(RolesKey);

    public void BumpUsers() => _db.StringIncrement(UsersKey);

    public void BumpOrders() => _db.StringIncrement(OrdersKey);

    public void BumpAnalytics() => _db.StringIncrement(AnalyticsKey);

    public void BumpRoles() => _db.StringIncrement(RolesKey);

    private long ReadLong(string key)
    {
        var v = _db.StringGet(key);
        return v.HasValue ? (long)v : 0L;
    }
}
