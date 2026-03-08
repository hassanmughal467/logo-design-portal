# System Audit Report: Mediated Admin Workflow

**Date:** March 6, 2025  
**Scope:** Order workflow, messaging workflow, file lifecycle, and security validation  
**Tech Stack:** ASP.NET Core API, Angular, SQL Server, SignalR  
**Status:** Analysis only — no code changes made

---

## Executive Summary

The system implements a **mediated Admin workflow** where Client and Designer never communicate directly. The messaging workflow correctly enforces this. However, **critical issues** exist in the file lifecycle: designer preview files are **permanently deleted** on revision request (spec requires retention for history), admin can **partially forward** preview files, and the **Projects view** allows designers to upload as Reference type (bypassing the preview workflow). **Files can be lost** in revision and approval flows.

---

## 1. VERIFIED ORDER WORKFLOW

### Workflow Sequence: CLIENT → ADMIN → DESIGNER → ADMIN → CLIENT

| Step | Verified | Details |
|------|----------|---------|
| Client creates order | ✅ | `OrdersController.CreateOrder` → `OrderService.CreateOrderAsync` |
| Admin assigns designer | ✅ | `AssignOrderToDesignerAsync` sets `DesignerId`, status → `InProgress` |
| Designer uploads preview | ✅ | `FileService.UploadFileAsync` / `UploadMultipleFilesAsync` with `fileType=Preview` |
| Admin forwards to client | ✅ | `SendFilesToClientAsync` sets `IsVisibleToClient=true`, `IsAdminApproved=true` |
| Client receives files | ✅ | `GetOrderFilesAsync` filters by `IsVisibleToClient` for Client role |
| Client approves/rejects | ✅ | `RevisionService.ApproveLogoAsync` or `RequestRevisionAsync` |

### Order Creation Flow

| Check | Status | Details |
|-------|--------|---------|
| Controller | ✅ | `POST /api/orders` → `OrderService.CreateOrderAsync` |
| File upload | ⚠️ | **Separate request.** Order created first, then `POST /api/files/upload-multiple/{orderId}` |
| Database records | ✅ | `LogoFile` with `OrderId`, `FileType.Reference`, `IsVisibleToClient=true` |
| File storage | ✅ | Reference files in `{FileStorage}/` (main path) |
| Multiple files | ✅ | `UploadMultipleFilesAsync` supports multiple files |
| File name collisions | ✅ | `fileName = $"{Guid.NewGuid()}{fileExtension}"` — no collision risk |
| Files linked to order | ✅ | `LogoFile.OrderId` set correctly |
| Atomicity | ❌ | Order creation and file upload are **not atomic**. If upload fails, order exists without files. |

---

## 2. ADMIN ORDER ASSIGNMENT

| Check | Status | Details |
|-------|--------|---------|
| Assignment API | ✅ | `POST /api/orders/{id}/assign` → `AssignOrderToDesignerAsync` |
| Database update | ✅ | `DesignerId` set, status → `InProgress` |
| SignalR notification | ⚠️ | **No `OrderAssigned` event.** Designer gets in-app notification only; no grid refresh. |
| Designer sees order | ✅ | `GetOrderByIdAsync` / `GetOrdersByDesignerAsync` |
| Designer sees project brief | ✅ | Order includes `Title`, `Description`, `Instructions`, etc. |
| Designer sees client files | ✅ | `GetOrderFilesAsync` returns all files for assigned orders |
| Designer does NOT see client identity | ✅ | `response.Client = null` for Designer in `GetOrderByIdAsync` |
| DTO filtering | ✅ | `ClientInfoDto` masked; designer gets no client email/phone |

---

## 3. DESIGNER FILE UPLOAD

| Check | Status | Details |
|-------|--------|---------|
| Preview upload API | ✅ | `POST /api/files/upload/{orderId}` and `upload-multiple/{orderId}` |
| fileType=Preview | ✅ | `file-upload.component.ts` sets `fileType: FileType.Preview` for designers |
| Multiple files | ✅ | `UploadMultipleFilesAsync` |
| Revision numbers | ⚠️ | `VersionNumber` is global per order (max+1). **No `RevisionNumber` or batch grouping.** |
| File grouping by revision | ❌ | No `RevisionNumber` or `PreviewBatchId` |
| IsVisibleToClient | ✅ | Designer uploads set `IsVisibleToClient=false` |
| IsAdminApproved | ✅ | Designer uploads set `IsAdminApproved=false` |
| Client sees before admin approval | ✅ | Client filter: `Where(f => f.IsVisibleToClient)` — correct |

### Critical Bug: Project-Detail Upload Path

| Location | Issue |
|----------|-------|
| `project-detail.component.ts` lines 201–206 | Uses `apiService.post(\`files/upload/${projectId}\`, formData)` **without `fileType`** |
| Backend default | `FilesController.UploadFile` has `[FromForm] string? fileType = "Reference"` |
| Result | **Designer uploads via Projects view are stored as Reference**, not Preview |
| Impact | Files not deleted on revision; client may see them immediately; workflow bypassed |

---

## 4. ADMIN PREVIEW REVIEW

| Check | Status | Details |
|-------|--------|---------|
| Admin preview API | ✅ | `GetOrderFilesAsync` (Admin) returns all files — no visibility filter |
| File query logic | ✅ | Admin sees Reference + Preview |
| Admin sees ALL preview files | ✅ | No filter limiting to "latest file" — all files returned |

---

## 5. ADMIN FORWARD TO CLIENT

| Check | Status | Details |
|-------|--------|---------|
| Forward API | ✅ | `POST /api/orders/{id}/send-files-to-client` with `List<Guid> fileIds` |
| File selection | ❌ | **Admin manually selects** files via checkboxes. No "send all pending preview" action. |
| All files from revision | ❌ | **Not enforced.** Admin can send a subset. |
| Files skipped | ⚠️ | Depends on admin selection. Client may receive incomplete preview set. |
| SignalR notification | ✅ | `SendPreviewDeliveredAsync` notifies client |
| Reference files in dialog | ⚠️ | Send Files dialog shows Reference + Preview. Admin can select Reference (redundant). |

---

## 6. CLIENT FILE VIEW

| Check | Status | Details |
|-------|--------|---------|
| Client file API | ✅ | `GetOrderFilesAsync` with `userRole=Client` |
| Filtering | ✅ | `filesQuery.Where(f => f.IsVisibleToClient)` |
| Client sees only approved | ✅ | Correct |
| Client does NOT see designer | ✅ | Designer identity masked in DTOs |
| Client does NOT see internal history | ✅ | Only visible files returned |

---

## 7. CLIENT REVISION REQUEST

| Check | Status | Details |
|-------|--------|---------|
| Revision API | ✅ | `POST /api/revisions/orders/{id}/request` |
| Old preview files removed | ❌ | **Permanently deleted** (physical + DB). Spec: "hidden but still stored for history." |
| Designer notified | ✅ | `CreateNotificationAsync` + `SendPreviewRejectedAsync` |
| Client gallery | ✅ | After deletion, client only sees new set when admin forwards again |
| Old previews visible incorrectly | N/A | Deleted, so not visible — but **lost for audit/history** |

---

## 8. FINAL APPROVAL

| Check | Status | Details |
|-------|--------|---------|
| Approve API | ✅ | `POST /api/revisions/orders/{id}/approve-logo` |
| Approved files marked | ✅ | `FileType.Final`, `IsFinalVersion=true`, moved to Permanent |
| Client gallery | ✅ | `ClientGallery` entries created |
| Revision files | ⚠️ | **Permanently deleted** (soft-deleted in DB, physical file removed) |

---

## 9. FILE LIFECYCLE AUDIT

| Rule | Status | Notes |
|------|--------|-------|
| Client upload files remain attached to order | ✅ | `LogoFile.OrderId`; Reference files in main path |
| Designer previews stored separately | ✅ | Temporary storage for Preview/Revision |
| Designer previews grouped by revision | ❌ | No `RevisionNumber`; cannot group |
| Admin forwarding marks visible | ✅ | `IsVisibleToClient=true`, `IsAdminApproved=true` |
| Client revision hides previous previews | ⚠️ | **Deletes** them; spec says "hide" |
| Final approval locks files | ✅ | Moved to Permanent, added to ClientGallery |

---

## 10. FILE STORAGE ARCHITECTURE

| Path | Purpose |
|------|---------|
| `{FileStorage}/` | Reference files (client uploads) |
| `{FileStorage}/Temporary/` | Preview, Revision files |
| `{FileStorage}/Permanent/` | Final approved files |

| Risk | Status |
|------|--------|
| File name collisions | ✅ | `Guid.NewGuid()` used |
| Incorrect storage path | ✅ | Correct by `FileType` |
| Overwrite on new upload | ✅ | Each file gets unique GUID filename |
| Multiple designers overwriting | ✅ | Each upload creates new `LogoFile` record |

---

## 11. SIGNALR EVENT AUDIT

| Event | Exists | Sent To | Notes |
|-------|--------|---------|-------|
| New order created | ✅ | Admin, SuperAdmin | `OrderCreated` |
| Designer assigned | ❌ | — | **Missing.** Only in-app notification. |
| Preview uploaded | ❌ | — | **Missing.** Order status updated; no SignalR. |
| Files sent to client | ✅ | Client | `PreviewDelivered` |
| Revision requested | ✅ | Admin, Designer | `PreviewRejected` |
| Final approval | ✅ | Client, Admin, Designer | `OrderStatusChanged` (Completed) |

---

## 12. MESSAGING WORKFLOW (Admin-Mediated)

| Check | Status | Details |
|-------|--------|---------|
| Client → Designer direct | ❌ | Blocked. `CreateMessageAsync` throws if Client→Designer. |
| Designer → Client direct | ❌ | Blocked. Same validation. |
| Client/Designer → Admin | ✅ | `RequiresAdminApproval=true`, `RecipientId=null` |
| Admin forward | ✅ | `ForwardMessageAsync` — `[Authorize(Roles = "SuperAdmin,Admin")]` |
| Admin reject | ✅ | `RejectMessageAsync` — same authorization |
| Identity masking | ✅ | Client sees "Company Design Team"; Designer sees "Company Project" |

---

## 13. DETECTED BUGS AND RISKS

### Critical

1. **Revision permanently deletes preview files**  
   - **Location:** `RevisionService.DeletePreviewFilesAsync`  
   - **Behavior:** Physical delete + `_context.LogoFiles.RemoveRange(previewFiles)`  
   - **Spec:** "Old preview files should remain in database for history but hidden from client."  
   - **Impact:** No audit trail; files lost for compliance.

2. **Admin can partially forward preview files**  
   - **Location:** Send Files dialog + `SendFilesToClientAsync`  
   - **Behavior:** Admin selects subset via checkboxes.  
   - **Spec:** "ALL uploaded files for that preview must be sent."  
   - **Impact:** Client may receive incomplete preview set.

3. **Project-detail designer uploads as Reference**  
   - **Location:** `project-detail.component.ts` → `files/upload` without `fileType`  
   - **Behavior:** Backend defaults to `Reference`.  
   - **Impact:** Designer previews stored as Reference; not deleted on revision; may be visible to client immediately.

### Medium

4. **canSendFiles never true for pending designer previews**  
   - **Location:** `order-detail.component.ts` line 451  
   - **Code:** `canSendFiles(): files.some(f => !f.isVisibleToClient && f.isAdminApproved)`  
   - **Issue:** Pending previews have `isAdminApproved=false`, so condition is never true.  
   - **Mitigation:** Admin Action Panel uses `hasPendingPreviewFiles()` which is correct.

5. **No RevisionNumber / batch grouping**  
   - **Location:** `LogoFile` entity  
   - **Impact:** Cannot enforce "send all from revision N" or "show only latest set to client."

6. **Order creation and file upload not atomic**  
   - **Impact:** Order can exist without files if upload fails.

### Low

7. **Designer assignment lacks SignalR grid update**  
8. **Missing SignalR: Designer uploaded preview** (Admin should get real-time notification)  
9. **Send Files dialog includes Reference files** (redundant but harmless)

---

## 14. DATABASE STRUCTURE AUDIT

### LogoFile Table

| Field | Present | Purpose |
|-------|---------|---------|
| OrderId | ✅ | Links to order |
| FileType | ✅ | Reference, Preview, Final, Revision |
| IsVisibleToClient | ✅ | Client visibility |
| IsAdminApproved | ✅ | Admin approval |
| VersionNumber | ✅ | Global version per order |
| UploadedBy | ✅ | User who uploaded |
| ApprovedBy, ApprovedAt | ✅ | Admin approval metadata |
| RevisionNumber | ❌ | **Missing.** Would group files by revision. |
| PreviewBatchId | ❌ | **Missing.** Same purpose. |
| IsHiddenFromClient | ❌ | **Missing.** Would allow soft-hide instead of delete. |
| UploadedByRole | ❌ | **Missing.** Could derive from UploadedBy + User.Role. |

### Orders, Messages

- Orders: Supports workflow. `AllowUploads` controls uploads.
- Messages: `RequiresAdminApproval`, `ForwardedByAdmin`, `OriginalSenderRole`, `IsRejected` support mediated workflow.

---

## 15. FILES WILL NEVER BE LOST — CONFIRMATION

### Answer: **NO — Files CAN be lost.**

| Scenario | Risk | Details |
|----------|------|---------|
| Order creation | Low | Upload can fail; order exists without files. User can retry. |
| Client reference files | Low | Stored in main path; not deleted on revision. |
| Designer preview files | **HIGH** | **Permanently deleted** on revision request. No recovery. |
| Revision files (client refs) | Medium | `DeleteRevisionFilesAsync` removes `RevisionFile` records and physical files. |
| Final approved files | Low | Moved to Permanent; added to ClientGallery. |
| Admin partial forward | Medium | Admin can omit files; client gets incomplete set (files not lost, but workflow broken). |

**Summary:** Designer preview files are **lost** when the client requests a revision. The spec expects them to remain in the database for history.

---

## 16. RECOMMENDATIONS (Not Implemented)

### Fix 1: Soft-hide instead of delete on revision

- Add `IsHiddenFromClient` to `LogoFile`.
- In `RequestRevisionAsync`, set `IsHiddenFromClient=true` for Preview files instead of deleting.
- Client query: `Where(f => f.IsVisibleToClient && !f.IsHiddenFromClient)`.
- Keep physical files in Temporary or archive path.

### Fix 2: Enforce "send all" for admin forward

- Add `RevisionNumber` or `PreviewBatchId` to group files.
- Add "Send all pending preview files" action that sends every file with `!IsVisibleToClient` and `FileType=Preview`.
- Optionally restrict `SendFilesToClient` to Preview files only.

### Fix 3: Project-detail designer uploads

- In `project-detail.component.ts`, pass `fileType: 'Preview'` when designer uploads.
- Or route designer to `/orders/:id/upload` instead of project upload.

### Fix 4: canSendFiles logic

- Change to: `files.some(f => !f.isVisibleToClient)` (align with `hasPendingPreviewFiles`).

### Fix 5: Add RevisionNumber

- Add `RevisionNumber` (int) to `LogoFile`.
- Increment when new revision is requested; assign to new designer uploads.

### Fix 6: SignalR events

- Add `OrderAssigned` for designer grid refresh.
- Add `PreviewUploaded` for admin when designer uploads.

---

## 17. SUMMARY TABLE

| Step | Overall | Key Issues |
|------|---------|------------|
| 1. Client creates order | ✅ | Files in separate request; not atomic |
| 2. Admin assigns designer | ✅ | No SignalR grid update for designer |
| 3. Designer uploads preview | ⚠️ | Project-detail uses Reference by default |
| 4. Admin forwards files | ❌ | Manual selection; no "send all" |
| 5. Client receives files | ✅ | Visibility filtering correct |
| 6. Client decision | ⚠️ | Revision permanently deletes previews |
| Messaging workflow | ✅ | Admin-mediated; Client/Designer never direct |

---

*End of audit report. No code changes were made.*
