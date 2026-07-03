# Order Workflow & File Lifecycle Audit Report

**Date:** March 6, 2025  
**Scope:** End-to-end order workflow, file lifecycle, and role-based visibility  
**Status:** Analysis only — no business logic modified

---

## 1. End-to-End Flow Verification

### STEP 1 — Client Creates Order

| Check | Status | Details |
|-------|--------|---------|
| Controller handling order creation | ✅ | `OrdersController.CreateOrder` → `OrderService.CreateOrderAsync` |
| File upload logic | ⚠️ | Files are **not** uploaded in the same request. Order is created first via `POST /api/orders`, then files are uploaded via `POST /api/files/upload-multiple/{orderId}` |
| Database records for files | ✅ | `LogoFile` records created with `FileType.Reference`, `IsVisibleToClient=true`, `IsAdminApproved=true` |
| File storage path | ✅ | Reference files stored in `{FileStorage}/` (main path) |
| SignalR notification to Admin | ✅ | `SendOrderCreatedAsync` notifies Admin/SuperAdmin; `CreateNotificationForRoleAsync` for Admin/SuperAdmin |
| Files visible to Admin | ✅ | Admin uses `GetOrderFilesAsync` which returns all files (no `IsVisibleToClient` filter for Admin) |

**Flow:** Client submits order → Order created with status `WaitingForAdminApproval` → If client selected files, `uploadFiles(orderId)` is called → Files uploaded as Reference type → Admin notified.

**Risk:** If order creation succeeds but file upload fails (network, etc.), the order exists without files. The frontend shows a warning and allows uploading later from order details. Order creation and file upload are not atomic.

---

### STEP 2 — Admin Assigns Designer

| Check | Status | Details |
|-------|--------|---------|
| Order assignment logic | ✅ | `OrderService.AssignOrderToDesignerAsync` sets `DesignerId`, status → `InProgress` |
| SignalR notification to designer | ⚠️ | Uses `CreateNotificationAsync` (in-app notification), **not** `SendOrderCreatedAsync`-style real-time entity update. Designer gets notification record but no SignalR `OrderAssigned` event for grid refresh. |
| File retrieval for designer dashboard | ✅ | Designer uses `GetOrderFilesAsync` — returns all files for assigned orders. `UploadedBy`/`UploadedByName` are masked for designers. |
| Designer sees Order ID, Project brief, Uploaded files | ✅ | Order includes `Files`; designer gets order via `GetOrderByIdAsync` / `GetOrdersByDesignerAsync` |
| Designer does NOT see client identity | ✅ | `response.Client = null` for Designer role in `GetOrderByIdAsync` |

---

### STEP 3 — Designer Works On Order

| Check | Status | Details |
|-------|--------|---------|
| Preview upload endpoint | ✅ | `POST /api/files/upload/{orderId}` and `POST /api/files/upload-multiple/{orderId}` with `fileType=Preview` |
| Database records for each file | ✅ | `LogoFile` with `FileType.Preview`, `IsVisibleToClient=false`, `IsAdminApproved=false` |
| File version handling | ⚠️ | `VersionNumber` is a global increment per order (max of existing + 1). No `RevisionNumber` or batch grouping for preview sets. |
| File visibility flag | ✅ | Designer uploads set `IsVisibleToClient=false` until admin forwards |
| Multiple files support | ✅ | `UploadMultipleFilesAsync` supports multiple files |
| Files visible to Admin | ✅ | Admin sees all files via `GetOrderFilesAsync` |
| Files NOT visible to Client | ✅ | Client filter: `filesQuery.Where(f => f.IsVisibleToClient)` |

**Designer upload path:** Designer uses `/orders/:orderId/upload` (file-upload component) which sets `fileType=Preview` for designers. ✅

**Alternative path:** `project-detail` uses `files/upload` with **default** `fileType=Reference`. If designers use the Projects view to upload, files would be stored as Reference (not Preview) and would **not** be deleted on revision request. ⚠️

---

### STEP 4 — Admin Receives Preview Files

| Check | Status | Details |
|-------|--------|---------|
| Admin preview API | ✅ | `GetOrderFilesAsync` returns all files for Admin (no visibility filter) |
| File retrieval logic | ✅ | Admin sees Reference + Preview files |
| Forward-to-client logic | ✅ | `OrderService.SendFilesToClientAsync` accepts `List<Guid> fileIds`, sets `IsVisibleToClient=true`, `IsAdminApproved=true` |
| ALL files from that preview sent | ❌ | Admin **manually selects** which files to send. Send Files dialog shows all files with checkboxes. Admin can select a subset — no enforcement that all preview files for a revision are sent. |
| No files skipped or missing | ❌ | Depends on admin selection. No "Send all pending preview files" action. |

**Send Files dialog:** Shows `*ngFor="let file of files"` — all files (Reference + Preview). Admin can select any subset. Spec requires "ALL uploaded files for that preview are sent."

---

### STEP 5 — Client Receives Files

| Check | Status | Details |
|-------|--------|---------|
| Client file API | ✅ | `GetOrderFilesAsync` with `userRole=Client` filters by `IsVisibleToClient` |
| File visibility filtering | ✅ | `filesQuery.Where(f => f.IsVisibleToClient)` for Client |
| SignalR notification | ✅ | `SendPreviewDeliveredAsync` notifies client; `CreateNotificationAsync` for "Preview Files Available" |

---

### STEP 6 — Client Decision

| Check | Status | Details |
|-------|--------|---------|
| Approve flow | ✅ | `RevisionService.ApproveLogoAsync` converts Preview → Final, moves to Permanent, adds to ClientGallery, status → Completed |
| Request Revision flow | ✅ | `RevisionService.RequestRevisionAsync` — only when status is `PreviewDelivered` |
| Old preview files removed from client gallery | ⚠️ | **Permanently deleted** (physical + DB). Spec says: "Keep preview files in database for history but hidden from client." Current behavior: files are **removed**, not soft-hidden. |
| Client gallery shows only latest preview set | ✅ | After revision, old previews are deleted, so client only sees new set when admin forwards again |
| Revision request notifies Designer | ✅ | `CreateNotificationAsync` + `SendPreviewRejectedAsync` |

---

## 2. File Lifecycle Verification

| Rule | Status | Notes |
|------|--------|-------|
| 1. Client files always attached to order | ✅ | `LogoFile.OrderId` links to order; Reference files stored in main path |
| 2. Designer preview uploads support multiple files | ✅ | `UploadMultipleFilesAsync` |
| 3. Admin forwards ALL preview files from that revision | ❌ | Admin selects files manually; no batch/send-all enforcement |
| 4. Client gallery removes previous previews on revision | ✅ | `DeletePreviewFilesAsync` removes them (but permanently, not soft-hide) |
| 5. Only latest preview in client gallery | ✅ | Old ones deleted; new ones visible only after admin forwards |
| 6. Only approved files stored as final | ✅ | `ApproveLogoAsync` converts Preview → Final and moves to Permanent |
| 7. Old previews remain in DB for history | ❌ | Old previews are **permanently deleted** (physical + DB remove) |

---

## 3. Potential Bugs

### Critical

1. **Revision deletes preview files permanently**  
   - **Location:** `RevisionService.DeletePreviewFilesAsync`  
   - **Behavior:** Physical file delete + `_context.LogoFiles.RemoveRange(previewFiles)`  
   - **Spec:** "Old preview files should remain in database for history but hidden from client."  
   - **Impact:** No audit trail of previous previews; files lost for compliance/history.

2. **Admin can partially forward preview files**  
   - **Location:** Send Files dialog + `SendFilesToClientAsync`  
   - **Behavior:** Admin selects files via checkboxes; can send subset.  
   - **Spec:** "ALL uploaded files for that preview must be sent."  
   - **Impact:** Client may receive incomplete preview set.

3. **Project-detail designer uploads as Reference**  
   - **Location:** `project-detail.component.ts` → `files/upload` without `fileType`  
   - **Behavior:** Defaults to `Reference`; stored in main path.  
   - **Impact:** If designer uses Projects view, files are not Preview type and are **not** deleted on revision request. Old "preview" files could remain visible.

### Medium

4. **canSendFiles never true for pending designer previews**  
   - **Location:** `order-detail.component.ts` line 451  
   - **Code:** `canSendFiles(): files.some(f => !f.isVisibleToClient && f.isAdminApproved)`  
   - **Issue:** Pending designer previews have `isAdminApproved=false`, so condition is never true.  
   - **Impact:** Header "Send Files to Client" button may not show. Admin relies on Admin Action Panel "Forward to Client" (driven by `hasPendingPreviewFiles`).

5. **No RevisionNumber / batch grouping**  
   - **Location:** `LogoFile` entity  
   - **Issue:** No `RevisionNumber` or `PreviewBatchId` to group files by revision.  
   - **Impact:** Cannot enforce "send all files from revision N" or filter "latest preview set only."

6. **Order creation and file upload not atomic**  
   - **Location:** `order-create.component.ts`  
   - **Behavior:** Order created first; files uploaded in separate request.  
   - **Impact:** Order can exist without files if upload fails; user must upload later.

### Low

7. **Designer assignment lacks real-time grid update**  
   - Designer gets in-app notification but no SignalR entity event for order grid refresh.

8. **Send Files dialog includes Reference files**  
   - Admin can select Reference files (already visible). Redundant but harmless.

---

## 4. Missing Database Flags

| Flag | Present | Purpose |
|------|---------|---------|
| `IsVisibleToClient` | ✅ | Controls client visibility |
| `IsAdminApproved` | ✅ | Admin approval before client sees |
| `IsApproved` (client approval) | ⚠️ | Implicit via `FileType.Final` and `ClientGallery` |
| `RevisionNumber` | ❌ | Would group files by revision; enable "latest set" and "send all from revision N" |
| `PreviewBatchId` / `RevisionBatchId` | ❌ | Same as above — group files per preview batch |
| `IsHiddenFromClient` (soft hide) | ❌ | Would allow keeping old previews in DB but hidden, instead of delete |
| `SupersededByFileId` | ❌ | Optional; link old preview to new one for history |

**Recommendation:** Add `RevisionNumber` (or `PreviewBatchId`) and `IsHiddenFromClient` to support:
- Sending all files from a revision in one action
- Keeping old previews in DB but hidden from client

---

## 5. Confirmation: Will Files Ever Be Lost?

| Scenario | Risk | Notes |
|----------|------|-------|
| Order creation | Low | Order created first; files in separate request. Upload can fail; user can retry. |
| Client reference files | Low | Stored in main path; not deleted on revision. |
| Designer preview files | **High** | **Permanently deleted** on revision request. No recovery. |
| Revision files (client refs) | Medium | `DeleteRevisionFilesAsync` permanently deletes `RevisionFile` records. |
| Final approved files | Low | Moved to Permanent; added to ClientGallery. |
| Admin partial forward | Medium | Admin can omit files; client gets incomplete set. |

**Summary:** Designer preview files are lost when the client requests a revision. The spec expects them to remain in the database for history.

---

## 6. Suggested Fixes (Not Implemented)

### Fix 1: Soft-hide instead of delete on revision

- In `RequestRevisionAsync`, do **not** call `DeletePreviewFilesAsync`.
- Add `IsHiddenFromClient` (or similar) to `LogoFile`.
- Set `IsHiddenFromClient = true` for all Preview files for that order.
- Client query: `Where(f => f.IsVisibleToClient && !f.IsHiddenFromClient)` (or equivalent).
- Physical files can remain in Temporary storage or be moved to an archive path.

### Fix 2: Enforce "send all" for admin forward

- Add `PreviewBatchId` or `RevisionNumber` to group files.
- When designer uploads, assign batch id.
- Add endpoint or UI action: "Send all pending preview files" that sends every file with `!IsVisibleToClient` and `FileType=Preview` for the order.
- Optionally restrict `SendFilesToClient` to Preview files only.

### Fix 3: Project-detail designer uploads

- In `project-detail.component.ts`, pass `fileType: 'Preview'` when designer uploads.
- Or route designer to `/orders/:id/upload` instead of project upload.

### Fix 4: canSendFiles logic

- Change to: `files.some(f => !f.isVisibleToClient)` (align with `hasPendingPreviewFiles`).
- Or remove `isAdminApproved` from the condition for pending previews.

### Fix 5: Add RevisionNumber

- Add `RevisionNumber` (int) to `LogoFile`.
- Increment when new revision is requested; assign to new designer uploads.
- Use for "send all from revision N" and "show only latest set to client."

---

## 7. Summary Table

| Step | Overall | Key Issues |
|------|---------|------------|
| 1. Client creates order | ✅ | Files in separate request; not atomic |
| 2. Admin assigns designer | ✅ | No SignalR grid update for designer |
| 3. Designer uploads preview | ✅ | Project-detail uses Reference by default |
| 4. Admin forwards files | ❌ | Manual selection; no "send all" |
| 5. Client receives files | ✅ | Visibility filtering correct |
| 6. Client decision | ⚠️ | Revision permanently deletes previews |

---

*End of audit report. No code changes were made.*
