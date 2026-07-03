# Redis restart runbook

Use this when Redis is down, restarting, or unreachable from the API (OOM, service crash, network blip).

## Expected behavior after resilience changes

- The API **should stay up** when Redis is unavailable (`AbortOnConnectFail=false`).
- `/health/ready` should report **`Degraded`**, not **`Unhealthy`**, when only Redis is failing.
- MySQL failure still reports **`Unhealthy`** (database down = truly down).
- Rate limiting **fail-opens** by default (requests allowed) unless `EmergencyFallbackEnabled=true`.

## 1. Confirm API state

```powershell
curl https://api.hawkmerchandising.com/health/ready
```

- **`status: "Degraded"`** — Redis (or another non-critical dependency) is impaired; API is still serving traffic.
- **`status: "Unhealthy"`** — Check `database_schema` / `db` entries; MySQL or schema may be down.

Also check aggregate health:

```powershell
curl https://api.hawkmerchandising.com/health
```

## 2. Check Redis Windows service

```cmd
sc query Redis
```

Look for `STATE` — should be `RUNNING`. If `STOPPED` or `STOP_PENDING`, proceed to restart.

## 3. Restart Redis

```cmd
net start Redis
```

If the service fails to start, check Redis logs and memory/disk on the host before continuing.

## 4. Emergency rate-limit fallback (single-instance only)

If Redis cannot be restored quickly **and** you are on a **single API instance**, enable in-process rate limiting as a temporary escape hatch:

1. Set environment variable on the IIS app pool / API host:
   - `Scalability__EmergencyFallbackEnabled=true`
2. Recycle or restart the API site / app pool.

**Do not** leave this enabled on multi-instance deployments — each instance will enforce limits independently.

While `EmergencyFallbackEnabled=false` (default), rate limiting **allows all requests** when Redis errors (fail-open).

## 5. Restore normal operations

Once Redis is healthy again:

1. Verify `/health/ready` returns **`Healthy`** (or `Degraded` only for unrelated checks).
2. Set `Scalability__EmergencyFallbackEnabled=false` (or remove the override).
3. Restart / recycle the API so distributed cache, rate limits, and SignalR backplane use Redis again.

## Configuration reference

| Setting | Default (Production) | Purpose |
|--------|----------------------|---------|
| `Scalability:AllowInMemoryFallback` | `false` | Startup fallback for cache/Hangfire (not for runtime Redis blips) |
| `Scalability:EmergencyFallbackEnabled` | `false` | Runtime in-memory rate limit when Redis fails |
| `Scalability:RedisRetryCount` | `3` | Connect retries |
| `Scalability:RedisConnectTimeout` | `5000` | Connect timeout (ms) |
| `Scalability:RedisRetryBaseDelayMs` | `500` | Exponential reconnect base delay (ms) |

SignalR backplane and Hangfire Redis storage are **not** changed by this runbook; they manage their own reconnection.
