# Secret Management Guide

## Status: migration-ready (env-first)

Secrets must **never** appear in committed `appsettings.*.json` files for Staging/Production. Development uses **.NET User Secrets** (`UserSecretsId: logo-design-portal-api-dev`).

---

## Inventory

| Secret | Storage today | Target |
|--------|---------------|--------|
| JWT signing key | `Jwt__Key` env / User Secrets | Key Vault / Secrets Manager |
| MySQL password | `ConnectionStrings__DefaultConnection` | Same |
| Redis password | `ConnectionStrings__Redis` | Same |
| SMTP password | `Email__SmtpPassword` | Same |
| Payment API keys | Not in repo | Provider sandbox (staging) / live (prod) |
| E2E CI passwords | GitHub Actions env | OIDC + short-lived test users |

---

## Fail-fast protections

| Validator | Blocks |
|-----------|--------|
| `ProductionSecretsValidator` | Placeholder JWT, missing JWT, exception details in prod/staging |
| `EnvironmentConfigurationValidator` | Placeholder DB strings, wrong `FileStorage` path |

---

## Recommended rollout

### Phase 1 — Environment variables (current)

- IIS `<environmentVariables>` or systemd `Environment=`
- GitHub Actions secrets for deploy pipelines
- Document every key in `ENVIRONMENT_VARIABLE_REFERENCE.md`

### Phase 2 — Managed secret store

Choose one:

| Platform | Integration |
|----------|-------------|
| **Azure Key Vault** | `AddAzureKeyVault` in `Program.cs` (optional package) |
| **AWS Secrets Manager** | `Amazon.Extensions.Configuration.SecretsManager` |
| **Doppler** | Sync to env at deploy time |

### Phase 3 — CI OIDC

- GitHub OIDC → cloud IAM → fetch secrets at deploy
- No long-lived PATs on build agents

---

## Development setup

```powershell
cd Backend\src\LogoDesignPortal.API
dotnet user-secrets set "Jwt:Key" "<random-32+-char-key>"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=127.0.0.1;Port=3306;Database=LogoDesignPortalDb;User=root;Password=<local>;"
```

Optional: `ConnectionStrings:Redis`, `Email:SmtpPassword`

---

## Rotation

| Secret | Cadence | Procedure |
|--------|---------|-----------|
| JWT | 90 days or on incident | Set new `Jwt__Key`, deploy API, all users re-login |
| DB password | Annual / on compromise | Update MySQL user, update env, restart pool |
| SMTP | On provider rotation | Update `Email__*` env vars |

---

## Anti-patterns (do not)

- Commit `Password=ADMIN` or sample app passwords
- Share production JWT key with staging
- Store secrets in `web.config` in git — use deploy-time injection only
- Disable validators to “make it start”

---

## Scanning

- Enable GitHub **secret scanning** and push protection
- Add **gitleaks** to `pr-validation.yml` (recommended)
- Review `appsettings.Development.json` in PRs for accidental literals

---

## Verification

```powershell
dotnet test Backend/src/LogoDesignPortal.API.IntegrationTests `
  --filter "FullyQualifiedName~ProductionConfiguration"
```

Ensure production publish output contains **no** `appsettings.Testing.json` (`.csproj` `CopyToPublishDirectory: Never`).
