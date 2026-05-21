# Order, File & Auth/Permission Module — QA Reference

**Module scope:** Order lifecycle (`OrderService`, `OrdersController`), file handling (`FileService`, `FilesController`), JWT/cookie auth, `[RequirePermission]`, `OrderStatusStateMachine`, role masking.

**Canonical backend:** `Backend/src/`

---

## 1. Manual QA checklist

### Auth & session

| # | Step | Expected |
|---|------|----------|
| A1 | Login as Client (HTTPS API + SPA with `useCookieAuth`) | 200; `ldp_access` httpOnly cookie set; user lands on dashboard |
| A2 | Refresh page while logged in | Session restored from cookie + `sessionStorage` user profile |
| A3 | Logout | `POST /api/auth/logout`; cookies cleared; redirected to login |
| A4 | Mutating API without `X-XSRF-TOKEN` (cookie mode) | 403 CSRF (except login/register/refresh) |
| A5 | Login wrong password 5+ times | Lockout message; 401 |

### Order creation (Client)

| # | Step | Expected |
|---|------|----------|
| O1 | Create order with title, description, price | 201; status `WaitingForAdminApproval` |
| O2 | View order as Client | No designer name/email; `assignedDesignerDisplayName` = "Company Design Team" if shown |
| O3 | Create order without auth | 401 |
| O4 | Designer calls `POST /api/orders` | 403 (role) |

### Admin workflow

| # | Step | Expected |
|---|------|----------|
| O5 | Admin approves order → assigns designer | Status `InProgress`; designer sees order on assigned list |
| O6 | Designer uploads preview (not visible to client) | Files saved; client file list does not show preview |
| O7 | Admin send preview batch to client | Status `PreviewDelivered`; client sees files |
| O8 | Client requests revision | Status `RevisionRequested` |
| O9 | Client approves logo | `ClientApproved` or admin path to `Completed` per role |

### Files

| # | Step | Expected |
|---|------|----------|
| F1 | Client uploads reference (allowed extension) | 200; `IsVisibleToClient` true |
| F2 | Client downloads own reference | 200 |
| F3 | Client attempts download of hidden preview GUID | **403** |
| F4 | Upload `.exe` or double extension `file.pdf.exe` | 400 invalid type |
| F5 | Upload PNG with PDF extension | 400 content mismatch |
| F6 | Upload when `AllowUploads=false` | 400 |
| F7 | Designer uploads to unassigned order | 403 |

### Permissions

| # | Step | Expected |
|---|------|----------|
| P1 | Designer `GET /api/orders` (all orders) | 403 (no `ViewAllOrders`) |
| P2 | Admin `GET /api/orders` | 200 (has permission) |
| P3 | Client assign designer | 403 |
| P4 | SuperAdmin assign designer | 200 |

### State machine

| # | Step | Expected |
|---|------|----------|
| S1 | Jump `WaitingForAdminApproval` → `Completed` via status API | 400 |
| S2 | Client cancel before work starts | `CancelledByUser` |
| S3 | Client cancel after `InProgress` | 400 |
| S4 | Admin cancel from `PreviewDelivered` | `CancelledByAdmin` |

---

## 2. Edge cases

- Order created with zero/negative price (validation).
- Concurrent status updates on same order (last write wins or conflict — document actual behavior).
- Duplicate file upload (same name/size within 1h) — rejected.
- Preview batch partial send (subset of files).
- Manual completed order backdated `CompletedAt`.
- Refresh token expired while cookie access still valid — refresh flow.
- SignalR reconnect after cookie session refresh.
- Client with `UpdateOrderStatus` permission but role guard only allows specific statuses.
- Empty multipart upload.
- 101 files in one multipart (combined size).
- Order locked statuses (`Completed`, `Cancelled*`) — uploads blocked.

---

## 3. Failure scenarios

| Scenario | Symptom | Mitigation |
|----------|---------|------------|
| Redis down in prod | Rate limit / SignalR degraded | Health `/health/ready`; fallback docs |
| File storage disk full | 500 on upload | Monitor disk; alert |
| DB migration pending | 500 on startup | `database_schema` health check |
| CORS misconfig with cookies | Login works but API 401 | Align `Cors:AllowedOrigins` + credentials |
| Missing role permissions seed | 403 on Client create order | Run DB init / `EnsureDefaultRolePermissionsAsync` |
| ProductionSafety `DisableFileUploads` | 400 all uploads | Ops kill-switch |

---

## 4. Security vulnerabilities (test focus)

| ID | Risk | Test coverage |
|----|------|----------------|
| SEC-F-01 | IDOR file download by GUID | Integration `FilesControllerDownloadSecurityTests` |
| SEC-F-02 | Path traversal in filename | `FileServiceUploadAbuseTests` |
| SEC-F-03 | MIME/extension spoofing | Upload abuse tests |
| SEC-A-01 | JWT in localStorage (legacy) | Cookie mode E2E |
| SEC-A-02 | CSRF on cookie session | CSRF middleware + E2E |
| SEC-P-01 | Permission bypass via role only | Authorization integration tests |
| SEC-M-01 | Designer/client PII in JSON | Privacy integration tests |

---

## 5. Permission abuse cases

| Actor | Abuse attempt | Expected |
|-------|---------------|----------|
| Client | `GET /api/orders` (all) | 403 |
| Client | `POST assign` | 403 |
| Client | Download other client's file | 403 |
| Designer | Upload to another designer's order | 403 |
| Designer | View client email on order DTO | `Client` null / masked |
| Admin without `AssignOrder` (if revoked) | Assign fails 403 | Seed-dependent |
| Anonymous | Any protected route | 401 |

---

## 6. Invalid workflow transitions (state machine)

| From | To | Allowed? |
|------|-----|----------|
| WaitingForAdminApproval | Completed | No |
| WaitingForAdminApproval | CancelledByUser | Yes (client) |
| InProgress | Completed | No (use preview path) |
| PreviewDelivered | InProgress | No |
| Completed | InProgress | No |
| Completed | Refunded | Yes (admin) |
| Refunded | Any | No (terminal) |

Enforced in: `UpdateOrderStatusAsync`, `OrderStatusTransitionHelper`, `RevisionService`, `CancelOrderAsync`.

---

## 7–10. Automated tests

See `docs/TESTING_ORDER_FILE_AUTH_MODULE.md` for file paths, naming, CI wiring, and setup.
