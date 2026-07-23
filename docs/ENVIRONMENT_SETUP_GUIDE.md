# Environment Setup Guide

Canonical paths: **`Backend/src/`** only. Legacy `Backend/LogoDesignPortal.API/` is out of solution.

---

## Environment matrix

| Environment | `ASPNETCORE_ENVIRONMENT` | Angular config | Upload path |
|-------------|------------------------|----------------|-------------|
| Local development | `Development` | default / `development` | `Files_Dev` |
| Automated testing | `Testing` | `testing` / `e2e` | `Files_Test` |
| Staging | `Staging` | `staging` | `Files_Staging` |
| Production | `Production` | `production` | `Files` |

---

## Local development

### Backend

```powershell
cd Backend\src\LogoDesignPortal.API
dotnet user-secrets set "Jwt:Key" "<random-32+-chars>"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=127.0.0.1;Port=3306;Database=LogoDesignPortalDb;User=root;Password=<your-local-password>;"
# Optional Redis parity:
dotnet user-secrets set "ConnectionStrings:Redis" "localhost:6379"
dotnet run
```

- Swagger: `https://localhost:44398/swagger` (or launchSettings URL)
- Redis optional (`Scalability:AllowInMemoryFallback: true` in Development)

### Frontend

```powershell
cd Frontend
npm install
npm start
```

Default API: `https://localhost:44398` (`environment.development.ts`). Align Kestrel/IIS Express port in `launchSettings.json` if needed.

---

## Automated testing (backend)

- `ASPNETCORE_ENVIRONMENT=Testing` — set by `TestWebApplicationFactory`
- SQLite / in-memory patterns in integration tests
- `appsettings.Testing.json` **not published**

```powershell
cd Backend
dotnet test src/LogoDesignPortal.API.IntegrationTests
```

### Frontend / E2E

```powershell
cd Frontend
npm run build:testing   # or serve --configuration=e2e
```

Playwright env: see `Frontend/e2e/.env.example` (`E2E_API_URL`, `E2E_BASE_URL`).

---

## Staging

See **`STAGING_SETUP_GUIDE.md`** and **`STAGING_DEPLOYMENT_CHECKLIST.md`**.

```powershell
cd Frontend
npm run build:staging
```

---

## Production

See **`PRODUCTION_DEPLOYMENT_GUIDE.md`** and **`DEPLOYMENT_CHECKLIST.md`**.

```powershell
cd Frontend
npm run build:production
```

---

## Configuration validation

Startup validators (Staging/Production):

- `ProductionSecretsValidator`
- `EnvironmentConfigurationValidator`

```powershell
dotnet test Backend/src/LogoDesignPortal.API.IntegrationTests -c Release `
  --filter "FullyQualifiedName~EnvironmentAppSettings|FullyQualifiedName~ProductionConfiguration"
```

---

## Related docs

| Doc | Topic |
|-----|-------|
| `ENVIRONMENT_AUDIT_REPORT.md` | Audit findings |
| `ENVIRONMENT_VARIABLE_REFERENCE.md` | All env vars |
| `SECRET_MANAGEMENT_GUIDE.md` | Secrets |
| `DEPLOYMENT_WORKFLOW.md` | CI/CD |
| `IIS_HARDENING_GUIDE.md` | IIS |
