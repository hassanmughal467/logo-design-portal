# Post-Deploy Validation

Complete within **30 minutes** of production deploy.

## Automated checks

```bash
curl -f https://api.<domain>/health/live
curl -f https://api.<domain>/health/ready
```

Expected ready checks: `database`, `hangfire`, `file_storage` (and `redis` if configured).

**Do not** expect Swagger:

```bash
curl -o /dev/null -w "%{http_code}" https://api.<domain>/swagger/index.html
# Expected: 404
```

## Authentication

- [ ] Login from `https://admin.<domain>` with production admin account
- [ ] Cookie set (`HttpOnly`, `Secure`)
- [ ] Mutating request succeeds with CSRF header (browser devtools)
- [ ] Logout clears session
- [ ] Refresh works before access token expiry

## Authorization smoke

- [ ] Client user sees only own orders
- [ ] Designer cannot open `/api/invoices/{id}` for client invoice (404)
- [ ] SuperAdmin can access admin functions

## Core workflows

- [ ] List orders (admin)
- [ ] Assign designer to order
- [ ] Upload file on order (within size limit)
- [ ] Download file as authorized user
- [ ] Create invoice (admin) — if billing enabled
- [ ] SignalR notification appears on order update (browser console / UI badge)

## Security spot checks

- [ ] API error responses do not contain stack traces
- [ ] CORS: preflight from admin origin succeeds; random origin fails
- [ ] `/hangfire` requires auth (401/403 anonymous)

## Operations

- [ ] Serilog writing to configured sink
- [ ] No repeated exceptions in first 100 log lines
- [ ] Hangfire dashboard shows recurring jobs scheduled
- [ ] Disk space on file storage volume > 20% free

## Sign-off

| Check | Pass | Fail | Notes |
|-------|------|------|-------|
| Health | | | |
| Auth | | | |
| Orders | | | |
| Security | | | |

If any **Fail** → initiate `ROLLBACK_PROCEDURES.md`.
