# Test Coverage Gap Report

## P0 — Before public launch

| Gap | Layer | Recommended test |
|-----|-------|------------------|
| CSRF middleware | Integration | **Done** — `CsrfCookieAuthIntegrationTests` |
| Cookie auth login | E2E | Login with `useCookieAuth`; verify CSRF header on POST (API covered) |
| Designer invoice read | Integration | **Done** — `GetInvoiceById_AsDesigner_Returns404` |

## P1 — First 30 days post-launch

| Gap | Layer | Recommended test |
|-----|-------|------------------|
| SignalR hub message delivery | Integration | Connect hub, receive notification after order update |
| SignalR reconnect | E2E / unit | `RealtimeNotificationService` reconnect backoff |
| PayPal webhook events | Integration | `PAYMENT.CAPTURE.COMPLETED` updates payment row |
| File download IDOR UI | E2E | Enable `file-download-idor.spec.ts` with seed |
| Download path containment | Unit | `FileService` rejects path outside root |

## P2 — Quality improvement

| Gap | Layer |
|-----|-------|
| Frontend `order-detail` component | Unit |
| Frontend `invoice-list` | Unit |
| Duplicate upload window UI | E2E |
| Invalid order transition UI | Component + E2E |
| Staging config validator | Unit (extend Production tests) |

## Coverage by module (backend)

| Module | Integration | Application unit |
|--------|-------------|------------------|
| Auth | Strong | Strong |
| Orders | Strong | Strong |
| Invoices | Strong (post-fix) | Medium |
| Payments | Strong | Medium |
| Files/Uploads | Strong | Strong |
| Revisions | Medium | Medium |
| Designer payout | Strong | Medium |
| Analytics | Weak | Weak |
| Settings | Weak | None |

## Frontend coverage

- **~17%** of components have spec files (15/89 approximate)
- Interceptors: only `token.interceptor.spec.ts` — **csrf**, **cookie-credentials** missing

## Commands to verify gaps closed

```bash
dotnet test Backend/src/LogoDesignPortal.API.IntegrationTests
cd Frontend && npm run test:ci
npm run e2e
```
