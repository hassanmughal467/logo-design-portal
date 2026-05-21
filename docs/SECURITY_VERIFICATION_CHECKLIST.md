# Security Verification Checklist — Pre-Launch

Use before every production release. All items must be **checked** or **waived with sign-off**.

## Authentication & session

- [ ] `Jwt__Key` set via secret manager (≥32 chars, not placeholder)
- [ ] `ProductionSecretsValidator` passes at startup (no crash on boot)
- [ ] Cookie auth: `Secure=true`, HTTPS admin origin
- [ ] CSRF header sent on mutating requests from SPA (`useCookieAuth: true`)
- [ ] Refresh token rotation works; replay test passes in CI
- [ ] Account lockout / rate limits active in non-Development

## API surface

- [ ] Swagger returns 404 in Production (not Development env on server)
- [ ] No `[AllowAnonymous]` on sensitive mutations except webhooks + auth entrypoints
- [ ] PayPal webhook signature verification enabled
- [ ] Hangfire `/hangfire` requires Admin/SuperAdmin (non-Development)

## Authorization & privacy

- [ ] Client cannot read other tenant orders/invoices/files (integration privacy tests green)
- [ ] **Designer cannot read/list client billing invoices** (`InvoicesControllerPrivacyTests`)
- [ ] Designer cannot see client PII on orders (privacy tests)
- [ ] Payment mutations use `PaymentInvoiceAccessHelper`

## Uploads & files

- [ ] `UploadSecurityHelper` on all upload paths
- [ ] Multipart size limit matches IIS `web.config`
- [ ] Upload abuse integration tests pass
- [ ] Download 403 for cross-tenant file IDs

## SignalR

- [ ] Hub requires authentication
- [ ] WSS only in production
- [ ] `JoinUserGroup` cannot join another user's group

## Configuration

- [ ] `IncludeExceptionDetailsInProduction=false` (Production + Staging)
- [ ] `Scalability:AllowInMemoryFallback=false` in Production
- [ ] Redis connection configured (required)
- [ ] CORS origins trimmed to production HTTPS admin only
- [ ] `AllowedHosts` set to real hostnames (recommended)

## Logging & secrets

- [ ] No JWT/passwords in application logs
- [ ] Serilog sinks configured for production
- [ ] `appsettings.Development.json` passwords not used in prod
- [ ] SuperAdmin default password changed after first deploy

## Automated verification (CI)

```bash
dotnet test Backend/src/LogoDesignPortal.API.IntegrationTests --filter "FullyQualifiedName~Security|FullyQualifiedName~Privacy|FullyQualifiedName~Swagger|FullyQualifiedName~Upload"
```

```bash
cd Frontend && npm run e2e -- --grep "@security|upload-security"
```

## Sign-off

| Role | Name | Date | Notes |
|------|------|------|-------|
| Security | | | |
| Engineering | | | |
| Operations | | | |
