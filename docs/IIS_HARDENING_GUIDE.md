# IIS Hardening Guide

Applies to **API** (`LogoDesignPortal.API`) and **Frontend** (Angular SPA) sites on Windows Server / IIS 10+.

---

## API site (`web.config`)

### Current baseline (`Backend/src/LogoDesignPortal.API/web.config`)

| Setting | Value | Notes |
|---------|-------|-------|
| `maxAllowedContentLength` | 524288000 (500 MB) | Matches multipart upload limits |
| `hostingModel` | inprocess | Standard for ASP.NET Core Module V2 |
| `ASPNETCORE_ENVIRONMENT` | Production | Override to `Staging` on staging site |

### Required additions (per site)

```xml
<webSocket enabled="true" />
```

SignalR requires WebSocket protocol enabled in IIS Features.

### Environment variables

Set in `<aspNetCore><environmentVariables>` — never commit real values:

- `ConnectionStrings__DefaultConnection`
- `ConnectionStrings__Redis`
- `Jwt__Key`

Template: `web.config.staging.example.xml`

---

## HTTPS enforcement

- Bind valid TLS certificate on all public bindings
- Enable **Require SSL** on IIS site
- ASP.NET Core `UseHttpsRedirection` active outside Development
- HSTS via `SecurityHeadersMiddleware` (verify in staging)

---

## Upload limits

| Layer | Limit |
|-------|-------|
| IIS `maxAllowedContentLength` | 500 MB (configured) |
| Kestrel / `FormOptions` | `UploadLimits.MaxMultipartBytes` |
| Application | `UploadSecurityHelper` type/size rules |

Align all three when changing limits.

---

## Security headers

Verify API returns (via middleware):

- `X-Content-Type-Options`
- `X-Frame-Options` / CSP as configured
- `Referrer-Policy`
- HSTS in production

Frontend static site: add headers via IIS `web.config` or URL Rewrite outbound rules if not served behind CDN.

---

## Compression

- Enable dynamic + static compression for SPA assets (`.js`, `.css`)
- Exclude already compressed formats

---

## Static file safety (frontend)

- Serve only from `dist/` — no directory browsing
- `web.config` SPA fallback to `index.html` (existing)
- Do not expose `environment*.ts` or source maps in production builds (`outputHashing: all`)

---

## Request limits

- `maxAllowedContentLength` for uploads
- Rate limiting enforced at API (Redis in Staging/Production)
- Consider IIS `requestFiltering` deny rules for suspicious paths (`/.env`, `/wp-admin`, etc.)

---

## App Pool hardening

| Setting | Recommendation |
|---------|----------------|
| Identity | Dedicated service account (not LocalSystem) |
| Permissions | Write only to `Files*` and `logs` |
| .NET CLR | No Managed Code (ANCM hosts Core) |
| Idle timeout | Default; avoid aggressive recycle during uploads |
| Rapid fail protection | Enable |

---

## Logging

- `stdoutLogEnabled=true` for initial deploy debugging — disable after stable
- Centralize Serilog to file/agent in production
- Rotate `logs\stdout` and application logs

---

## Checklist

- [ ] WebSockets enabled
- [ ] TLS 1.2+ only
- [ ] Secrets in env vars only
- [ ] App Pool least privilege
- [ ] 500 MB upload limit intentional
- [ ] Staging uses separate site + `Files_Staging`
- [ ] Production `AllowedHosts` matches real hostnames

---

## References

- `PRODUCTION_INFRASTRUCTURE_GUIDE.md`
- `SIGNALR_SCALING_GUIDE.md`
- `FILE_UPLOAD_SECURITY.md`
