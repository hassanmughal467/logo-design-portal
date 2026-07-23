# Release Process

## Roles

| Role | Responsibility |
|------|----------------|
| Developer | Feature complete, tests, PR |
| Reviewer | Code + security review |
| QA | Staging UAT per module QA docs |
| Deployer | IIS deploy, migrations, smoke |
| Owner | Go/no-go for production |

---

## Release cadence

- **Staging:** continuous or weekly from `develop`
- **Production:** tagged releases `vMAJOR.MINOR.PATCH` after staging sign-off

---

## Standard release steps

### 1. Prepare

- [ ] All PRs merged to release branch
- [ ] `CHANGELOG` / release notes drafted
- [ ] DB migration scripts reviewed (`PRODUCTION-MIGRATION-SAFETY.md`)
- [ ] Kill switches (`ProductionSafety`) verified for production intent

### 2. Build

```powershell
git tag v1.2.3
git push origin v1.2.3
```

Triggers `deploy-artifacts.yml` (or run workflow_dispatch).

### 3. Stage

- Deploy artifacts to **staging** (use staging frontend build if validating UI)
- Execute `STAGING_DEPLOYMENT_CHECKLIST.md`
- Run regression: integration tests + targeted Playwright + manual UAT

### 4. Production window

- Announce maintenance if migrations required
- Backup MySQL + `Files` volume
- Deploy production artifacts
- Run migrations
- Execute `DEPLOYMENT_CHECKLIST.md` + `POST_DEPLOY_VALIDATION.md`

### 5. Post-release

- Monitor Serilog / IIS logs 60 minutes
- Confirm Hangfire jobs processing
- Document incidents in `INCIDENT_RESPONSE_GUIDE.md`

---

## Hotfix process

1. Branch from production tag
2. Minimal fix + tests
3. Fast-track staging smoke (abbreviated checklist)
4. Production deploy with rollback zip ready
5. Forward-merge to `main`/`develop`

---

## Versioning

- **MAJOR:** breaking API/contracts
- **MINOR:** features
- **PATCH:** fixes only

Frontend and API versions should be tagged together for traceability.

---

## Communication template

```
Release v1.2.3 — [date/time UTC]
Scope: [summary]
Staging validated: [yes/no, link to checklist]
Migration: [yes/no]
Rollback owner: [name]
```
