# Complete Technology Stack Reference

**Related:** [ARCHITECTURE_INDEX.md](./ARCHITECTURE_INDEX.md) · [BACKEND_ARCHITECTURE.md](./BACKEND_ARCHITECTURE.md) · [FRONTEND_ARCHITECTURE.md](./FRONTEND_ARCHITECTURE.md)

Inventory derived from `Frontend/package.json`, `Backend/src/**/*.csproj`, and `Program.cs` service registration.

---

## 1. Runtime platforms

| Technology | Version | Where | Purpose |
|------------|---------|-------|---------|
| .NET | 8.0 | API, Application, Domain, Infrastructure, Tests | Backend runtime |
| Node.js | 18 (CI) | Frontend build, Playwright | Tooling |
| Angular | 15.2.10 | Frontend SPA | UI framework |
| MySQL | 8.0 | Production/staging/E2E CI | Primary relational database |
| Redis | 6+ (StackExchange client 2.8) | Staging/production | Cache, SignalR, Hangfire, rate limit |
| IIS | Windows Server | Deployment host | Reverse proxy + static SPA |

---

## 2. Frontend dependencies (`Frontend/package.json`)

### 2.1 Production

| Package | Version | Purpose | Used in |
|---------|---------|---------|---------|
| `@angular/core` (+ platform, router, forms, animations, common) | 15.2.10 | SPA framework | All feature modules |
| `@angular/cdk` | 15.2.9 | CDK utilities | Layout, overlays |
| `@microsoft/signalr` | ^10.0.0 | Realtime hub client | `RealtimeNotificationService` |
| `primeng` | 15.4.0 | UI components (tables, dialogs, toast) | Feature modules, layout |
| `primeicons` | 6.0.1 | Icons | Global styles |
| `rxjs` | 7.5.7 | Reactive streams | Services, components |
| `zone.js` | 0.12.0 | Angular change detection | `polyfills` |
| `chart.js` | ^3.9.1 | Charts (legacy) | Some analytics |
| `apexcharts` | ^5.10.3 | Charts | Financial/analytics dashboards |
| `ng-apexcharts` | ^1.7.0 | Angular wrapper for ApexCharts | Chart components |

### 2.2 Development / test

| Package | Version | Purpose |
|---------|---------|---------|
| `@angular/cli` / `@angular-devkit/build-angular` | 15.2.10 | Build, serve |
| `@playwright/test` | ^1.58.2 | E2E automation |
| `@stryker-mutator/core` | ^9.6.0 | Mutation testing (optional) |
| `jasmine` + `karma` | 4.x / 6.x | Unit tests |
| `typescript` | 4.9.5 | Language |
| `dotenv` | ^16.6.1 | E2E env loading |

---

## 3. Backend NuGet packages

### 3.1 `LogoDesignPortal.API`

| Package | Version | Purpose |
|---------|---------|---------|
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 8.0.0 | JWT authentication |
| `Microsoft.AspNetCore.SignalR.StackExchangeRedis` | 8.0.11 | SignalR scale-out |
| `Microsoft.Extensions.Caching.StackExchangeRedis` | 8.0.1 | Distributed cache |
| `Hangfire.AspNetCore` | 1.8.17 | Background job dashboard/host |
| `Hangfire.Redis.StackExchange` | 1.9.3 | Distributed job storage |
| `Hangfire.MemoryStorage` | 1.8.1.1 | Dev fallback storage |
| `AspNetCore.HealthChecks.Redis` | 8.0.1 | Redis health probe |
| `Microsoft.EntityFrameworkCore.Design` | 8.0.0 | Migrations tooling |
| `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore` | 8.0.11 | DB health |
| `Serilog.AspNetCore` | 10.0.0 | Structured logging host |
| `Serilog.Enrichers.Environment` | 3.0.1 | Machine/env enrichers |
| `Serilog.Formatting.Compact` | 3.0.0 | JSON log format |
| `Serilog.Settings.Configuration` | 10.0.0 | Config-driven Serilog |
| `Serilog.Sinks.Console` | 6.1.1 | Console sink |
| `Swashbuckle.AspNetCore` | 6.5.0 | OpenAPI (**Development only**) |
| `Polly.Extensions` | 8.6.6 | Resilience (via hosting extensions) |
| `OpenTelemetry.*` | 1.15.x | Metrics/traces (optional) |
| `Azure.Monitor.OpenTelemetry.Exporter` | 1.6.0 | Azure Monitor export |

### 3.2 `LogoDesignPortal.Application`

| Package | Version | Purpose |
|---------|---------|---------|
| `AutoMapper` + `Extensions.Microsoft.DependencyInjection` | 12.0.1 | DTO mapping |
| `BCrypt.Net-Next` | 4.0.3 | Password hashing |
| `Microsoft.EntityFrameworkCore` | 8.0.0 | Abstractions / queries in services |
| `Microsoft.Extensions.Http.Resilience` | 8.10.0 | Outbound HTTP resilience |
| `QuestPDF` | 2024.12.2 | Invoice PDF generation |

### 3.3 `LogoDesignPortal.Infrastructure`

| Package | Version | Purpose |
|---------|---------|---------|
| `Pomelo.EntityFrameworkCore.MySql` | 8.0.0 | MySQL provider |
| `Microsoft.EntityFrameworkCore.Sqlite` | 8.0.0 | SQLite for tests |
| `Microsoft.EntityFrameworkCore.Tools` | 8.0.0 | Migrations CLI |
| `System.IdentityModel.Tokens.Jwt` | 8.2.1 | JWT creation |
| `Microsoft.IdentityModel.Tokens` | 8.2.1 | Signing credentials |
| `StackExchange.Redis` | 2.8.24 | Redis connection multiplexer |

### 3.4 Test projects

| Package | Purpose |
|---------|---------|
| `xunit` 2.6.2 | Test framework |
| `Microsoft.NET.Test.Sdk` | Test runner |
| `coverlet.collector` | Coverage |
| `Moq` | Application unit mocks |
| `Microsoft.AspNetCore.Mvc.Testing` | `WebApplicationFactory` |
| `Microsoft.EntityFrameworkCore.InMemory` | Integration DB |

---

## 4. Infrastructure dependencies (operational)

| Dependency | Purpose | Documentation |
|------------|---------|---------------|
| ASP.NET Core Hosting Bundle | IIS module for .NET 8 | IIS deploy guides |
| MySQL Server 8 | Application data | `ENVIRONMENT_SETUP_GUIDE.md` |
| Redis | Scale-out services | `REDIS_PRODUCTION_GUIDE.md` |
| SMTP server | Transactional email | `Email` appsettings section |
| TLS certificates | HTTPS | IIS binding |

---

## 5. Security-related libraries

| Library | Layer | Role |
|---------|-------|------|
| `JwtBearer` | API | Validate access tokens |
| `System.IdentityModel.Tokens.Jwt` | Infrastructure | Issue tokens |
| `BCrypt.Net-Next` | Application | Password storage |
| Custom `CsrfValidationMiddleware` | API | CSRF for cookies |
| Custom `UploadSecurityHelper` | Application | File upload policy |
| Custom `RateLimitingMiddleware` | API | Abuse throttling |
| Custom `SecurityHeadersMiddleware` | API | HSTS, CSP report-only, X-Frame-Options |

---

## 6. Logging and observability

| Component | Purpose |
|-----------|---------|
| Serilog | Request logging, structured properties |
| `CorrelationIdMiddleware` | Trace correlation |
| `SlowRequestPerformanceMiddleware` | Latency warnings |
| OpenTelemetry exporters | Optional OTLP / Azure Monitor |
| Health checks | Liveness/readiness for orchestration |

---

## 7. Realtime and messaging

| Technology | Where | Purpose |
|------------|-------|---------|
| ASP.NET Core SignalR | API `NotificationHub` | Push notifications + order grid events |
| StackExchange Redis backplane | `Program.cs` | Multi-instance hub |
| `@microsoft/signalr` | Frontend | Client connection |

---

## 8. Background processing

| Technology | Purpose |
|------------|---------|
| Hangfire | Email send, orphan file cleanup, billing auto-invoice |
| `IBackgroundJobScheduler` abstraction | Swap `HangfireBackgroundJobScheduler` vs `NullBackgroundJobScheduler` in tests |

---

## 9. Data access

| Technology | Purpose |
|------------|---------|
| EF Core 8 | ORM, migrations, global filters |
| Pomelo MySQL | Production provider |
| SQLite / InMemory | Tests |

---

## 10. CI/CD tooling

| Tool | Workflow | Purpose |
|------|----------|---------|
| GitHub Actions | `test.yml`, `pr-validation.yml`, `e2e-playwright.yml`, `deploy-artifacts.yml` | CI/CD |
| `dotnet format` | test.yml | Style enforcement |
| Coverlet | test.yml | Code coverage |
| Playwright | test.yml smoke + e2e workflow | E2E |
| Karma + ChromeHeadlessCI | test.yml | Angular unit tests |

---

## 11. Deployment artifacts

| Output | Produced by |
|--------|-------------|
| `LogoDesignPortal.API.zip` | `dotnet publish` |
| Frontend `dist/` zip | `ng build --configuration=production` |
| `web.config` | API project content |

---

## 12. Package → architectural role map

```mermaid
flowchart LR
  subgraph UI
    NG[Angular]
    PN[PrimeNG]
    SR[@microsoft/signalr]
  end
  subgraph API
    JWT[JwtBearer]
    HF[Hangfire]
    SIG[SignalR Server]
  end
  subgraph Data
    EF[EF Core]
    MY[(MySQL)]
    RD[(Redis)]
  end
  NG --> API
  SR --> SIG
  API --> EF
  EF --> MY
  API --> RD
  HF --> RD
  SIG --> RD
```

---

## 13. Intentionally not used

| Technology | Note |
|------------|------|
| NgRx / Akita | Service + RxJS state only |
| MediatR / CQRS | Direct service methods |
| Material UI | PrimeNG chosen (forbidden per onboarding) |
| Legacy `Backend/` outside `src` | Excluded from solution |

---

## 14. Version upgrade pointers

| Stack | Guide |
|-------|-------|
| Angular | `docs/ANGULAR_UPGRADE_PLAN.md` |
| .NET | Track LTS releases; currently on .NET 8 LTS |
| MySQL | Test migrations on staging before prod |
| Redis | Match StackExchange.Redis compatibility with server version |
