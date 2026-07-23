# Rollback Procedures

## Decision criteria

Rollback if any of:

- `/health/ready` unhealthy > 10 minutes after deploy
- Authentication failure rate > 5% for 15 minutes
- Data corruption suspected from migration
- Critical security regression discovered

## Application rollback (fast — &lt; 15 min)

1. Stop IIS app pool (optional, reduces errors during swap)
2. Restore previous API publish folder from backup:
   - `C:\inetpub\LogoDesignPortal\api_backup_<date>`
3. Restore previous frontend `wwwroot` from backup
4. Recycle app pool
5. Verify `/health/live` and login
6. **Do not** re-run new migration on old binaries if schema incompatible — see DB rollback

## Database rollback

| Scenario | Action |
|----------|--------|
| Migration failed mid-flight | Restore MySQL from pre-deploy `mysqldump` |
| Migration applied, app rolled back | If new columns required by old app — **must restore DB** or forward-fix |
| Data-only issue | Point-in-time restore; contact DBA |

**Always** take backup before migration:

```bash
mysqldump -u user -p LogoDesignPortalDb > backup_YYYYMMDD_HHMM.sql
```

## Redis / Hangfire

- Redis: rollback usually **not** required for app version rollback
- Clear poison Hangfire jobs if failed recurring jobs spike after rollback

## Configuration rollback

- Restore previous IIS environment variable set (document before change)
- Revert `appsettings.Production.json` only if committed change caused issue

## Post-rollback

1. Incident record per `INCIDENT_RESPONSE_GUIDE.md`
2. Root cause analysis within 48h
3. Block re-deploy until fix + tests added

## Testing rollback path (staging)

Quarterly exercise:

1. Deploy build N+1 to staging
2. Restore build N from folder backup
3. Confirm health + login — document duration
