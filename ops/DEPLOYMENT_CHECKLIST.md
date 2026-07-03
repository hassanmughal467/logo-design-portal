# Hawk Merchandising Portal — Deployment Checklist

Manual IIS deployments using versioned releases and junction swaps. Scripts live in `ops/`.

## Server layout

```
C:\Sites\HawkPortal\
  shared\appsettings.Production.json   # secrets & env-specific config (never in ZIP)
  releases\{version}\                  # extracted API artifacts
  current\                             # junction → active release

C:\Sites\HawkPortalFrontend\
  releases\{version}\                  # Angular static files
  current\                             # junction → active release

ops\artifacts\                         # downloaded CI ZIPs before deploy
  api-{version}.zip
  frontend-{version}.zip               # optional if using ZIP instead of dist/
```

Point each IIS site physical path at the `current` junction folder.

---

## Pre-deploy (15 min)

- [ ] Confirm release tag in GitHub (`v*.*.*`) and download artifacts from the **Release — Publish deploy artifacts** workflow
- [ ] Rename or verify artifact names match script expectations:
  - API: `api-{version}.zip` (e.g. `api-1.2.3.zip`)
  - Frontend: `frontend-{version}.zip` or have Angular `dist/` ready
- [ ] Place API ZIP in `ops\artifacts\`
- [ ] Verify `C:\Sites\HawkPortal\shared\appsettings.Production.json` is up to date (connection strings, JWT, CORS, storage)
- [ ] Run database backup (AWS CLI or your standard procedure)
- [ ] Note the **currently active version** (read `current` junction target or last deploy log)
- [ ] Confirm disk space on `C:\Sites\` (> 2 GB free recommended)
- [ ] Announce maintenance window if needed

---

## Deploy API

From an elevated PowerShell session on the server:

```powershell
cd C:\path\to\repo\ops
.\deploy.ps1 -Version "1.2.3"
```

The script will:

1. Extract `artifacts\api-1.2.3.zip` → `releases\1.2.3\`
2. Copy shared `appsettings.Production.json` into the release
3. Stop IIS site → swap `current` junction → start IIS site
4. Run smoke test automatically
5. **Auto-rollback** to the previous release if smoke test fails
6. Prune releases older than the last 3

Optional: set `$ApiBaseUrl` at the top of `deploy.ps1` if IIS binding auto-detection is unreliable.

---

## Deploy frontend

After API is healthy:

```powershell
# From extracted dist folder
.\deploy-frontend.ps1 -Version "1.2.3" -DistPath "D:\builds\logo-design-portal-frontend"

# Or from ZIP in artifacts\
.\deploy-frontend.ps1 -Version "1.2.3"
```

No smoke test runs for static files. Verify the site loads in a browser after deploy.

---

## Smoke test (manual)

`deploy.ps1` runs this automatically. To re-run manually:

```powershell
.\smoke-test.ps1 -BaseUrl "https://api.yourdomain.com"
```

Checks:

| Endpoint | Expected |
|----------|----------|
| `GET /health/live` | HTTP 200 |
| `GET /health/ready` | HTTP 200 |
| `GET /api/system/health` | HTTP non-5xx (200 with DB/SignalR/storage status) |

**PASS** → proceed to post-deploy verification.  
**FAIL** → see rollback decision below.

---

## Rollback decision

| Situation | Action |
|-----------|--------|
| Smoke test fails during `deploy.ps1` | Script auto-rolls back to the previous release |
| Smoke test fails within **10 minutes** of deploy (manual discovery) | Run `.\deploy.ps1 -Rollback` immediately |
| Partial failure (API bad, frontend OK) | Roll back API only; leave frontend or roll back both if API contract changed |
| Smoke passes but functional bug found later | `.\deploy.ps1 -Rollback` for API; `.\deploy-frontend.ps1` with previous version's dist/ZIP for frontend |

Manual API rollback:

```powershell
.\deploy.ps1 -Rollback
```

Prints which version was restored (second newest release folder by `LastWriteTime`).

---

## Post-deploy verification (30 min)

See also `docs/POST_DEPLOY_VALIDATION.md`.

- [ ] `.\smoke-test.ps1` returns **PASS**
- [ ] Frontend loads at production URL (no blank page / 404 on routes)
- [ ] Admin login succeeds (cookies `HttpOnly`, `Secure`)
- [ ] CSRF header accepted on a mutating request
- [ ] List orders, upload a file, confirm SignalR notification
- [ ] Check Serilog / application logs for exceptions in first 100 lines
- [ ] Hangfire dashboard accessible to authorized users
- [ ] CORS preflight from admin origin succeeds

---

## First deploy (greenfield)

When IIS sites or folder structure do not exist yet:

1. Create `C:\Sites\HawkPortal\shared\` and place `appsettings.Production.json`
2. Create `C:\Sites\HawkPortal\releases\` and `C:\Sites\HawkPortalFrontend\releases\`
3. Run `deploy.ps1 -Version "{version}"` — scripts skip IIS stop/start if the site is missing
4. Create IIS sites pointing at `current` junctions (after first deploy creates them)
5. Re-run deploy or start sites manually in IIS Manager

On first deploy there is no previous release; if smoke test fails, manual fix is required (no auto-rollback target).

---

## Troubleshooting

| Problem | Check |
|---------|-------|
| Junction swap fails | Run PowerShell as Administrator; ensure `current` is a junction not a real folder |
| Smoke test 502/503 | App pool started? `appsettings.Production.json` present in release? |
| `/health/ready` fails | Database reachable; migrations applied; file storage path writable |
| Wrong version active | `cmd /c dir C:\Sites\HawkPortal\current` — junction shows target |
| Artifact not found | ZIP name must be `api-{version}.zip` in `ops\artifacts\` |

---

## Sign-off

| Step | Done | By | Time |
|------|------|----|------|
| Pre-deploy checks | | | |
| API deploy | | | |
| Smoke test PASS | | | |
| Frontend deploy | | | |
| Post-deploy validation | | | |
