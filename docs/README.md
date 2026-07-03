# Documentation Index

This folder contains both the **canonical documentation set** (maintained, verified against the codebase) and a large number of **historical reports and guides** from earlier project phases. When in doubt, trust the canonical set and the source code.

## Canonical documents (start here)

| Document | Contents |
|---|---|
| [../AGENTS.md](../AGENTS.md) | Rules for engineers and AI agents; golden rules; AI Agent Rules |
| [../README.md](../README.md) | Project overview, stack, local setup |
| [architecture.md](architecture.md) | Layers, middleware pipeline, auth, infrastructure, environment differences |
| [module-map.md](module-map.md) | Backend + frontend module map with routes and roles |
| [api.md](api.md) | Full API route map with roles/permissions |
| [database.md](database.md) | Entities, tables, migrations, seeding, backups |
| [coding-rules.md](coding-rules.md) | Coding standards (C#, Angular, general) |
| [testing-plan.md](testing-plan.md) | Testing strategy, commands, CI gates |
| [deployment.md](deployment.md) | CI, release procedure, rollback, env variables, staging vs production |
| [incident-response.md](incident-response.md) | Severities, playbooks, contacts template |
| [PRODUCTION_READINESS_CHECKLIST.md](PRODUCTION_READINESS_CHECKLIST.md) | Pre-launch gates and sign-off |

## Supporting operational references (still current)

These are referenced by the canonical set and remain the detailed source for their topics:

- **Migrations**: `PRODUCTION-MIGRATION-SAFETY.md`, `../Backend/PRODUCTION_MIGRATION_GUIDE.md`, `../Backend/scripts/README-MIGRATION.md`
- **Deploy/release**: `DEPLOYMENT_WORKFLOW.md`, `RELEASE_PROCESS.md`, `ROLLBACK_GUIDE.md`, `POST_DEPLOY_VALIDATION.md`, `STAGING_DEPLOYMENT_CHECKLIST.md`, `../ops/DEPLOYMENT_CHECKLIST.md`
- **Incidents/ops**: `INCIDENT_RESPONSE_GUIDE.md`, `FAILURE_RECOVERY_GUIDE.md`, `OPERATIONS_RUNBOOK.md`, `PRODUCTION-TROUBLESHOOTING.md`, `PRODUCTION-SAFETY-KILL-SWITCH.md`, `../ops/BACKUP_RUNBOOK.md`, `../ops/redis-restart.md`, `DISASTER_RECOVERY_PLAN.md`
- **Monitoring**: `MONITORING_SETUP_GUIDE.md`, `OBSERVABILITY_SETUP.md`, `ALERTING_RECOMMENDATIONS.md`
- **Security**: `FILE_UPLOAD_SECURITY.md`, `SECURITY_TESTING_GUIDE.md`, `SECRET_MANAGEMENT_GUIDE.md`, `IIS_HARDENING_GUIDE.md`
- **Testing**: `TESTING_STANDARDS.md`, `QA-TESTING-GUIDELINE.md`, `PLAYWRIGHT_TESTING_GUIDE.md`, `LOAD_TESTING_PLAN.md`, module guides `TESTING_*_MODULE.md` / `QA_*_MODULE.md`
- **Config**: `ENVIRONMENT_VARIABLE_REFERENCE.md`, `ENVIRONMENT_SETUP_GUIDE.md`, `PRODUCTION_CONFIGURATION_GUIDE.md`, `REDIS_PRODUCTION_GUIDE.md`
- **Architecture deep-dives**: `architecture/` folder (SYSTEM_OVERVIEW, BACKEND/FRONTEND/DATABASE/SECURITY/TESTING architecture, tech-stack reference)

## Point-in-time reports (context only)

Audit reports, week-N readiness assessments, performance/QA snapshots, and improvement plans in this folder (e.g. `SECURITY_AUDIT_REPORT.md`, `WEEK4_LAUNCH_READINESS_ASSESSMENT.md`, `TECHNICAL_DEBT_REPORT.md`, `PERFORMANCE_OPTIMIZATION_REPORT.md`) describe the system **as it was at the time of writing**. Useful for history and rationale; do not treat them as current specifications.

## Archive

`archive/` holds the markdown reports that previously lived at the repository root (implementation summaries, early testing guides, phase plans, the DigitalOcean staging guide, etc.). They are kept for history only — several are known to be outdated (the DigitalOcean guide references SQL Server; the real stack is MySQL 8 on IIS). Do not follow them for current work, and do not add new reports to the repository root.
