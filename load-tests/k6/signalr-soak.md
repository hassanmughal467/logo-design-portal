# SignalR load testing (Week 3)

k6 does not provide first-class WebSocket/SignalR load for .NET hubs. Use one of:

1. **Playwright E2E** — `Frontend/e2e/tests/realtime/` (health + reconnect)
2. **dotnet tool** — [Microsoft.AspNetCore.SignalR.Client](https://www.nuget.org/packages/Microsoft.AspNetCore.SignalR.Client) soak harness (recommended for 100+ connections)
3. **Azure Load Testing** or **Artillery** with WebSocket plugin

## Staging soak target (from LOAD_TESTING_PLAN.md)

- 100 concurrent hub connections for 10 minutes
- Reconnect every 2 minutes
- Verify `JoinUserGroup` after reconnect
- Redis backplane required (`ConnectionStrings__Redis`, `AllowInMemoryFallback=false`)

## Manual smoke

```bash
# API must expose /hubs/notifications with auth cookie or ?access_token=
curl -s "$API_BASE_URL/health/ready" | jq '.entries.signalr'
```
