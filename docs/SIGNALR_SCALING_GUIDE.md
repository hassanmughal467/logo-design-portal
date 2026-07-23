# SignalR Scaling Guide (Week 3)

## Hub design

- **Endpoint:** `/hubs/notifications`
- **Hub:** `NotificationHub` — `JoinUserGroup` adds connection to `user-{userId}` (authorized)
- **Senders:** `SignalRRealtimeNotificationSender`, `SignalRRealtimeEntityUpdateSender` — **per-user groups only** (no broadcast-all)

## Multi-instance

Requires Redis backplane (`AddStackExchangeRedis`, prefix `ldp:signalr`):

```csharp
// Program.cs — when redisOk && !Testing
builder.Services.AddSignalR().AddStackExchangeRedis(...);
```

Without Redis: realtime events only reach clients on the **same API instance**.

## Frontend

- `realtime-notification.service.ts`: `withAutomaticReconnect()`, re-invokes `JoinUserGroup` on reconnect
- Synthetic `orderId: '**reconnect**'` triggers dashboard refresh
- Fallback: 60s polling via `notification.service.ts`

## Reliability recommendations

1. **Staging soak:** 100 connections × 10 min (see `load-tests/k6/signalr-soak.md`).
2. Extend `SignalRHealthCheck` to verify Redis multiplexer when backplane required.
3. Log `JoinUserGroup` failures (client never receives pushes).
4. Throttle high-frequency entity updates per user (Week 4 if needed).

## Load balancer

- WebSockets: enable sticky sessions **only if** backplane is unavailable (degraded mode).
- With backplane: round-robin is fine.

## Testing

- E2E: `Frontend/e2e/tests/realtime/signalr-health.api.spec.ts`
- Integration: Testing environment intentionally omits backplane
