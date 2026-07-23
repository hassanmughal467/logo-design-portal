# CI/CD Setup

## Workflows

| Workflow | Trigger | Purpose |
|----------|---------|---------|
| `.github/workflows/test.yml` | push/PR `main`, `develop` | Full backend matrix tests, frontend Karma, production builds, Playwright smoke |
| `.github/workflows/pr-validation.yml` | pull_request | Fast gates: format, build, unit tests, frontend typecheck |
| `.github/workflows/e2e-playwright.yml` | path filters / manual | Full E2E with MySQL service |
| `.github/workflows/deploy-artifacts.yml` | tags `v*.*.*` / manual | Publish API + Frontend zip artifacts for IIS deploy |

## Backend pipeline

1. `dotnet restore` / `dotnet format --verify-no-changes`
2. `dotnet build -c Release`
3. `dotnet test` per project with Coverlet (`coverlet*.runsettings`, **80% line threshold** on Domain + Application)
4. Integration job enforces Cobertura `line-rate >= 0.80`

## Frontend pipeline

1. `npm ci`
2. `npx tsc -p tsconfig.app.json --noEmit`
3. `npm run test:ci` (Karma headless)
4. `ng build --configuration=production`
5. Playwright smoke: `login`, `unauthorized`, `rbac-and-token`

## Analyzers and nullable

- `Backend/Directory.Build.props` — nullable enabled, `latest-minimum` analyzers
- Gradual adoption: `TreatWarningsAsErrors` documented for Week 2 after baseline cleanup
- `Backend/.editorconfig` — formatting rules for `dotnet format`

## Recommended branch protection (GitHub)

**`main` and `develop`:**

- Require PR before merge
- Require status checks: `Backend (integration)`, `Frontend (Karma + coverage gates)`, `Build verification`, `PR Validation / backend-quality`
- Require branches up to date
- Do not allow bypass for admins (production)
- Restrict force pushes

## Artifacts

- `backend-coverage-*` — Cobertura per matrix job
- `frontend-coverage` — Karma output
- `playwright-report` — on E2E workflows
- `LogoDesignPortal-API` / `LogoDesignPortal-Frontend` — release zips from `deploy-artifacts.yml`

### Release deploy (manual IIS)

1. Run workflow **Release — Publish deploy artifacts** (or push tag `v1.2.3`)
2. Download artifacts from Actions
3. Unzip API to `C:\inetpub\LogoDesignPortal\api`, Frontend to `wwwroot`
4. Set IIS env vars; run `docs/POST_DEPLOY_VALIDATION.md`

## Local parity

```powershell
cd Backend
dotnet format LogoDesignPortal.sln --verify-no-changes
dotnet build LogoDesignPortal.sln -c Release
dotnet test LogoDesignPortal.sln -c Release

cd ..\Frontend
npm ci
npx tsc -p tsconfig.app.json --noEmit
npm run test:ci
npx ng build --configuration=production
```
