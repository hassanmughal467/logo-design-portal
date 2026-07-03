# Coding Rules

Standards for humans and AI agents. These consolidate `.cursor/rules/architecture.mdc`, `.cursor/rules/testing.mdc`, the root `cursorrules` file, and `docs/ENGINEERING_STANDARDS_GUIDE.md`. Where they conflict, this file and `AGENTS.md` win.

## Non-negotiables

1. **Backend code lives in `Backend/src/` only.** `Backend/LogoDesignPortal.*` folders outside `src/` are legacy — never edit them.
2. **Clean Architecture direction**: Domain → Application → Infrastructure → API. Domain references nothing; Application references Domain only; never import Infrastructure into Domain or Application.
3. **Order status changes go through `OrderStatusStateMachine`** (via `OrderStatusTransitionHelper.Apply()`). Never assign `order.Status` directly in a service or controller.
4. **Controllers stay thin**: parse request → call Application service → return response. No business logic, no direct DbContext access in controllers.
5. **Authorization is server-side and mandatory**: `[Authorize]` + role lists + `[RequirePermission]` + service-level access checks. Frontend guards are UX only — never a security boundary.
6. **Role masking in DTOs**: clients never see designer identity/pricing; designers never see client PII or client prices. Enforce in Application-layer mapping.
7. **No secrets in code or committed config.** Env vars / User Secrets only. Never log tokens, hashes, or connection strings.
8. **All uploads validated** through `UploadSecurityHelper` (extension, MIME, magic bytes, traversal, size, duplicate window).
9. **Structured logging with Serilog** — message templates with named properties, correlation IDs preserved; never string-interpolate into log messages.
10. **Async everywhere for I/O** — no `.Result` / `.Wait()` (deadlock risk under ASP.NET).

## C# / .NET

- `async/await` throughout; accept a `CancellationToken` on public async service methods.
- Guard clauses at the top of methods — fail fast with meaningful exceptions/results.
- Inject `ILogger<T>` via constructor; use `IOptions<T>`-bound configuration classes (e.g. `StorageOptions`, `RateLimitingOptions`) rather than reading `IConfiguration` inline.
- DTOs live in `Application/DTOs/`; prefer records where practical. Never return Domain entities from controllers.
- Interfaces in `Application/Interfaces/`, implementations in Application or Infrastructure per the dependency rule.
- Reusable logic goes in `Application/Helpers/` (see `OrderLockingHelper`, `ClientCurrencyHelper`, `PaymentInvoiceAccessHelper`) — avoid duplicating access rules across services.
- No magic strings: permission names, cookie names, status values come from constants/enums.
- Soft delete by default — set `IsDeleted` and respect global query filters; only hard-delete where an existing service already does so deliberately.
- Schema changes via `dotnet ef migrations add` only; never hand-edit migrations or the model snapshot; never raw DDL in services.
- EF queries: use `AsNoTracking` for read paths, project to DTOs, prefer indexed columns in filters; long-running or racy mutations rely on `RowVersion` concurrency tokens (orders, invoices).
- XML doc comments on public interfaces and service methods.
- Code must pass `dotnet format --verify-no-changes` (CI-enforced) and build clean in Release with analyzers.

## Angular / TypeScript

- **PrimeNG only** — no Angular Material, ng-bootstrap, or other UI libraries. Prefer the shared UI kit in `src/app/shared/ui-components/` before building new widgets.
- **No NgRx** — state is services + RxJS (`BehaviorSubject`, `shareReplay`).
- Typed everything — no `any` unless truly unavoidable; API models in `shared/models/`.
- All HTTP goes through `core/services/api.service.ts` or a typed feature service — never `HttpClient` directly in components.
- Auth/CSRF/loading behavior comes from the registered interceptors — don't add per-request headers manually.
- New components use `OnPush` change detection; unsubscribe via `takeUntilDestroyed()` or `ngOnDestroy`.
- Feature modules are lazy-loaded; register routes with `AuthGuard` + `RoleGuard` (`data.roles`) consistent with the backend role list for the corresponding endpoints.
- Debug logging via `LoggerService` (gated by `enableDebugLogging`) — no stray `console.log`.
- Respect production budgets (initial 500 kb warn / 1 mb error).

## Testing requirements

See [testing-plan.md](testing-plan.md) for full detail. Minimum bar for any change touching business logic:

- Unit tests in `Domain.Tests` or `Application.Tests` (AAA pattern, deterministic, isolated).
- Integration tests in `API.IntegrationTests` (`[Collection("Integration")]`) for API-visible behavior — assert status codes **and** bodies.
- Security-sensitive areas (auth, orders, invoices, uploads, role masking, state machine) additionally need: 401/403 tests, wrong-role tests, IDOR tests, invalid-transition tests, duplicate-action tests.
- Playwright E2E in `Frontend/e2e/tests/` for changed user-facing workflows.
- Never weaken assertions or skip tests to go green — fix the root cause.

## Git and PRs

- CI must pass: `pr-validation.yml` (format, build, unit tests, typecheck, Angular prod build) and `test.yml` (full matrix + ≥80% integration coverage).
- No commented-out code in commits; no new root-level `.md` reports (use `docs/`).
- Follow `.github/pull_request_template.md`.

## What NOT to do — ever

- Do not add NgRx, MediatR, CQRS scaffolding, or non-PrimeNG UI libraries.
- Do not expose Swagger outside Development.
- Do not enable automatic migrations in Staging/Production.
- Do not bypass the state machine, role masking, CSRF validation, rate limiting, or upload validation.
- Do not hardcode environment-specific URLs, keys, or credentials.
- Do not use `.Result`/`.Wait()`.
- Do not trust frontend-supplied roles/permissions for any decision.
