# Release Runbook

## Roles

| Role | Responsibility |
|------|----------------|
| Release engineer | Execute deploy, migrations |
| QA | Post-deploy validation |
| On-call | Monitor 48h |

## Timeline (standard release)

| T-24h | Freeze release branch; CI green |
| T-4h | Notify stakeholders; confirm maintenance window if needed |
| T-1h | DB backup; record current app version |
| T-0 | Deploy per `DEPLOYMENT_CHECKLIST.md` |
| T+15m | `POST_DEPLOY_VALIDATION.md` |
| T+48h | Review logs, error rates, Hangfire failures |

## Release steps

1. **Prepare**
   - Tag release: `vX.Y.Z`
   - Verify `CHANGELOG` / release notes
   - Run full test suite on tag

2. **Database**
   - If schema change: generate SQL from EF migration
   - Review `docs/PRODUCTION-MIGRATION-SAFETY.md`
   - Apply migration **before** or **during** deploy (single writer — one instance)

3. **API deploy**
   ```powershell
   dotnet publish Backend/src/LogoDesignPortal.API -c Release -o C:\inetpub\LogoDesignPortal\api
   ```
   - Set IIS env vars (unchanged unless rotating secrets)
   - Recycle app pool

4. **Frontend deploy**
   ```powershell
   cd Frontend
   npm ci
   ng build --configuration production
   ```
   - Copy `dist/logo-design-portal-frontend/*` to wwwroot
   - Copy `web.config`

5. **Validate** — `POST_DEPLOY_VALIDATION.md`

6. **Communicate** — release complete

## Hotfix procedure

- Branch from production tag `vX.Y.Z-hotfix`
- Minimal fix + targeted tests
- Skip unrelated migrations
- Deploy API only if FE unchanged

## Emergency kill switches

`ProductionSafety` section (Staging/production appsettings):

- `DisableBillingGeneration`
- `DisableDesignerPayout`
- `DisableFileUploads`
- `DisableInvoiceEditing`

See `docs/PRODUCTION-SAFETY-KILL-SWITCH.md`.

## No deploy pipeline (current state)

Releases are **manual** via `deploy-to-iis.ps1` or IIS guide.
**Recommendation:** Add GitHub Actions artifact publish workflow in Month 1 post-launch.
