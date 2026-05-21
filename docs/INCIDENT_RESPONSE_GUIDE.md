# Incident Response Guide

## Severity levels

| Level | Example | Response time |
|-------|---------|---------------|
| SEV1 | Production down, data breach | 15 min |
| SEV2 | Auth broken, payments failing | 1 h |
| SEV3 | Degraded performance, single feature | 4 h |
| SEV4 | Minor bug, workaround exists | Next sprint |

## First 15 minutes (SEV1/SEV2)

1. **Acknowledge** — on-call assigns incident commander
2. **Assess** — `/health/ready`, logs, recent deploy?
3. **Mitigate** — rollback (`ROLLBACK_PROCEDURES.md`) or kill switch (`PRODUCTION-SAFETY-KILL-SWITCH.md`)
4. **Communicate** — status to stakeholders
5. **Preserve** — log exports, DB snapshot if data issue suspected

## Security incident

- Rotate `Jwt__Key` (forces re-login)
- Invalidate refresh tokens in DB if breach suspected
- Review audit logs (`AuditLogService`)
- Do not delete logs before forensic copy

## Data incident

- Stop writes (maintenance mode / kill switches)
- Restore from backup per `BACKUP_STRATEGY.md`
- Document records affected

## Post-incident (within 48h)

- Blameless postmortem
- Action items with owners
- Add regression test if applicable

## Contacts (fill in operations)

| Role | Contact |
|------|---------|
| On-call engineering | |
| DBA / hosting | |
| Security | |
| Product owner | |

## Useful commands

```bash
curl https://api.<domain>/health/ready
# IIS: restart app pool, review api\logs\stdout_*.log
```
