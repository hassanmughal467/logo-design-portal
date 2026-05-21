# Redis Production Guide (Week 3)

## Architecture

`ScalabilityServiceRegistration` wires:

| Component | Redis usage | Fallback (single-instance only) |
|-----------|-------------|--------------------------------|
| `IDistributedCache` | StackExchange.Redis | `MemoryDistributedCache` |
| Read-model epochs | `RedisReadModelCacheVersions` | `ReadModelCacheVersions` |
| Rate limiting | `RedisDistributedRateLimiter` | `MemoryDistributedRateLimiter` |
| Hangfire | `UseRedisStorage` prefix `hangfire:ldp:` | `UseMemoryStorage` |
| SignalR | Backplane channel `ldp:signalr` | None (same-node only) |

## Production requirements

1. **`ConnectionStrings__Redis`** — required in Production/Staging when `AllowInMemoryFallback=false`.
2. **`Scalability:AllowInMemoryFallback`** — **must be `false`** for 2+ API instances (fixed in `appsettings.Production.json` Week 3).
3. **Health:** `/health/ready` includes Redis when connection string is set.
4. **TLS:** Use `rediss://` or Azure/AWS managed Redis with TLS in connection string.

## Connection settings

- `AbortOnConnectFail = false` — app starts if Redis is briefly down (cache fail-open).
- Non-dev without fallback: **throws** if Redis missing or connect fails at startup.

## Multi-instance checklist

- [ ] Redis reachable from all API nodes
- [ ] Same Redis DB/prefix for Hangfire and SignalR
- [ ] Shared file storage OR object storage (local disk is not multi-instance safe)
- [ ] Load balancer: no sticky sessions required **when SignalR backplane is active**

## Failure behavior

| Failure | Current behavior | Recommendation |
|---------|------------------|----------------|
| Redis down at runtime | Cache miss; rate limiter fail-open | Alert + fail-closed for auth buckets (Week 4) |
| Redis down at startup (prod) | Process exit | Correct for multi-instance |
| Epoch bump fails | Stale read models until TTL | Monitor bump errors |

## Environment variables (example)

```
ConnectionStrings__Redis=your-redis:6379,password=***,ssl=True
Scalability__AllowInMemoryFallback=false
```

See [CACHE_STRATEGY.md](./CACHE_STRATEGY.md).
