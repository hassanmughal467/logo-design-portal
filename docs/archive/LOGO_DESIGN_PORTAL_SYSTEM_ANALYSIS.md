# Logo Design Portal – System Analysis

**Purpose:** Full analysis of the current production implementation for safe refactoring.  
**Scope:** Order lifecycle, file upload, storage, admin workflow, designer pricing, notifications, database.  
**Rules:** Analysis only – no code changes.

---

## 1. Order Lifecycle

### 1.1 Full List of Order Statuses

| Enum Value | Status | Description |
|------------|--------|-------------|
| 1 | WaitingForAdminApproval | Order created by client; admin must approve |
| 2 | PriceApprovalPending | Admin requested price approval from client |
| 3 | InProgress | Order approved; designer can work |
| 4 | PreviewDelivered | Admin sent preview files to client |
| 5 | RevisionRequested | Client requested revision |
| 6 | ClientApproved | Client approved design; admin can mark Completed |
| 7 | Completed | Order finished |
| 8 | Cancelled | Legacy (not used) |
| 13 | CancelledByUser | Client cancelled before work started |
| 14 | CancelledByAdmin | Admin cancelled |
| 15 | Refunded | Order refunded |

**Terminal statuses (locked, read-only):** `Completed`, `Cancelled`, `CancelledByUser`, `CancelledByAdmin`, `Refunded`

### 1.2 Order Status Flow

```
Client creates order
    → WaitingForAdminApproval [Client]

Optional: Admin requests price approval
    → PriceApprovalPending [Admin]
    → Client approves/rejects
    → WaitingForAdminApproval or stays PriceApprovalPending [Client]

Admin approves order
    → InProgress [Admin]

Admin assigns designer
    (status stays InProgress) [Admin]

Designer uploads Preview
    (status stays InProgress) [Designer]

Admin sends files to client
    → PreviewDelivered [Admin]

Client reviews:
    Option A: Client approves
        → ClientApproved [Client] (via UpdateOrderStatus or ApproveLogo)
    Option B: Client requests revision
        → RevisionRequested [Client]

Admin sends revised preview
    → PreviewDelivered [Admin]

Client approves (ApproveLogo)
    → ClientApproved [Client] OR Completed [Admin/SuperAdmin]

Admin marks Completed (if ClientApproved)
    → Completed [Admin]

Cancellation:
    → CancelledByUser [Client] (only from WaitingForAdminApproval or PriceApprovalPending)
    → CancelledByAdmin [Admin/SuperAdmin]

Refund:
    → Refunded [Admin]
```

### 1.3 Where Each Status Is Set

| Status | Service/Method | API/Trigger | Role |
|--------|----------------|-------------|------|
| WaitingForAdminApproval | OrderService.CreateOrderAsync | POST /api/orders | Client |
| WaitingForAdminApproval | OrderService.ApprovePriceAsync | POST /api/orders/{id}/approve-price | Client |
| PriceApprovalPending | OrderService.RequestPriceApprovalAsync | POST /api/orders/{id}/request-price-approval | Admin |
| InProgress | OrderService.ApproveOrderAsync | POST /api/orders/{id}/approve | Admin |
| InProgress | OrderService.UpdateOrderStatusAsync | PUT /api/orders/{id}/status | Admin |
| PreviewDelivered | OrderService.SendFilesToClientInternalAsync | POST /api/orders/{id}/send-files-to-client, send-preview-batch | Admin |
| PreviewDelivered | OrderService.UpdateOrderStatusAsync | PUT /api/orders/{id}/status | Admin |
| RevisionRequested | RevisionService.RequestRevisionAsync | POST /api/revisions/{orderId}/request | Client |
| RevisionRequested | OrderService.UpdateOrderStatusAsync | PUT /api/orders/{id}/status | Client |
| ClientApproved | RevisionService.ApproveLogoAsync | POST /api/revisions/{orderId}/approve-logo | Client |
| ClientApproved | OrderService.UpdateOrderStatusAsync | PUT /api/orders/{id}/status | Client |
| Completed | RevisionService.ApproveLogoAsync | POST /api/revisions/{orderId}/approve-logo | Admin/SuperAdmin |
| Completed | OrderService.UpdateOrderStatusAsync | PUT /api/orders/{id}/status | Admin |
| CancelledByUser | OrderService.CancelOrderAsync | (via cancel endpoint) | Client |
| CancelledByAdmin | OrderService.CancelOrderAsync | (via cancel endpoint) | Admin |
| Refunded | OrderService.RefundOrderAsync | Refund endpoint | Admin |

### 1.4 Role-Based Status Transitions (UpdateOrderStatusAsync)

- **Client:** From `PreviewDelivered` only → `RevisionRequested` or `ClientApproved`
- **Designer:** Cannot change order status
- **Admin/SuperAdmin:** From `RevisionRequested` → `PreviewDelivered` or `CancelledByAdmin`; from other statuses → `InProgress`, `PreviewDelivered`, `Completed`, `CancelledByAdmin`

---

## 2. File Upload System

### 2.1 Who Can Upload

| Role | File Types | Access Rule |
|------|------------|-------------|
| Client | Reference only | Must be order owner (ClientId matches) |
| Designer | Preview, Final | Must be assigned designer (DesignerId matches) |
| Admin/SuperAdmin | All types | Full access |

**Verification:** `FileService.VerifyOrderUploadAccessAsync` – checks Client profile or Designer profile against order.

### 2.2 Upload Endpoints

| Endpoint | Method | Parameters |
|----------|--------|------------|
| `/api/files/upload/{orderId}` | POST | `file`, `fileType`, `description`, `designCategory`, `designType`, `proposedPrice` (for Designer + Final) |
| `/api/files/upload-multiple/{orderId}` | POST | `files[]`, `fileType`, `description`, `designCategory`, `designType`, `proposedPrice` |

**Request size limit:** 50 MB (`RequestSizeLimit(50 * 1024 * 1024)`)

### 2.3 Supported File Types (FileType enum)

| Value | Name | Who | Storage |
|-------|------|-----|---------|
| 1 | Reference | Client | Main (Files/) |
| 2 | Preview | Designer | Temporary |
| 3 | Final | Designer | Permanent |
| 4 | Revision | (Revision files in OrderRevisions) | Temporary |

**Allowed extensions:** `.jpg`, `.jpeg`, `.png`, `.gif`, `.webp`, `.svg`, `.pdf`, `.ai`, `.eps`, `.psd`  
**Size limits:** Images 10MB, vector/docs 25MB

### 2.4 File Upload Flow

```
Client/Designer/Admin → POST /api/files/upload/{orderId}
    → FilesController.UploadFile
    → FileService.UploadFileAsync
        → VerifyOrderUploadAccessAsync
        → OrderLockingHelper.IsOrderLocked
        → order.AllowUploads check
        → ValidateFile (extension, size)
        → Determine storage path by FileType
        → If Designer + Final: SubmitDesignerPricingAsync
        → Copy file to disk
        → Create LogoFile entity, SaveChanges
        → Notifications (Preview/Final/Reference)
    → Return FileUploadResponseDto
```

### 2.5 Metadata Saved in DB (LogoFile)

- Id, OrderId, FileName, OriginalFileName, FilePath, ContentType, FileSize
- FileType, FileCategory, FileStatus
- IsFinalVersion, IsVisibleToClient, IsAdminApproved
- VersionNumber, Description
- UploadedBy, ApprovedBy, ApprovedAt
- PreviewBatchId (Designer Preview only)
- BaseEntity: CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsDeleted, DeletedAt, DeletedBy

---

## 3. File Types and Metadata

### 3.1 LogoFile Properties

| Property | Type | Purpose |
|----------|------|---------|
| FileType | enum | Reference, Preview, Final, Revision |
| IsVisibleToClient | bool | Client can see file |
| IsAdminApproved | bool | Admin approved for client visibility |
| IsFinalVersion | bool | True for Final type |
| IsDeleted | bool | Soft delete |
| UploadedBy | Guid | User who uploaded |
| OrderId | Guid | Order |
| VersionNumber | int | Delivery round for Preview; sequence for others |
| PreviewBatchId | Guid? | Groups Preview files from same upload; admin must send full batch |

### 3.2 How System Determines File Type

- **Preview:** `FileType == FileType.Preview` – Designer drafts, stored in Temporary
- **Final:** `FileType == FileType.Final` – Either designer uploads as Final, or Preview converted on client approval
- **Client visible:** `IsVisibleToClient == true` – Set when: (1) Client uploads Reference (auto), (2) Admin sends via SendFilesToClient, (3) Admin approves via ApproveFile, (4) Client approves via ApproveLogo (Preview→Final)

### 3.3 Client Visibility Rules (GetOrderFilesAsync)

- **Client:** Only files where `IsVisibleToClient == true`
- **Designer:** All files for assigned orders (no visibility filter)
- **Admin/SuperAdmin:** All files (no filter)

---

## 4. Storage System

### 4.1 Configuration

- **Config key:** `FileStorage:Path` (appsettings.json)
- **Default:** `Files` (relative to app directory) or `Path.Combine(Directory.GetCurrentDirectory(), "Files")`

### 4.2 Folder Structure

```
{FileStorage:Path}/
├── (root)           → Reference files (client uploads)
├── Temporary/       → Preview, Revision files
└── Permanent/       → Final files
```

### 4.3 Storage by File Type

| FileType | Path | When |
|----------|------|------|
| Reference | `{Path}/` | Client upload |
| Preview | `{Path}/Temporary/` | Designer upload |
| Revision | `{Path}/Temporary/` | Revision reference images |
| Final | `{Path}/Permanent/` | Designer upload as Final, or after ApproveLogo |

### 4.4 Preview → Final Movement

- **Path:** `RevisionService.ApproveLogoAsync`
- **When:** Client or Admin approves logo (order status `PreviewDelivered`)
- **Action:** `File.Move(source, destination, overwrite: true)` – Preview files moved from Temporary to Permanent
- **DB:** FileType set to Final, IsFinalVersion=true, IsVisibleToClient=true, IsAdminApproved=true; ClientGallery entry created
- **No copy/re-upload:** Files are moved in place

### 4.5 No Cloud Storage

- Local disk only (no Azure Blob, AWS S3)
- Paths stored in `LogoFile.FilePath`

---

## 5. Admin Approval Workflow

### 5.1 SendFilesToClientAsync / SendFilesToClientInternalAsync

**Service:** OrderService  
**API:** POST /api/orders/{id}/send-files-to-client (body: `List<Guid> fileIds`)  
**Role:** Admin, SuperAdmin  

**Behavior:**
- Validates order exists, not locked
- Validates all fileIds belong to order
- For Preview files: must send entire batch (no partial delivery)
- Sets `IsVisibleToClient=true`, `IsAdminApproved=true` on selected files
- Sets `ApprovedBy`, `ApprovedAt`
- Order status → `PreviewDelivered`
- Notification to client: "Preview Files Available"
- SignalR: `SendPreviewDeliveredAsync` to client

### 5.2 ApproveLogoAsync

**Service:** RevisionService  
**API:** POST /api/revisions/{orderId}/approve-logo  
**Role:** Client (own order), Admin, SuperAdmin  

**Behavior:**
- Requires order status `PreviewDelivered`
- Deletes revision files (physical delete + soft delete)
- Converts all Preview files to Final: move to Permanent, update FileType, IsFinalVersion, IsVisibleToClient, IsAdminApproved
- Creates ClientGallery entries
- Order status: Client → `ClientApproved`; Admin → `Completed`
- Sets `AllowUploads=false`
- If Admin: sets `BillingEligible=true`, `CompletedDate`, notifies Client and Designer
- If Client: notifies Admin/SuperAdmin only

### 5.3 RequestRevisionAsync

**Service:** RevisionService  
**API:** POST /api/revisions/{orderId}/request  
**Role:** Client (order owner)  

**Behavior:**
- Requires status `PreviewDelivered`
- Checks revision limit
- Deletes all previous Preview files (physical)
- Deletes all Revision files (physical)
- Soft-deletes previous OrderRevisions
- Creates new OrderRevision with instructions and optional reference files
- Order status → `RevisionRequested`
- Notifies Admin, SuperAdmin, Designer
- SignalR: `SendPreviewRejectedAsync`

### 5.4 ApproveDesignerPriceAsync

**Service:** DesignerPayoutService  
**API:** (DesignerPayoutController)  
**Role:** Admin, SuperAdmin  

**Behavior:**
- Requires `PriceApprovalStatus` PendingApproval or Modified
- Actions: Approve (use ProposedPrice), Modify (use ApprovedPrice), Reject
- Sets `ApprovedPrice`, `PriceApproved`, `RequiresPriceApproval=false`

### 5.5 ApproveFileAsync (individual file)

**Service:** FileService  
**API:** PUT /api/files/{id}/approve  
**Role:** Admin, SuperAdmin  

**Behavior:**
- Sets `IsAdminApproved`, `IsVisibleToClient`, `IsFinalVersion` per request
- Updates ApprovedBy, ApprovedAt

---

## 6. Designer Pricing System

### 6.1 When Pricing Is Set

- **Only** when Designer uploads **Final** files
- `FileService.UploadFileAsync` / `UploadMultipleFilesAsync` calls `SubmitDesignerPricingAsync` before saving file
- **Not** set when Designer uploads Preview or when Client approves Preview (ApproveLogo)

### 6.2 SubmitDesignerPricingAsync Flow

**Service:** DesignerPayoutService  
**Triggered by:** FileService when Designer uploads Final  

**Input:** orderId, `SubmitDesignerPricingRequestDto` (DesignCategory, DesignType, ProposedPrice), designerUserId  

**Logic:**
1. Verify designer is assigned to order
2. Reject if `PriceApproved` already
3. Reject if DesignType=ComplexVector and ProposedPrice <= 0
4. Get `StandardPrice` from DesignPricing table
5. Set on order: DesignCategory, DesignType, StandardPrice, ProposedPrice
6. If ProposedPrice == StandardPrice → AutoApproved, PriceApproved=true, ApprovedPrice=ProposedPrice
7. If ProposedPrice != StandardPrice or no standard → RequiresPriceApproval=true, PriceApprovalStatus=PendingApproval; notify Admin/SuperAdmin

### 6.3 DesignPricing Table

- Columns: DesignCategory, DesignType, DefaultPrice, IsActive
- DefaultPrice=0 means "Custom" (designer must enter; e.g. ComplexVector)
- Fallback defaults if table empty: Embroidery LeftChest 350, JacketBack 700, Vector Simple 350, Complex null

### 6.4 Payout Eligibility (GetDesignerPayoutEligibleOrdersAsync)

Order must satisfy:
- DesignerId matches
- Status == Completed
- PriceApproved == true
- ApprovedPrice > 0
- !IsDesignerInvoiced

**Note:** Orders that complete via Preview→ApproveLogo (without designer ever uploading Final) do **not** have designer pricing set and are **not** eligible for designer payout.

### 6.5 Designer Pricing Flow

```
Designer uploads Final
    → FileService requires designCategory, designType, proposedPrice
    → SubmitDesignerPricingAsync
        → StandardPrice from DesignPricing
        → If proposed == standard → AutoApproved
        → If differs → PendingApproval, notify Admin
    → Admin approves via ApproveDesignerPriceAsync
    → Order completed → BillingEligible, included in designer invoice
```

---

## 7. Notification System

### 7.1 NotificationService (DB Notifications)

- Creates records in `Notifications` table
- `CreateNotificationAsync`, `CreateNotificationForRoleAsync`
- Types: OrderStatusChange, FileUpload, RevisionRequest, PriceApproval, Info

### 7.2 SignalR (IRealtimeEntityUpdateSender)

**Hub:** NotificationHub  
**Group:** `user-{userId}`  

| Event | When | Recipients |
|-------|------|------------|
| OrderCreated | Client creates order | Admin, SuperAdmin |
| OrderAssigned | Admin assigns designer | Designer |
| PreviewUploaded | Designer uploads Preview | Admin, SuperAdmin |
| PreviewDelivered | Admin sends files to client | Client |
| PreviewApproved | Client approves preview | Admin, SuperAdmin |
| PreviewRejected | Client requests revision | Admin, Designer |
| OrderStatusChanged | Status update | Client, Admin, Designer (role-dependent) |
| OrderUpdated | Order details changed | Role-dependent |
| InvoiceGenerated | Invoice created | Client |

### 7.3 Notification Triggers by Workflow Step

| Step | DB Notification | SignalR |
|------|------------------|---------|
| Order created | Admin, SuperAdmin | OrderCreated |
| Price approval requested | Client | - |
| Price approved | Admin | - |
| Order approved | Client | - |
| Designer assigned | Designer | OrderAssigned |
| Preview uploaded | Admin, SuperAdmin | PreviewUploaded |
| Final uploaded | Admin, SuperAdmin | - |
| Files sent to client | Client | PreviewDelivered |
| Revision requested | Admin, SuperAdmin, Designer | PreviewRejected |
| Client approves (ApproveLogo) | Admin, SuperAdmin | PreviewApproved, OrderStatusChanged |
| Admin marks Completed | Client, Designer | OrderStatusChanged |
| Designer price approval needed | Admin, SuperAdmin | - |

---

## 8. Database Structure

### 8.1 Relevant Tables

| Table | Key Columns |
|-------|-------------|
| LogoOrders | Id, ClientId, DesignerId, Status, Price, StandardPrice, ProposedPrice, ApprovedPrice, PriceApprovalStatus, PriceApproved, DesignCategory, DesignType, BillingEligible, IsDesignerInvoiced, CompletedDate, AllowUploads, RevisionCount, RevisionLimit |
| LogoFiles | Id, OrderId, FileName, FilePath, FileType, IsVisibleToClient, IsAdminApproved, IsFinalVersion, UploadedBy, ApprovedBy, VersionNumber, PreviewBatchId |
| OrderStatusHistories | OrderId, PreviousStatus, NewStatus, ChangedBy |
| OrderRevisions | OrderId, Instructions, RequestedBy, IsResolved |
| RevisionFiles | RevisionId, FilePath, FileType |
| DesignPricings | DesignCategory, DesignType, DefaultPrice, IsActive |
| DesignerInvoices | DesignerId, TotalAmount, Status, BillingPeriod |
| DesignerInvoiceItems | DesignerInvoiceId, OrderId, Amount |
| ClientGalleries | OrderId, ClientId, FileId, FilePath |
| Notifications | UserId, Title, Message, Type, ReferenceType, ReferenceId |
| Users, Roles, ClientProfiles, DesignerProfiles | Standard auth/profile |

### 8.2 Relationships

- LogoOrder → ClientProfile, DesignerProfile (optional)
- LogoOrder → LogoFiles (1:N)
- LogoOrder → OrderRevisions (1:N)
- OrderRevision → RevisionFiles (1:N)
- LogoOrder → OrderStatusHistories (1:N)
- LogoFile → ClientGallery (1:1 when approved)
- LogoOrder → DesignerInvoiceItem (via DesignerInvoice)
- DesignPricing: standalone config table

---

## 9. Full Workflow Summary

### 9.1 End-to-End Flow

```
1. Client Order Created
   API: POST /api/orders or POST /api/orders/with-files
   Service: OrderService.CreateOrderAsync / CreateOrderWithFilesAsync
   Status: WaitingForAdminApproval
   Role: Client

2. (Optional) Price Approval
   API: POST /api/orders/{id}/request-price-approval → PriceApprovalPending
   API: POST /api/orders/{id}/approve-price → WaitingForAdminApproval
   Role: Admin, Client

3. Admin Approves Order
   API: POST /api/orders/{id}/approve
   Service: OrderService.ApproveOrderAsync
   Status: InProgress
   Role: Admin

4. Admin Assigns Designer
   API: POST /api/orders/{id}/assign
   Service: OrderService.AssignOrderToDesignerAsync
   Role: Admin

5. Designer Uploads Files
   API: POST /api/files/upload/{orderId} or upload-multiple
   Service: FileService.UploadFileAsync
   FileType: Preview (Temporary) or Final (Permanent + pricing)
   Role: Designer

6. Admin QA / Sends to Client
   API: POST /api/orders/{id}/send-files-to-client or send-preview-batch
   Service: OrderService.SendFilesToClientAsync
   Status: PreviewDelivered
   Role: Admin

7. Client Review
   - Option A: Approve
     API: PUT /api/orders/{id}/status { status: "ClientApproved" }
       OR POST /api/revisions/{orderId}/approve-logo
     Service: OrderService.UpdateOrderStatusAsync or RevisionService.ApproveLogoAsync
     Status: ClientApproved (or Completed if Admin approves)
     Role: Client (or Admin for direct Complete)
   - Option B: Request Revision
     API: POST /api/revisions/{orderId}/request
     Service: RevisionService.RequestRevisionAsync
     Status: RevisionRequested
     Role: Client

8. (If Revision) Designer Uploads New Preview
   Same as step 5; Admin sends again (step 6)

9. Client Approves (ApproveLogo)
   API: POST /api/revisions/{orderId}/approve-logo
   Service: RevisionService.ApproveLogoAsync
   - Preview → Final (move to Permanent)
   - Revision files deleted
   - ClientGallery entries created
   Status: ClientApproved or Completed
   Role: Client or Admin

10. Admin Marks Completed (if ClientApproved)
    API: PUT /api/orders/{id}/status { status: "Completed" }
    Service: OrderService.UpdateOrderStatusAsync
    Status: Completed
    Role: Admin

11. Designer Payout (if designer uploaded Final with pricing)
    - Admin approves price: ApproveDesignerPriceAsync (if needed)
    - Order: PriceApproved, BillingEligible, CompletedDate set
    - Generate designer invoice for period
```

### 9.2 Key API Endpoints Summary

| Action | Method | Endpoint |
|--------|--------|----------|
| Create order | POST | /api/orders |
| Create order with files | POST | /api/orders/with-files |
| Get order | GET | /api/orders/{id} |
| Request price approval | POST | /api/orders/{id}/request-price-approval |
| Approve price | POST | /api/orders/{id}/approve-price |
| Approve order | POST | /api/orders/{id}/approve |
| Assign designer | POST | /api/orders/{id}/assign |
| Update status | PUT | /api/orders/{id}/status |
| Send files to client | POST | /api/orders/{id}/send-files-to-client |
| Send preview batch | POST | /api/orders/{id}/send-preview-batch |
| Upload file | POST | /api/files/upload/{orderId} |
| Upload multiple | POST | /api/files/upload-multiple/{orderId} |
| Get order files | GET | /api/files/order/{orderId} |
| Approve file | PUT | /api/files/{id}/approve |
| Request revision | POST | /api/revisions/{orderId}/request |
| Approve logo | POST | /api/revisions/{orderId}/approve-logo |
| Approve designer price | (DesignerPayoutController) | designer-payout/orders/{orderId}/approve-price |

---

*Document generated from codebase analysis. No code was modified.*
