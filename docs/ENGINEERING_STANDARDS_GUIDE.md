# Engineering Standards Guide

## Architecture (mandatory)

- Code only in `Backend/src/`
- Thin controllers; logic in Application services
- State changes via `OrderStatusStateMachine` / `OrderStatusTransitionHelper`
- DTO role masking — never expose cross-tenant PII
- Backend authorization on every mutation — never trust frontend
- Async I/O for database and external calls

## C# conventions

- Nullable reference types respected
- `ForbiddenAccessException` for authorization failures
- Structured logging via Serilog — no secrets in logs
- Uploads through `UploadSecurityHelper`

## TypeScript / Angular conventions

- Feature modules per domain area
- Interceptors for auth/CSRF only in `core/`
- Unsubscribe via `takeUntil` or `async` pipe
- No business authorization in components — UI hints only

## Testing (mandatory for touched features)

Per `.cursor/rules/testing.mdc`:

- Unit: Domain + Application
- Integration: `[Collection("Integration")]`
- E2E for user-visible workflows
- AAA pattern; deterministic; isolated DB per factory

## Pull request checklist

- [ ] Tests added/updated
- [ ] No secrets committed
- [ ] No legacy `Backend/LogoDesignPortal.API` changes
- [ ] Module QA doc updated if behavior change

## Code review focus

1. IDOR / access checks in **service** layer
2. Illegal order status transitions
3. Upload validation present
4. Invoice/payment privacy

## References

- `.cursor/rules/architecture.mdc`
- `.cursor/rules/testing.mdc`
- `docs/TESTING_STANDARDS.md`
