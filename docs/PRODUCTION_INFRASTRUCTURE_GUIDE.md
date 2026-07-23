# Production Infrastructure Guide (Week 3)

## Current deployment model

- **IIS** hosting ASP.NET Core API
- **Angular** static frontend (separate site or CDN)
- **MySQL** database
- **Local or NAS** file storage
- **Redis** optional but required for scale-out

## Readiness scorecard

| Area | Score (1–5) | Notes |
|------|-------------|-------|
| Horizontal API scaling | 4 | Redis/SignalR/Hangfire wired; files are blocker |
| Database scaling | 3 | Indexes improved; read replica not documented |
| Cache | 4 | Epoch strategy; production fallback fixed |
| Files | 2 | Local disk |
| Observability | 4 | Week 2 Serilog/OTel |
| CI/CD | 4 | See CI_CD_SETUP.md |
| DR | 2 | Plan documented; automate backups |

**Overall infrastructure readiness: 3.5 / 5**

## Reverse proxy (IIS / ARR)

- Enable WebSocket for SignalR
- Request timeout ≥ 120s for large uploads
- Forward `X-Forwarded-For` / `X-Forwarded-Proto`

## Docker readiness (recommended path)

1. Multi-stage Dockerfile for API
2. Nginx or Traefik ingress with TLS
3. Env vars for secrets (no keys in image)
4. Volume only for dev; prod uses S3

## CDN

- Front static Angular `dist/`
- Optional CDN for approved logo downloads (signed URLs)

## Secrets

See `SECRET_MANAGEMENT_GUIDE.md` — `Jwt__Key`, `ConnectionStrings__*`, SMTP.
