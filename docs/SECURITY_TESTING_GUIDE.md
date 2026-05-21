# Security Testing Guide

## Automated layers

| Layer | Location | Focus |
|-------|----------|--------|
| Integration | `Backend/src/LogoDesignPortal.API.IntegrationTests/Security/` | JWT, refresh abuse, IDOR, path traversal |
| Integration | `Controllers/*PrivacyTests`, `*AuthorizationTests` | DTO masking, 403/401 |
| Integration | `FilesControllerUploadSecurityIntegrationTests` | Extension, magic bytes, auth |
| Playwright | `Frontend/e2e/tests/security/` | RBAC, hidden data, state transitions |
| Domain | `LogoDesignPortal.Domain.Tests/OrderStatusStateMachineTests` | Illegal transitions |

## Week 2 additions

- `IdorAndCrossTenantIntegrationTests` — invoice cross-role, path traversal on file download
- `RefreshTokenAndReplayIntegrationTests` — garbage refresh, distinct login tokens
- `MarkPaidDuplicateRegressionTests` — financial duplicate action
- E2E: `hidden-data-exposure`, `invalid-state-transition`, upload abuse suite

## Checklist (every release)

1. Broken access control — wrong role on admin endpoints (403)
2. IDOR — order/invoice/file by foreign ID (403/404)
3. JWT tampering / expired / none alg (401)
4. Refresh token misuse (400/401)
5. Upload: exe, double extension, magic-byte mismatch, oversize (400/413)
6. Designer never sees client email; client never sees designer email
7. State machine — no skip to `Completed` / `Refunded` without rules
8. No tokens/passwords in logs (Serilog + exception middleware)

## Manual / staging

- CSRF with cookie auth (`AuthCookies:Enabled`)
- Stripe webhook signature (`PaymentsControllerWebhookSecurityTests`)
- Rate limit headers under auth spike (`load-tests/k6/auth-spike.js`)

See `docs/PRODUCTION_SECURITY_CHECKLIST.md` and `docs/WEEK1_SECURITY_HARDENING.md`.
