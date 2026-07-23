# Observability Setup

## Current (Week 2)

| Capability | Implementation |
|------------|----------------|
| Structured logging | Serilog compact JSON, `FromLogContext` |
| Correlation IDs | `CorrelationIdMiddleware` → `X-Correlation-Id` |
| Request logging | `UseSerilogRequestLogging` with UserId, UserRole, OrderId, InvoiceId |
| Slow requests | `SlowRequestPerformanceMiddleware` (≥500ms warning) |
| Exception taxonomy | `ExceptionCategory` on unhandled errors |
| Health | `/health`, `/health/ready`, `/health/live` |
| OpenTelemetry | Opt-in via `Observability:Enabled` in `appsettings.json` |

## Configuration

```json
"Observability": {
  "Enabled": false,
  "ServiceName": "LogoDesignPortal.API",
  "OtlpEndpoint": "http://otel-collector:4318",
  "ApplicationInsightsConnectionString": ""
}
```

When `Enabled` is true:

- Traces: ASP.NET Core, HttpClient, EF Core
- Metrics: runtime, ASP.NET Core, HttpClient
- Export: OTLP and/or Azure Monitor when connection string set

## Recommended production stack

| Tool | Role |
|------|------|
| **Grafana + Loki** | Log aggregation (ingest Serilog JSON via Promtail/Alloy) |
| **Grafana Tempo / Jaeger** | Trace backend (OTLP from API) |
| **Prometheus** | Scrape `/metrics` when OTel metrics exporter exposed |
| **Sentry** | Error grouping — map `ExceptionCategory` + `CorrelationId` as tags |
| **Application Insights** | Azure-hosted APM (package already referenced) |

## Safe logging rules

- Never log JWT, refresh tokens, passwords, or full payment payloads
- Use route-level enrichment only (OrderId, InvoiceId), not request bodies on auth routes
- Production 500 responses omit stack unless `IncludeExceptionDetailsInProduction` is explicitly true

## Alerts (recommended)

- `ExceptionCategory=Unknown` rate spike
- `/health/ready` non-200 > 1 min
- `hangfire` or `redis` unhealthy
- `memory` degraded for 5+ min
- p95 HTTP duration > 2s (from OTel or slow-request logs)
