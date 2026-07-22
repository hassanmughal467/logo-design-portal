# Authentication & Authorization Security Audit — Findings

**Scope:** JWT issuance/validation, `AuthService`/`AuthController`, refresh-token handling, rate limiting, forwarded-header trust, the permission/role system across all controllers, file/payment ownership checks, and frontend token storage.

**Date:** 2026-07-22
**Status key:** ✅ Fixed and merged to `develop` · 🔲 Open

No Critical findings — nothing was exploitable pre-authentication without at least valid credentials or an existing account.

---

## Summary

| # | Finding | Priority | Status |
|---|---|---|---|
| 1 | Rate limiter trusts spoofable `X-Forwarded-For` header | High | ✅ Fixed — [PR #7](https://github.com/hassanmughal467/logo-design-portal/pull/7) |
| 2 | Full exception stack traces exposed to clients in Prod/Staging | High | ✅ Fixed — [PR #7](https://github.com/hassanmughal467/logo-design-portal/pull/7) |
| 3 | Forwarded headers trusted from any client (IP spoofing) | High | ✅ Fixed — [PR #7](https://github.com/hassanmughal467/logo-design-portal/pull/7) |
| 4 | Password change doesn't revoke refresh tokens | Medium | ✅ Fixed — [PR #8](https://github.com/hassanmughal467/logo-design-portal/pull/8) |
| 5 | PayPal verify endpoint missing ownership check | Medium | ✅ Fixed — [PR #8](https://github.com/hassanmughal467/logo-design-portal/pull/8) |
| 6 | JWTs stored in `localStorage`, not httpOnly | Medium | ✅ Fixed — [PR #9](https://github.com/hassanmughal467/logo-design-portal/pull/9) |
| 7 | Weak, inconsistent password policy at registration | Low | ✅ Fixed — [PR #9](https://github.com/hassanmughal467/logo-design-portal/pull/9) |
| 8 | Lockout message enables account enumeration | Low | ✅ Fixed — [PR #9](https://github.com/hassanmughal467/logo-design-portal/pull/9) |
| 9 | Hardcoded default SuperAdmin recovery password | Low | ✅ Fixed — [PR #9](https://github.com/hassanmughal467/logo-design-portal/pull/9) |

---

## High Priority

### 1. Rate limiter trusts spoofable `X-Forwarded-For` header ✅ Fixed
**File:** `Backend/src/LogoDesignPortal.API/Extensions/HttpContextRateLimitExtensions.cs:7`
**Status:** Fixed in [PR #7](https://github.com/hassanmughal467/logo-design-portal/pull/7)

The rate limiter's client-identity resolver read the raw `X-Forwarded-For` request header directly, with no validation that the request actually came through a trusted proxy. An attacker scripting `POST /api/auth/login` could set a different fake `X-Forwarded-For` value on every request and completely bypass the 5-requests/minute brute-force throttle (`AuthPerMinute`), enabling unlimited-speed credential stuffing against any account, plus unrestricted abuse of `/api/auth/register` and `/api/auth/forgot-password`.

**Fix:** Now reads `HttpContext.Connection.RemoteIpAddress`, which — after fixing #3 — is only rewritten from forwarded headers when the request genuinely comes from a trusted proxy. Covered by 3 new unit tests proving spoofed headers no longer change the resolved identity.

---

### 2. Full exception stack traces exposed to clients in Prod/Staging ✅ Fixed
**File:** `Backend/src/LogoDesignPortal.API/appsettings.Production.json:8` (identical in `appsettings.Staging.json`)
**Status:** Fixed in [PR #7](https://github.com/hassanmughal467/logo-design-portal/pull/7)

`IncludeExceptionDetailsInProduction` was set to `true` in both Production and Staging configs. Any unhandled exception caused `ExceptionMiddleware.cs` to serialize `exception.ToString()` — full stack trace, internal file paths, method/class names — directly into the JSON response sent to the client, including on endpoints reachable anonymously.

**Fix:** Flipped to `false` in both files. Full exception details still reach Serilog server-side via the existing `_logger.LogError` call; nothing was lost for debugging.

---

### 3. Forwarded headers trusted from any client (IP spoofing) ✅ Fixed
**File:** `Backend/src/LogoDesignPortal.API/Program.cs:227`
**Status:** Fixed in [PR #7](https://github.com/hassanmughal467/logo-design-portal/pull/7)

`ForwardedHeadersOptions` called `KnownNetworks.Clear()` and `KnownProxies.Clear()`, which disables ASP.NET Core's default loopback-only trust boundary. Any external request could set `X-Forwarded-For: 127.0.0.1` and have it accepted as if it came from a real reverse proxy — undermining every downstream IP-based decision, including the localhost check gating `AuthController`'s SuperAdmin-password-reset endpoint (finding #9), and enabling finding #1.

**Fix:** Restored the secure (loopback-only) default. Real deployments that do sit behind a load balancer can opt in via an optional `ForwardedHeaders:TrustedProxies` config list — nothing configured means the same secure default as before this option existed.

---

## Medium Priority

### 4. Password change doesn't revoke refresh tokens ✅ Fixed
**File:** `Backend/src/LogoDesignPortal.Application/Services/AuthService.cs` — `ChangePasswordAsync`, `ResetPasswordAsync`, `ResetPasswordWithTokenAsync`
**Status:** Fixed in [PR #8](https://github.com/hassanmughal467/logo-design-portal/pull/8)

None of the three password-change code paths cleared `user.RefreshToken`/`RefreshTokenExpiryTime`. An attacker holding a victim's refresh token (stolen device, XSS-driven `localStorage` read — see finding #6) kept a persistent foothold even after the victim "secured" their account by changing the password: the attacker could keep calling `/api/auth/refresh-token` indefinitely to mint fresh access tokens.

**Fix:** All three methods now clear `RefreshToken`/`RefreshTokenExpiryTime` alongside the password update, forcing re-authentication everywhere. Covered by 3 new unit tests.

---

### 5. PayPal verify endpoint missing ownership check ✅ Fixed
**File:** `Backend/src/LogoDesignPortal.API/Controllers/PaymentsController.cs:119` (`VerifyPayPalPayment`)
**Status:** Fixed in [PR #8](https://github.com/hassanmughal467/logo-design-portal/pull/8)

Any authenticated user of any role — a Client, or a Designer with no relation to the order — could `POST /api/payments/verify/paypal` with an arbitrary `orderId`/`paymentId` and get back `{verified: true/false}` for someone else's PayPal order. Every sibling endpoint in the same controller (`GetPayment`, `GetPaymentsByInvoice`) correctly routes through an access-controlled service method; this one didn't.

**Fix:** Added `VerifyPayPalPaymentWithAccessAsync`, which resolves the order ID to our internal `Payment` record first and applies the same Client-must-own-the-invoice / Admin-SuperAdmin-can-access-all rule used elsewhere, returning 404 for both "no such payment" and "not yours." Confirmed the frontend method calling this endpoint isn't wired into any component yet, so the fix carried no risk to a live flow. Covered by 4 new unit tests.

---

### 6. JWTs stored in `localStorage`, not httpOnly ✅ Fixed
**File:** `Frontend/src/app/core/services/auth.service.ts`
**Status:** Fixed in [PR #9](https://github.com/hassanmughal467/logo-design-portal/pull/9)

Access and refresh JWTs were persisted in `localStorage`, readable by any JavaScript running on the page. A XSS vector would have allowed exfiltration of both tokens.

**Fix:** Access token is now kept **in-memory only** (`this.accessToken` private field) — it is never written to `localStorage` or `sessionStorage` and disappears on page reload. A `auth_token_for_refresh` key in `sessionStorage` holds the raw JWT value used only by the `/auth/refresh-token` call, so silent refresh can succeed after a reload. The refresh token and user profile (non-credential) are also in `sessionStorage` (tab-scoped, cleared on tab close). Legacy `localStorage` keys (`auth_token`, `auth_refresh_token`, `auth_expires_at`, `auth_user`) are wiped on every app start via `migrateLegacyLocalStorage()`. Full httpOnly cookie migration was not pursued here because it would require backend cookie middleware, CORS credential config, and a SignalR rewrite; the in-memory approach eliminates the exfiltration risk at lower change cost.

---

## Low Priority

### 7. Weak, inconsistent password policy at registration ✅ Fixed
**File:** `Backend/src/LogoDesignPortal.Application/DTOs/Auth/RegisterRequestDto.cs:12`
**Status:** Fixed in [PR #9](https://github.com/hassanmughal467/logo-design-portal/pull/9)

Registration allowed a 6-character password and hashed with the BCrypt default work factor, inconsistent with other flows requiring 8+ characters and `workFactor: 12`.

**Fix:** `[MinLength(6)]` raised to `[MinLength(8)]` on `RegisterRequestDto.Password`. `RegisterAsync` now passes `workFactor: 12` to `BCrypt.Net.BCrypt.HashPassword`.

---

### 8. Lockout message enables account enumeration ✅ Fixed
**File:** `Backend/src/LogoDesignPortal.Application/Services/AuthService.cs:61`
**Status:** Fixed in [PR #9](https://github.com/hassanmughal467/logo-design-portal/pull/9)

The lockout-specific message `"Account is locked. Please try again in N minute(s)."` disclosed both that the account exists and the precise remaining lockout duration.

**Fix:** Locked accounts now throw `"Invalid email or password."` — identical to the wrong-password and nonexistent-email responses — so no information leaks about lockout state.

---

### 9. Hardcoded default SuperAdmin recovery password ✅ Fixed
**File:** `Backend/src/LogoDesignPortal.Application/Services/AuthService.cs` (`ResetSuperAdminPasswordAsync`)
**Status:** Fixed in [PR #9](https://github.com/hassanmughal467/logo-design-portal/pull/9)

`ResetSuperAdminPasswordAsync` reset the SuperAdmin to a fixed source-visible password `"SuperAdmin@123"`.

**Fix:** Now generates a 24-character random Base64 password via `RandomNumberGenerator.GetBytes(18)` on every invocation. The generated password is logged server-side only (WARNING level) and never included in the API response body. The response body tells the operator to check the server logs.

---

## Methodology notes

- Backend review: read every controller's `[Authorize]`/`[AllowAnonymous]` attributes, `AuthController`, `AuthService`, `JwtTokenService`, the custom `RequirePermission` filter, rate-limiting middleware, forwarded-headers configuration, and CORS policy.
- Spot-checked for IDOR: `PaymentsController`, `FilesController`, `UsersController` — confirmed ownership checks are correctly applied elsewhere (e.g. `FileService.VerifyOrderUploadAccessAsync`, `UsersController`'s self-or-admin checks).
- Frontend review: token storage (`auth.service.ts`), `TokenInterceptor`'s 401/refresh handling, route guards (`AuthGuard`, `RoleGuard`, `PermissionGuard`) — confirmed these are UI-layer only and the real enforcement is server-side, as it should be.
- All "Fixed" findings were verified with new unit tests before merge, not just patched and assumed correct; see the linked PRs for test details.
