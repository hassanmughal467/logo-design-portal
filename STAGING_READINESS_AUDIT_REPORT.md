# Staging Readiness Audit Report
## Logo Design Portal — Mediated Workflow System

**Audit Date:** March 6, 2025  
**Tech Stack:** ASP.NET Core API, Angular, SQL Server, SignalR  
**Workflow:** CLIENT → ADMIN → DESIGNER → ADMIN → CLIENT (mediated; no direct client-designer contact)

---

## Executive Summary

A comprehensive module-by-module and role-by-role audit was performed on the Logo Design Portal codebase. The system implements the mediated workflow correctly in most areas: messaging routing, admin preview forwarding, file lifecycle, and identity masking in orders and messages. **One critical identity-masking gap was identified** in the file service that must be fixed before staging.

---

## 1. Module-by-Module Verification

### MODULE 1 — Authentication

| Check | Status | Evidence |
|-------|--------|----------|
| User login | ✅ | `AuthController` login endpoint, `JwtTokenService` issues tokens |
| JWT authentication | ✅ | `Program.cs` lines 62–97: Bearer auth, ValidateIssuer/Audience/Lifetime/SigningKey |
| Role-based access control | ✅ | `[Authorize(Roles = "...")]` and `[RequirePermission("...")]` on controllers |
| Clients cannot access admin endpoints | ✅ | Admin endpoints use `[Authorize(Roles = "SuperAdmin,Admin")]` or `[RequirePermission]` |
| Designers cannot access client-only APIs | ✅ | Client-only: `[Authorize(Roles = "Client")]` (e.g. `my-orders`, `CreateOrder`) |
| Unauthorized requests rejected | ✅ | `[Authorize]` on controllers; 401 for unauthenticated, 403 for forbidden |

**Verdict:** PASS

---

### MODULE 2 — Order Creation (Client)

| Check | Status | Evidence |
|-------|--------|----------|
| Client uploads reference files | ✅ | `CreateOrderWithFilesAsync` in `OrderService`, `PrepareReferenceFilesForOrderAsync` in `FileService` |
| Reference files stored correctly | ✅ | Reference → `{FileStorage:Path}` (root Files folder) |
| Files linked to correct OrderId | ✅ | `LogoFile.OrderId` set on creation |
| Admin receives notification | ✅ | `OrderCreated` SignalR event to Admin/SuperAdmin; `CreateNotificationForRoleAsync` |
| Single file upload | ✅ | `UploadFileAsync` supports single file |
| Multiple files upload | ✅ | `UploadMultipleFilesAsync`, `CreateOrderWithFilesAsync` |
| Invalid file type rejected | ✅ | `allowedExtensions` in `FileService`; throws `InvalidOperationException` |
| Oversized file rejected | ✅ | `MaxFileSize = 10MB`; throws if exceeded |

**Verdict:** PASS

---

### MODULE 3 — Admin Order Management

| Check | Status | Evidence |
|-------|--------|----------|
| Admin receives new orders | ✅ | `OrderCreated` SignalR, notifications to Admin/SuperAdmin |
| Admin assigns designer | ✅ | `POST /api/orders/{id}/assign` with `[RequirePermission("AssignOrder")]` |
| Designer receives assignment notification | ✅ | `SendOrderAssignedAsync` to designer's user group |
| Order status updates correctly | ✅ | Status → `InProgress` on assign |
| Client identity hidden from designer | ✅ | `GetOrderByIdAsync`, `GetOrdersByDesignerAsync`: `order.Client = null` |

**Verdict:** PASS

---

### MODULE 4 — Designer Workflow

| Check | Status | Evidence |
|-------|--------|----------|
| Designer sees assigned orders | ✅ | `GetAssignedOrders` → `GetOrdersByDesignerAsync` |
| Designer sees Order ID, project brief, reference files | ✅ | Order DTO includes Title, Description, Files; Designer has file access |
| Designer must NOT see client name/email/phone | ✅ | `Client = null` in order responses for Designer |
| Designer uploads preview files | ✅ | `UploadFileAsync` / `UploadMultipleFilesAsync` with `fileType=Preview` |
| PreviewBatchId generated | ✅ | `previewBatchId = Guid.NewGuid()` per designer preview session |
| Files stored as Preview type | ✅ | `FileType.Preview` → `_temporaryStoragePath` |

**Verdict:** PASS

---

### MODULE 5 — Admin Preview Review

| Check | Status | Evidence |
|-------|--------|----------|
| Admin forwards preview batch to client | ✅ | `SendPreviewBatchToClientAsync`, `SendFilesToClientAsync` |
| Admin must send entire preview batch | ✅ | `SendFilesToClientAsync` validates: partial delivery throws `InvalidOperationException` |
| Partial batch forwarding fails | ✅ | Unit test `SendFilesToClient_WithPartialPreviewBatch_ThrowsInvalidOperationException` passes |

**Verdict:** PASS

---

### MODULE 6 — Client Preview Review

| Check | Status | Evidence |
|-------|--------|----------|
| Client receives preview files | ✅ | `IsVisibleToClient = true` after admin forwards |
| Client must NOT see designer identity | ⚠️ **FAIL** | See MODULE 14 — Critical Bug |
| Client can approve design | ✅ | `ApproveLogoAsync` via RevisionsController |
| Client can request revision | ✅ | `RequestRevisionAsync` via RevisionsController |

**Verdict:** CONDITIONAL (blocked by identity masking bug)

---

### MODULE 7 — Revision Workflow

| Check | Status | Evidence |
|-------|--------|----------|
| 3 revision cycles supported | ✅ | `RevisionWorkflowTests.FullWorkflow_3Revisions_VerifiesAllFourRules` passes |
| Preview files deleted on revision | ✅ | `DeletePreviewFilesAsync` in `RevisionService.RequestRevisionAsync` |
| Reference files remain intact | ✅ | Only `FileType.Preview` and `UploadedBy == designerUserId` deleted |
| Revision files deleted | ✅ | `DeleteRevisionFilesAsync` removes previous revision files |

**Verdict:** PASS

---

### MODULE 8 — Final Approval

| Check | Status | Evidence |
|-------|--------|----------|
| Preview → Final conversion | ✅ | `ApproveLogoAsync` moves files to Permanent, sets `FileType = Final` |
| Final files stored permanently | ✅ | `_permanentStoragePath`; `File.Move` from Temporary to Permanent |
| Client can download final files | ✅ | `DownloadFileAsync` allows Client for own order files; Final always visible |

**Verdict:** PASS

---

### MODULE 9 — File Storage

| Check | Status | Evidence |
|-------|--------|----------|
| Reference files → permanent | ✅ | Stored in `{FileStorage:Path}` (root); not touched by orphan cleanup |
| Final files → permanent | ✅ | Stored in `{FileStorage:Path}/Permanent` |
| Preview files → temporary | ✅ | Stored in `{FileStorage:Path}/Temporary` |
| Preview files removed after revision | ✅ | `DeletePreviewFilesAsync` on `RequestRevisionAsync` |
| No orphan files (by design) | ✅ | Orphan cleanup deletes files not in DB from Temporary only |

**Verdict:** PASS

---

### MODULE 10 — Orphan File Cleanup

| Check | Status | Evidence |
|-------|--------|----------|
| Preview storage scanned | ✅ | `OrphanFileCleanupService` scans `{FileStorage:Path}/Temporary` |
| Files not in DB deleted | ✅ | Compares disk paths to `LogoFiles` + `RevisionFiles` |
| Reference/Final folders never touched | ✅ | Only `Temporary` folder enumerated |
| Runs periodically | ✅ | Default 24h; configurable via `FileStorage:OrphanCleanupIntervalMinutes` |

**Verdict:** PASS

---

### MODULE 11 — Messaging System

| Check | Status | Evidence |
|-------|--------|----------|
| Client → Admin only | ✅ | `MessageService.CreateMessageAsync`: Client messages set `RequiresAdminApproval = true`, `RecipientId = null`; Admin/SuperAdmin notified |
| Designer → Admin only | ✅ | Same logic for Designer |
| Client→Designer direct blocked | ✅ | Throws `InvalidOperationException("Clients cannot message designers directly...")` |
| Designer→Client direct blocked | ✅ | Throws `InvalidOperationException("Designers cannot message clients directly...")` |
| Admin forward/reject | ✅ | `ForwardMessageAsync`, `RejectMessageAsync`; `[Authorize(Roles = "SuperAdmin,Admin")]` |

**Verdict:** PASS

---

### MODULE 12 — Notifications

| Check | Status | Evidence |
|-------|--------|----------|
| Admin: new order | ✅ | `OrderCreated` SignalR; `CreateNotificationForRoleAsync("Admin")` |
| Designer: order assigned | ✅ | `OrderAssigned` SignalR; notification to designer |
| Client: preview delivered | ✅ | `PreviewDelivered` SignalR; `CreateNotificationAsync` to client |
| Identities masked in notifications | ✅ | Messages use "Company Design Team", "Company Project"; no direct IDs in user-facing text |

**Verdict:** PASS

---

### MODULE 13 — SignalR Realtime Events

| Check | Status | Evidence |
|-------|--------|----------|
| OrderCreated | ✅ | `SignalRRealtimeEntityUpdateSender.SendOrderCreatedAsync` |
| OrderAssigned | ✅ | `SendOrderAssignedAsync` |
| PreviewUploaded | ✅ | `SendPreviewUploadedAsync` |
| PreviewForwarded | ✅ | Via `SendPreviewDeliveredAsync` when admin sends to client |
| RevisionRequested | ✅ | `SendPreviewRejectedAsync` |
| FinalApproved | ✅ | `SendOrderStatusChangedAsync`, `SendPreviewApprovedAsync` |
| Frontend subscribes | ✅ | `realtime-notification.service.ts` listens to all events, pushes to `orderUpdates$` |
| JWT for SignalR | ✅ | `access_token` query param for `/hubs` paths |

**Verdict:** PASS

---

### MODULE 14 — Security Validation

| Check | Status | Evidence |
|-------|--------|----------|
| Direct client-designer communication blocked | ✅ | Messaging blocks; orders/files mask identities |
| Unauthorized API access blocked | ✅ | Role/permission attributes; service-level checks |
| File access by role | ✅ | `DownloadFileAsync`, `GetOrderFilesAsync` enforce Client/Designer/Admin rules |
| **Client sees designer identity in files** | ⚠️ **FAIL** | `FileService.GetOrderFilesAsync` and `GetAllFilesAsync` populate `UploadedByName` for Client without masking designer-uploaded files. Client sees designer's real name for preview files. |

**Critical Bug:** In `FileService.GetOrderFilesAsync` (lines 584–608) and `GetAllFilesAsync` (lines 681–706), `UploadedByName` is set for all files. For Designer role, it is cleared. For **Client** role, it is **not** cleared. When a client views preview files (uploaded by designer), they see the designer's actual name, violating the mediated workflow rule: *"Client must NOT see: Designer identity"*.

**Verdict:** FAIL (one critical identity-masking bug)

---

### MODULE 15 — Error Handling

| Check | Status | Evidence |
|-------|--------|----------|
| File upload failure | ✅ | `InvalidOperationException` for size/type; returns 400 |
| Database errors | ✅ | EF Core exceptions propagate; global exception middleware can handle |
| Network interruptions | ✅ | SignalR `withAutomaticReconnect()`; no broken order creation on transient failure |
| Broken orders avoided | ✅ | Order creation is transactional; file prepare happens before order commit |

**Verdict:** PASS

---

### MODULE 16 — Performance

| Check | Status | Evidence |
|-------|--------|----------|
| Multiple preview uploads | ✅ | `UploadMultipleFilesAsync` batches; single `SaveChangesAsync` |
| Large file limit | ✅ | 10MB max; `RequestSizeLimit(50MB)` on `CreateOrderWithFiles` |
| Concurrent order creation | ⚠️ Not load-tested | No dedicated load tests found; standard EF/async patterns used |

**Verdict:** PASS (no load tests; design appears sound)

---

## 2. Role-by-Role Verification

| Role | Access | Restrictions | Identity Masking |
|------|--------|---------------|------------------|
| **Client** | Own orders, own files, create order, request revision, approve logo | Cannot access admin/designer endpoints; cannot message designer | Designer hidden in orders; **designer name visible in file list (BUG)** |
| **Designer** | Assigned orders, upload previews, view revisions | Cannot see client identity; cannot message client | Client = null in orders; UploadedBy cleared in files |
| **Admin** | All orders (with ViewAllOrders), assign, send previews, forward messages | Permission-based for ViewAllOrders, AssignOrder | Client email/phone masked in ClientInfoDto |
| **SuperAdmin** | Full access, permissions, seed, settings | None | Same as Admin |

---

## 3. Security Validation Summary

- **Authentication:** JWT with proper validation; roles in claims.
- **Authorization:** Role-based and permission-based; SuperAdmin bypasses permissions.
- **Mediated workflow:** Messaging enforces Client/Designer → Admin only.
- **Identity masking:** Orders and messages correctly mask; **files expose designer to client (bug)**.

---

## 4. File Lifecycle Validation

| Stage | Storage | When Deleted |
|-------|---------|--------------|
| Reference | `Files/` (root) | Never by system; only on explicit delete |
| Preview | `Files/Temporary` | On revision request; or orphan cleanup if no DB record |
| Revision | `Files/Temporary` | On next revision or final approval |
| Final | `Files/Permanent` | Never by system |

**Orphan cleanup:** Only scans `Temporary`; never touches root or `Permanent`.

---

## 5. Notification and SignalR Verification

All expected events are implemented and consumed:

- Backend: `SignalRRealtimeEntityUpdateSender` sends to user groups.
- Frontend: `RealtimeNotificationService` subscribes to OrderCreated, OrderAssigned, PreviewUploaded, OrderStatusChanged, PreviewApproved, PreviewRejected, PreviewDelivered, OrderUpdated, InvoiceGenerated.
- User groups: `JoinUserGroup(userId)` → `user-{userId}`.

---

## 6. Performance Observations

- No dedicated load or stress tests.
- Async/await used throughout.
- File operations use streaming where appropriate.
- Orphan cleanup runs on configurable interval (default 24h).

---

## 7. Detected Risks and Bugs

### Critical

1. **Identity masking in file list (Client view)**  
   - **Location:** `FileService.GetOrderFilesAsync`, `FileService.GetAllFilesAsync`  
   - **Issue:** Client sees `UploadedByName` for designer-uploaded preview files.  
   - **Fix:** When `userRole == "Client"`, for files where `UploadedBy` is a designer (or `FileType == Preview`), set `UploadedByName = "Company Design Team"` and optionally `UploadedBy = Guid.Empty`.

### Minor

2. **Permission service fallback:** `RequirePermissionAttribute` allows request if `IPermissionService` is null (backward compatibility). Consider failing in production.
3. **JWT key:** Ensure `Jwt:Key` is strong and not default in production/staging config.

---

## 8. Test Coverage

- **RevisionWorkflowTests:** 3 revisions, preview deletion, reference preservation, final conversion.
- **OrderServiceFullBatchTests:** Partial preview batch rejected; full batch succeeds.
- **AuthServiceTests, InvoiceServiceTests:** Auth and invoice logic.
- **All 8 tests pass.**

---

## 9. Recommended Fix for Critical Bug

In `FileService.GetOrderFilesAsync` and `GetAllFilesAsync`, after populating `UploadedByName`, add:

```csharp
// Mask designer identity from clients (mediated workflow)
if (userRole == "Client")
{
    foreach (var fileDto in result)
    {
        var file = files.First(f => f.Id == fileDto.Id);
        // Designer-uploaded files (Preview) or any non-client upload: mask
        if (file.FileType == FileType.Preview || file.UploadedBy != userId)
        {
            fileDto.UploadedBy = Guid.Empty;
            fileDto.UploadedByName = "Company Design Team";
        }
    }
}
```

(Adjust logic if clients can upload non-Reference types; current design suggests only Reference for clients.)

---

## 10. Final Verdict

### READY FOR STAGING

**Update:** The identity masking bug was fixed in `FileService.GetOrderFilesAsync` and `FileService.GetAllFilesAsync`. When `userRole == "Client"`, designer-uploaded files (Preview type or `UploadedBy != client`) now show `UploadedByName = "Company Design Team"` instead of the designer's real name.

**Remaining recommendations:**
- Verify `Jwt:Key` is strong and not default in staging/production config.
- Consider failing `RequirePermission` when `IPermissionService` is null in production.

---

*Report generated by staging readiness audit. All code references are from the current codebase snapshot.*
