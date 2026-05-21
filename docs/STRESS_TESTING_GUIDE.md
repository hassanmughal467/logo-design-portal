# Stress Testing Guide

## Prerequisites

- k6 installed (`choco install k6` or https://k6.io)
- Staging API with seed users (`client@test.com`, admin accounts)
- Redis enabled, `AllowInMemoryFallback=false`
- Tokens for authenticated scripts (`ADMIN_TOKEN`, `ACCESS_TOKEN`)

## Run auth spike

```bash
k6 run load-tests/k6/auth-spike.js -e API_BASE_URL=https://staging-api.example.com
```

## Run dashboard read

```bash
k6 run load-tests/k6/dashboard-read.js -e API_BASE_URL=... -e ADMIN_TOKEN=...
```

## Run concurrent orders

```bash
k6 run load-tests/k6/concurrent-orders.js -e API_BASE_URL=...
```

## Run invoice spike

```bash
k6 run load-tests/k6/invoice-spike.js -e API_BASE_URL=... -e ADMIN_TOKEN=...
```

## Concurrency / integration

Backend: `SystemStressWorkflowTests.cs` (in-memory, workflow safety).

Run full suite:

```bash
dotnet test Backend/src/LogoDesignPortal.Application.Tests
dotnet test Backend/src/LogoDesignPortal.API.IntegrationTests
```

## SignalR

See `load-tests/k6/signalr-soak.md` — use dedicated harness for 100+ WebSocket connections.

## Record results

Copy [LOAD_TEST_RESULTS_TEMPLATE.md](./LOAD_TEST_RESULTS_TEMPLATE.md) per run.

See also [PERFORMANCE_TESTING_GUIDE.md](./PERFORMANCE_TESTING_GUIDE.md), [LOAD_TESTING_PLAN.md](./LOAD_TESTING_PLAN.md).
