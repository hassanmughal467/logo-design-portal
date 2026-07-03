# Logo Design Portal – Complete System Analysis for Automation Testing

**Document Version:** 1.0  
**Date:** March 7, 2025  
**Purpose:** Comprehensive analysis to build full automation testing for the entire application (Frontend Angular + Backend .NET API)

---

## 1. SYSTEM ARCHITECTURE

### 1.1 Frontend Framework
- **Framework:** Angular with PrimeNG UI components
- **Location:** `Frontend/src/app/`
- **Lazy Loading:** Feature modules are lazy-loaded via `loadChildren`
- **State:** Token in memory + `localStorage` for persistence; `BehaviorSubject` for current user

### 1.2 Backend Framework
- **Framework:** ASP.NET Core Web API (.NET)
- **Architecture:** Clean Architecture (API, Application, Domain, Infrastructure)
- **API Project:** `LogoDesignPortal.API`
- **JSON:** camelCase serialization for Angular compatibility

### 1.3 Authentication Method
- **Scheme:** JWT Bearer tokens
- **Storage:** `localStorage` (token, refreshToken, expiresAt, user)
- **Token Flow:** `TokenInterceptor` adds `Authorization: Bearer <token>` to all requests
- **Refresh:** On 401, attempts refresh via `POST /api/auth/refresh-token`; retries original request; on failure, logs out
- **SignalR:** Token passed via query string `access_token` (WebSockets don't support custom headers)
- **Config:** `appsettings.json` → `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`, `Jwt:AccessTokenExpiryHours`, `Jwt:RefreshTokenExpiryHours`

### 1.4 Database Technology
- **Provider:** MySQL (production) / SQLite (dev)
- **Connection:** `ConnectionStrings:DefaultConnection`
- **Migrations:** `LogoDesignPortal.Infrastructure/Migrations/`
- **Startup:** Migrations applied on startup; SuperAdmin seeded if missing

### 1.5 File Storage System
- **Base Path:** `FileStorage:Path` (default `"Files"`)
- **Layout:**
  - `Files/` – Base
  - `Files/Temporary/` – Preview and revision files (ephemeral)
  - `Files/Permanent/` – Final/delivered files
- **Orphan Cleanup:** `OrphanFileCleanupService` runs every 24h; deletes orphan files in `Temporary`
- **Initialization:** `FileStorageInitializer` creates directories at startup

### 1.6 Real-Time Systems
- **Technology:** SignalR
- **Hub:** `NotificationHub` at `/hubs/notifications`
- **Auth:** `[Authorize]` on hub; token from query string
- **Groups:** `JoinUserGroup(userId)` → `user-{userId}`
- **Events:**
  - `ReceiveNotification` – In-app notifications
  - `OrderCreated`, `OrderAssigned`, `PreviewUploaded`, `OrderStatusChanged`
  - `PreviewApproved`, `PreviewRejected`, `PreviewDelivered`, `OrderUpdated`, `InvoiceGenerated`

### 1.7 Frontend–Backend Communication
- **Base URL:** `environment.apiUrl` (e.g. `https://localhost:44398`)
- **HTTP Client:** `ApiService` with `HttpClient`
- **Interceptors:** `TokenInterceptor` (Bearer token, 401 retry with refresh), `ErrorInterceptor` (global error handling)
- **CORS:** `AllowAdmin` policy with credentials for SignalR

---

## 2. USER ROLES

### 2.1 Identified Roles (from `SystemRoles` enum)
| Role       | Value |
|-----------|-------|
| SuperAdmin| 1     |
| Admin     | 2     |
| Designer  | 3     |
| Client    | 4     |

### 2.2 Role–Permission Matrix

| Permission            | SuperAdmin | Admin | Designer | Client |
|----------------------|------------|-------|----------|--------|
| CreateUser           | ✓          | ✗     | ✗        | ✗      |
| ViewUsers            | ✓          | ✓     | ✗        | ✗      |
| UpdateUser           | ✓          | ✗     | ✗        | ✗      |
| DeleteUser           | ✓          | ✗     | ✗        | ✗      |
| CreateDesignerProfile| ✓          | ✓*    | ✗        | ✗      |
| ViewDesignerProfiles | ✓          | ✓*    | ✗        | ✗      |
| UpdateDesignerProfile| ✓          | ✓*    | ✗        | ✗      |
| ViewAllOrders        | ✓          | ✓*    | ✗        | ✗      |
| AssignOrder          | ✓          | ✓*    | ✗        | ✗      |
| UpdateOrderStatus    | ✓          | ✓*    | ✗        | ✗      |
| ManagePermissions    | ✓          | ✗     | ✗        | ✗      |
| UploadFile           | ✓          | ✓     | ✓        | ✓      |
| DownloadFile         | ✓          | ✓     | ✓        | ✓      |
| DeleteFile           | ✓          | ✓     | ✓        | ✓      |

*Admin requires permission granted by SuperAdmin via RolePermission.

### 2.3 Restricted Actions by Role
- **Client:** Cannot see designer identity (shows "Company Design Team"); cannot message designer directly; cannot access Users, Designers, Clients, Permissions
- **Designer:** Cannot see client identity; cannot message client directly; cannot access Users, Designers, Clients, Permissions
- **Admin:** Cannot create/update/delete users; cannot manage permissions; client email/phone masked in order views
- **SuperAdmin:** Full access; bypasses all `[RequirePermission]` checks

### 2.4 Visible UI Sections (from `main-layout.component.ts`)

| Section           | SuperAdmin | Admin | Designer | Client |
|-------------------|------------|-------|----------|--------|
| Dashboard         | ✓          | ✓     | ✓        | ✓      |
| Orders            | ✓          | ✓     | ✓        | ✓      |
| Users             | ✓          | ✓     | ✗        | ✗      |
| Clients           | ✓          | ✓     | ✗        | ✗      |
| Designers         | ✓          | ✓     | ✗        | ✗      |
| Permissions       | ✓          | ✗     | ✗        | ✗      |
| Projects          | ✓          | ✓     | ✓        | ✓      |
| Messages          | ✓          | ✓     | ✓        | ✓      |
| Reviews           | ✓          | ✓     | ✓        | ✓      |
| Files             | ✓          | ✓     | ✓        | ✓      |
| Invoices          | ✓          | ✓     | ✓        | ✓      |
| Analytics         | ✓          | ✓     | ✓        | ✓      |
| Financial         | ✓          | ✓     | ✓        | ✓      |
| Notifications     | ✓          | ✓     | ✓        | ✓      |
| Settings          | ✓          | ✓     | ✓        | ✓      |
| Gallery           | ✗          | ✗     | ✗        | ✓      |

---

## 3. APPLICATION MODULES

| Module        | Route       | Guard        | Purpose |
|---------------|-------------|--------------|---------|
| Auth          | `/auth/*`   | None         | Login, register, forgot-password, reset-password |
| Dashboard     | `/dashboard`| AuthGuard    | Main dashboard |
| Users         | `/users`    | RoleGuard [SuperAdmin, Admin] | User management |
| Orders        | `/orders`   | AuthGuard    | Order CRUD, file upload, order detail |
| Designers     | `/designers`| RoleGuard [SuperAdmin, Admin] | Designer profiles |
| Permissions   | `/permissions` | RoleGuard [SuperAdmin] | Role permissions |
| Files         | `/files`    | AuthGuard    | File management |
| Clients       | `/clients`  | RoleGuard [SuperAdmin, Admin] | Client profiles |
| Projects      | `/projects` | AuthGuard    | Project management |
| Invoices      | `/invoices` | AuthGuard    | Invoice list |
| Analytics     | `/analytics`| AuthGuard    | Analytics |
| Financial     | `/financial`| AuthGuard    | Financial overview |
| Messages      | `/messages` | AuthGuard    | Messaging |
| Reviews       | `/reviews`  | AuthGuard    | Reviews |
| Settings      | `/settings` | AuthGuard    | Settings |
| Notifications | `/notifications` | AuthGuard | Notification center |
| Gallery       | `/gallery`  | RoleGuard [Client] | Client gallery |

### Module Details (APIs used)
- **Auth:** `POST auth/login`, `POST auth/register`, `POST auth/refresh-token`, `POST auth/forgot-password`, `POST auth/reset-password-with-token`, `POST auth/change-password`
- **Orders:** `GET/POST orders`, `GET orders/{id}`, `GET orders/my-orders`, `GET orders/assigned-orders`, `POST orders/{id}/assign`, `PUT orders/{id}/status`, `POST orders/{id}/approve`, `POST orders/{id}/send-files-to-client`, `POST orders/{id}/send-preview-batch`, `POST orders/{id}/cancel`, `POST orders/{id}/archive`, `POST orders/{id}/refund`
- **Files:** `POST files/upload/{orderId}`, `POST files/upload-multiple/{orderId}`, `PUT files/{id}/approve`, `GET files/{id}/download`, `GET files/order/{orderId}`, `DELETE files/{id}`
- **Messages:** `POST messages`, `GET messages`, `GET messages/order/{orderId}`, `PUT messages/{id}/read`, `POST messages/forward`, `POST messages/reject`
- **Notifications:** `GET notifications`, `GET notifications/unread-count`, `PUT notifications/{id}/read`, `PUT notifications/mark-all-read`
- **Revisions:** `POST revisions/orders/{orderId}/request`, `GET revisions/orders/{orderId}/latest`, `POST revisions/orders/{orderId}/approve-logo`
- **Comments:** `POST comments/orders/{orderId}`, `GET comments/orders/{orderId}`, `DELETE comments/{id}`

---

## 4. ORDER / WORKFLOW LOGIC

### 4.1 Order Statuses (from `OrderStatus` enum)
| Status                 | Value | Terminal |
|------------------------|-------|----------|
| WaitingForAdminApproval| 1     | No       |
| PriceApprovalPending   | 2     | No       |
| InProgress             | 3     | No       |
| PreviewDelivered       | 4     | No       |
| RevisionRequested      | 5     | No       |
| FinalApproved          | 6     | Yes      |
| Completed              | 7     | Yes      |
| Cancelled              | 8     | Yes      |
| Pending                | 10    | No       |
| Paid                   | 11    | No       |
| Processing             | 12    | No       |
| CancelledByUser        | 13    | Yes      |
| CancelledByAdmin       | 14    | Yes      |
| Refunded               | 15    | Yes      |
| Failed                 | 16    | No       |
| Archived               | 17    | No       |

### 4.2 Terminal (Locked) Statuses
Orders in these statuses cannot be modified (no files, comments, messages, revisions):
- Completed, FinalApproved, Cancelled, CancelledByUser, CancelledByAdmin, Refunded

### 4.3 Status Transition Rules (from `GetAllowedStatusesForRole`)

**Client:**
- From `PreviewDelivered` → `RevisionRequested` or `FinalApproved`
- From `WaitingForAdminApproval`, `PriceApprovalPending`, `Pending` (own order) → `Cancelled`

**Designer:**
- From `InProgress` or `RevisionRequested` → `PreviewDelivered`

**Admin/SuperAdmin:**
- Can transition to any status

### 4.4 Status Flow Diagram (Text)
```
[Client] Create Order
    ↓
WaitingForAdminApproval
    ↓ [Admin assigns designer]
InProgress
    ↓ [Designer uploads preview]
PreviewDelivered
    ↓ [Client chooses]
    ├─→ RevisionRequested → [Designer uploads] → PreviewDelivered (loop)
    └─→ FinalApproved → [Admin sends files] → Completed

[Admin can also:]
- RequestPriceApproval → PriceApprovalPending → [Client approves] → InProgress
- CancelOrder → CancelledByAdmin / CancelledByUser
- ArchiveOrder → Archived
- RefundOrder → Refunded
```

---

## 5. API ENDPOINTS

### Auth (`/api/auth`)
| Method | Route | Purpose | Auth | Roles |
|--------|-------|---------|------|-------|
| POST | `/login` | Login | No | - |
| POST | `/register` | Register | No | - |
| POST | `/refresh-token` | Refresh token | No | - |
| POST | `/change-password` | Change own password | Yes | All |
| POST | `/reset-password` | Reset user password | Yes | SuperAdmin |
| POST | `/reset-superadmin-password` | Reset SuperAdmin to default | No | - |
| POST | `/forgot-password` | Request reset email | No | - |
| POST | `/reset-password-with-token` | Reset with token | No | - |

### Orders (`/api/orders`)
| Method | Route | Purpose | Auth | Roles |
|--------|-------|---------|------|-------|
| POST | `/` | Create order | Yes | Client |
| POST | `/with-files` | Create order with files | Yes | Client |
| GET | `/{id}` | Get order by ID | Yes | Owner/Admin/Designer |
| GET | `/my-orders` | Get client orders | Yes | Client |
| GET | `/assigned-orders` | Get designer orders | Yes | Designer |
| GET | `/` | Get all orders | Yes | ViewAllOrders |
| POST | `/{id}/assign` | Assign designer | Yes | AssignOrder |
| PUT | `/{id}/status` | Update status | Yes | All |
| POST | `/{id}/request-price-approval` | Request price approval | Yes | SuperAdmin, Admin |
| POST | `/{id}/approve-price` | Approve price | Yes | Client |
| POST | `/{id}/approve` | Approve order | Yes | SuperAdmin, Admin |
| POST | `/{id}/send-files-to-client` | Send files to client | Yes | SuperAdmin, Admin |
| POST | `/{id}/send-preview-batch` | Send preview batch | Yes | SuperAdmin, Admin |
| PUT | `/{id}` | Update order | Yes | Client |
| POST | `/{id}/cancel` | Cancel order | Yes | Client, Admin, SuperAdmin |
| POST | `/{id}/archive` | Archive order | Yes | Admin, SuperAdmin |
| POST | `/{id}/unarchive` | Unarchive order | Yes | Admin, SuperAdmin |
| POST | `/{id}/refund` | Refund order | Yes | Admin, SuperAdmin |
| PUT | `/{id}/allow-uploads` | Set allow uploads | Yes | Admin, SuperAdmin |
| GET | `/{id}/logs` | Get order logs | Yes | All |
| DELETE | `/{id}` | Delete (legacy, maps to cancel) | Yes | Client, Admin, SuperAdmin |

### Files (`/api/files`)
| Method | Route | Purpose | Auth | Roles |
|--------|-------|---------|------|-------|
| POST | `/upload/{orderId}` | Upload file | Yes | All |
| POST | `/upload-multiple/{orderId}` | Upload multiple | Yes | All |
| PUT | `/{id}/approve` | Approve file | Yes | SuperAdmin, Admin |
| GET | `/{id}/download` | Download file | Yes | All (role-filtered) |
| GET | `/` | Get all files | Yes | All |
| GET | `/order/{orderId}` | Get order files | Yes | All |
| DELETE | `/{id}` | Delete file | Yes | All |

### Revisions (`/api/revisions`)
| Method | Route | Purpose | Auth | Roles |
|--------|-------|---------|------|-------|
| POST | `/orders/{orderId}/request` | Request revision | Yes | Client |
| GET | `/orders/{orderId}/latest` | Get latest revision | Yes | Admin, SuperAdmin, Designer |
| POST | `/orders/{orderId}/approve-logo` | Approve logo | Yes | Client, Admin, SuperAdmin |
| GET | `/orders/{orderId}/can-request` | Can request revision | Yes | Client |
| GET | `/orders/{orderId}/can-approve` | Can approve logo | Yes | Client, Admin, SuperAdmin |

### Messages (`/api/messages`)
| Method | Route | Purpose | Auth | Roles |
|--------|-------|---------|------|-------|
| POST | `/` | Create message | Yes | All |
| GET | `/{id}` | Get message | Yes | All |
| GET | `/` | Get messages | Yes | All |
| GET | `/order/{orderId}` | Get by order | Yes | All |
| PUT | `/{id}/read` | Mark read | Yes | All |
| POST | `/forward` | Forward message | Yes | SuperAdmin, Admin |
| POST | `/reject` | Reject message | Yes | SuperAdmin, Admin |

### Notifications (`/api/notifications`)
| Method | Route | Purpose | Auth | Roles |
|--------|-------|---------|------|-------|
| GET | `/` | Get notifications | Yes | All |
| GET | `/unread-count` | Unread count | Yes | All |
| GET | `/{id}` | Get by ID | Yes | All |
| PUT | `/{id}/read` | Mark read | Yes | All |
| PUT | `/mark-all-read` | Mark all read | Yes | All |
| GET | `/debug` | Debug info | Yes | SuperAdmin |

### Users (`/api/users`)
| Method | Route | Purpose | Auth | Roles |
|--------|-------|---------|------|-------|
| POST | `/` | Create user | Yes | SuperAdmin |
| GET | `/{id}` | Get user | Yes | Self or Admin |
| GET | `/` | Get all users | Yes | SuperAdmin, Admin |
| PUT | `/{id}` | Update user | Yes | SuperAdmin |
| POST | `/designer-profiles` | Create designer profile | Yes | CreateDesignerProfile |
| GET | `/designer-profiles/{userId}` | Get designer profile | Yes | ViewDesignerProfiles |
| GET | `/designer-profiles` | Get all designer profiles | Yes | SuperAdmin, Admin + ViewDesignerProfiles |
| PUT | `/{id}/profile` | Update profile | Yes | Self or Admin |
| DELETE | `/{id}` | Delete user | Yes | SuperAdmin |
| PUT | `/{id}/activate` | Reactivate user | Yes | SuperAdmin |
| GET | `/clients/{id}/detail` | Client detail | Yes | SuperAdmin, Admin |
| GET | `/designers/{id}/detail` | Designer detail | Yes | SuperAdmin, Admin |

### Comments (`/api/comments`)
| Method | Route | Purpose | Auth | Roles |
|--------|-------|---------|------|-------|
| POST | `/orders/{orderId}` | Create comment | Yes | All |
| GET | `/orders/{orderId}` | Get comments | Yes | All |
| DELETE | `/{id}` | Delete comment | Yes | All |

### Invoices (`/api/invoices`)
| Method | Route | Purpose | Auth | Roles |
|--------|-------|---------|------|-------|
| POST | `/` | Create invoice | Yes | SuperAdmin, Admin |
| GET | `/{id}` | Get invoice | Yes | All |
| GET | `/` | Get invoices | Yes | All |
| PUT | `/{id}` | Update invoice | Yes | SuperAdmin, Admin |
| PUT | `/{id}/mark-paid` | Mark paid | Yes | SuperAdmin, Admin |
| POST | `/{id}/send` | Send invoice | Yes | SuperAdmin, Admin |
| GET | `/{id}/logs` | Invoice logs | Yes | All |
| GET | `/statistics` | Statistics | Yes | All |
| GET | `/report` | Download report | Yes | All |
| GET | `/{id}/download` | Download PDF | Yes | All |

### Permissions (`/api/permissions`)
| Method | Route | Purpose | Auth | Roles |
|--------|-------|---------|------|-------|
| GET | `/` | Get all permissions | Yes | SuperAdmin |
| GET | `/role/{roleId}` | Get role permissions | Yes | SuperAdmin |
| POST | `/assign` | Assign permission | Yes | SuperAdmin |
| DELETE | `/revoke` | Revoke permission | Yes | SuperAdmin |

### Settings (`/api/settings`)
| Method | Route | Purpose | Auth | Roles |
|--------|-------|---------|------|-------|
| GET | `/` | Get settings | Yes | SuperAdmin, Admin |
| POST | `/business` | Update business | Yes | SuperAdmin, Admin |
| POST | `/brand` | Update brand | Yes | SuperAdmin, Admin |
| POST | `/logo` | Upload logo | Yes | SuperAdmin, Admin |
| POST | `/invoice-template` | Invoice template | Yes | SuperAdmin, Admin |
| POST | `/payment-methods` | Payment methods | Yes | SuperAdmin, Admin |
| POST | `/invoice` | Invoice settings | Yes | SuperAdmin |
| POST | `/notifications` | Notification prefs | Yes | SuperAdmin, Admin |

### Gallery (`/api/gallery`)
| Method | Route | Purpose | Auth | Roles |
|--------|-------|---------|------|-------|
| GET | `/my-gallery` | Get client gallery | Yes | Client |
| GET | `/{id}` | Get gallery item | Yes | Client |

### Payments (`/api/payments`)
| Method | Route | Purpose | Auth | Roles |
|--------|-------|---------|------|-------|
| POST | `/` | Create payment | Yes | All |
| GET | `/{id}` | Get payment | Yes | All |
| GET | `/invoice/{invoiceId}` | Get by invoice | Yes | All |
| POST | `/link` | Generate payment link | Yes | All |
| POST | `/process` | Process payment | Yes | All |
| GET | `/bank-details` | Bank details | Yes | All |
| POST | `/verify/paypal` | Verify PayPal | Yes | All |
| PUT | `/{id}/status` | Update status | Yes | SuperAdmin, Admin |
| POST | `/webhook/paypal` | PayPal webhook | No | - |

### Reviews (`/api/reviews`)
| Method | Route | Purpose | Auth | Roles |
|--------|-------|---------|------|-------|
| POST | `/` | Create review | Yes | Client |
| GET | `/` | Get reviews | Yes | All |
| GET | `/{id}` | Get review | Yes | All |
| GET | `/order/{orderId}` | Get by order | Yes | All |

### Audit Logs (`/api/auditlogs`)
| Method | Route | Purpose | Auth | Roles |
|--------|-------|---------|------|-------|
| GET | `/` | Get audit logs | Yes | SuperAdmin, Admin |
| GET | `/{entityType}/{entityId}` | Get entity logs | Yes | SuperAdmin, Admin |

---

## 6. UI ACTIONS

| Action | UI Location | API Called | Role |
|--------|-------------|------------|------|
| Login | Auth/Login | POST auth/login | - |
| Register | Auth/Register | POST auth/register | - |
| Create Order | Orders/Create | POST orders or POST orders/with-files | Client |
| Assign Designer | Order Detail | POST orders/{id}/assign | Admin, SuperAdmin |
| Request Price Approval | Order Detail | POST orders/{id}/request-price-approval | Admin, SuperAdmin |
| Approve Price | Order Detail | POST orders/{id}/approve-price | Client |
| Approve Order | Order Detail | POST orders/{id}/approve | Admin, SuperAdmin |
| Send Files to Client | Order Detail | POST orders/{id}/send-files-to-client | Admin, SuperAdmin |
| Send Preview Batch | Order Detail | POST orders/{id}/send-preview-batch | Admin, SuperAdmin |
| Update Order Status | Order Detail | PUT orders/{id}/status | All |
| Cancel Order | Order Detail | POST orders/{id}/cancel | Client, Admin, SuperAdmin |
| Archive Order | Order Detail | POST orders/{id}/archive | Admin, SuperAdmin |
| Refund Order | Order Detail | POST orders/{id}/refund | Admin, SuperAdmin |
| Upload File | Order Detail / File Upload | POST files/upload/{orderId} | All |
| Approve File | Order Detail | PUT files/{id}/approve | Admin, SuperAdmin |
| Download File | Order Detail / File List | GET files/{id}/download | All |
| Delete File | Order Detail | DELETE files/{id} | All |
| Request Revision | Order Detail | POST revisions/orders/{orderId}/request | Client |
| Approve Logo | Order Detail | POST revisions/orders/{orderId}/approve-logo | Client, Admin, SuperAdmin |
| Add Comment | Order Detail | POST comments/orders/{orderId} | All |
| Delete Comment | Order Detail | DELETE comments/{id} | All |
| Create Message | Messages | POST messages | All |
| Forward Message | Messages | POST messages/forward | Admin, SuperAdmin |
| Reject Message | Messages | POST messages/reject | Admin, SuperAdmin |
| Mark Notification Read | Notifications / Header | PUT notifications/{id}/read | All |
| Mark All Notifications Read | Notifications | PUT notifications/mark-all-read | All |
| Create User | Users | POST users | SuperAdmin |
| Update User | Users | PUT users/{id} | SuperAdmin |
| Delete User | Users | DELETE users/{id} | SuperAdmin |
| Create Designer Profile | Designers | POST users/designer-profiles | Admin, SuperAdmin |
| Assign Permission | Permissions | POST permissions/assign | SuperAdmin |
| Revoke Permission | Permissions | DELETE permissions/revoke | SuperAdmin |
| Create Invoice | Invoices | POST invoices | Admin, SuperAdmin |
| Mark Invoice Paid | Invoices | PUT invoices/{id}/mark-paid | Admin, SuperAdmin |
| Create Review | Reviews | POST reviews | Client |
| Global Search | Header | Navigate to /orders?search= | All |

---

## 7. PERMISSION LOGIC (Extracted from Code)

1. **Client cannot see designer identity** – `OrderResponseDto` masks designer; shows "Company Design Team" (`OrderService.GetOrderByIdAsync`, `GetOrdersByClientAsync`).
2. **Designer cannot see client identity** – `Client` set to `null` for Designer in order responses (`OrderService.GetOrderByIdAsync`, `GetOrdersByDesignerAsync`, `GetAllOrdersAsync`).
3. **Admin acts as mediator for messages** – Client and Designer messages require admin approval; `RequiresAdminApproval = true`; Admin/SuperAdmin forward or reject (`MessageService.CreateMessageAsync`).
4. **Client cannot message designer directly** – Throws `InvalidOperationException` ("Clients cannot message designers directly").
5. **Designer cannot message client directly** – Throws `InvalidOperationException` ("Designers cannot message clients directly").
6. **Designer uploads never visible to client until admin approval** – `isVisibleToClient = false` for Designer uploads; only client Reference uploads are visible immediately (`FileService.UploadFileAsync`).
7. **Order access by role** – Client: own orders only; Designer: assigned orders only; Admin/SuperAdmin: all orders (with ViewAllOrders permission for Admin).
8. **Terminal orders are read-only** – No files, comments, messages, revisions for Completed, FinalApproved, Cancelled, CancelledByUser, CancelledByAdmin, Refunded (`OrderLockingHelper`).
9. **SuperAdmin bypasses RequirePermission** – All `[RequirePermission]` checks skip for SuperAdmin (`RequirePermissionAttribute`).

---

## 8. FILE MANAGEMENT

### 8.1 Upload Endpoints
- `POST /api/files/upload/{orderId}` – Single file
- `POST /api/files/upload-multiple/{orderId}` – Multiple files
- `POST /api/orders/with-files` – Create order with reference files

### 8.2 Storage Location
- **Reference:** `Files/` (base)
- **Preview/Revision:** `Files/Temporary/`
- **Final:** `Files/Permanent/`

### 8.3 File Validation
- **Allowed extensions:** `.jpg`, `.jpeg`, `.png`, `.gif`, `.webp` (images), `.svg`, `.pdf`, `.ai`, `.eps`, `.psd` (vectors)
- **Image max size:** 10MB
- **Vector max size:** 25MB
- **Request limit:** 50MB per request

### 8.4 Approval Logic
- Only Admin/SuperAdmin can approve files via `PUT files/{id}/approve`
- Designer previews require admin approval before client visibility
- Client reference uploads are auto-approved and visible immediately

---

## 9. NOTIFICATIONS

### 9.1 Channels
- **SignalR:** Real-time push via `ReceiveNotification` and order events
- **In-app:** Notification list, unread count, mark read
- **Polling fallback:** When SignalR fails

### 9.2 Notification Triggers (from code)
| Trigger | Recipients | Event |
|---------|------------|-------|
| Order Created | Admin, SuperAdmin | OrderStatusChange |
| Order Assigned | Designer | OrderStatusChange |
| Preview Uploaded | Admin, SuperAdmin | OrderStatusChange |
| Client Approves Preview (FinalApproved) | Admin, SuperAdmin, Designer | OrderStatusChange |
| Order Status Changed | Client (if not self) | OrderStatusChange |
| Preview Delivered to Client | Client | OrderStatusChange |
| New Message (from Client) | Admin, SuperAdmin | Info |
| New Message (from Designer) | Admin, SuperAdmin | Info |
| New Message (from Admin) | Recipient | Info |
| Invoice Generated | Client | - |
| Price Approval Requested | Client | PriceApproval |
| Revision Request | Admin, SuperAdmin, Designer | RevisionRequest |

### 9.3 SignalR Events
- `OrderCreated`, `OrderAssigned`, `PreviewUploaded`, `OrderStatusChanged`
- `PreviewApproved`, `PreviewRejected`, `PreviewDelivered`, `OrderUpdated`, `InvoiceGenerated`

---

## 10. DATABASE MODELS

### 10.1 Main Entities
| Entity | Purpose |
|--------|---------|
| User | Auth, roles, profiles |
| Role | RBAC |
| Permission | Granular permissions |
| RolePermission | Role–permission mapping |
| ClientProfile | Client company/contact |
| DesignerProfile | Designer profile |
| LogoOrder | Orders |
| LogoFile | Files (Reference, Preview, Final, Revision) |
| OrderStatusHistory | Status changes |
| OrderLog | Order audit |
| OrderRevision | Revision requests |
| RevisionFile | Revision files |
| OrderComment | Comments |
| Notification | Notifications |
| Message | Messages |
| Invoice | Invoices |
| InvoiceOrder | Invoice–order link |
| Payment | Payments |
| Settings | Key-value settings |
| AuditLog | Audit trail |
| ClientGallery | Client gallery |
| Review | Reviews |

### 10.2 Relationships
- User → Role (many-to-one)
- User → ClientProfile (one-to-one)
- User → DesignerProfile (one-to-one)
- LogoOrder → ClientProfile (many-to-one)
- LogoOrder → DesignerProfile (many-to-one)
- LogoOrder → LogoFile (one-to-many)
- LogoOrder → OrderStatusHistory (one-to-many)
- LogoOrder → OrderComment (one-to-many)
- LogoOrder → OrderRevision (one-to-many)
- Message → Order (optional)
- Notification → User (many-to-one)
- Invoice → InvoiceOrder → LogoOrder

---

## 11. SECURITY RULES

### 11.1 Authentication
- JWT Bearer; HMAC-SHA256
- Claims: NameIdentifier, Email, Name, Role
- Refresh tokens stored in DB

### 11.2 Token Handling
- Stored in `localStorage` (token, refreshToken, expiresAt)
- In-memory cache for fast access
- `TokenInterceptor` adds Bearer header
- 401 triggers refresh, then retry; on failure, logout

### 11.3 Session Handling
- Access token expiry: configurable (default 1h)
- Refresh token expiry: configurable (default 1h)
- Session survives page refresh via localStorage

### 11.4 Access Control
- `[Authorize]` on protected endpoints
- `[Authorize(Roles = "...")]` for role checks
- `[RequirePermission("...")]` for permission checks (SuperAdmin bypass)
- Resource-level checks in services (e.g. order ownership)

### 11.5 Rate Limiting
- **General:** 60 requests/minute per IP+path
- **Auth endpoints:** 5 requests/minute per IP+path
- Returns 429 Too Many Requests when exceeded

---

## 12. ERROR HANDLING

### 12.1 API Error Responses
- **401 Unauthorized:** Invalid/expired token
- **403 Forbidden:** Authenticated but no permission (`ForbiddenAccessException`)
- **404 NotFound:** Resource not found
- **400 BadRequest:** Validation, `InvalidOperationException`, `ArgumentException`
- **429 Too Many Requests:** Rate limit exceeded
- **500 Internal Server Error:** Unhandled exceptions

### 12.2 Exception Middleware
- `ForbiddenAccessException` → 403
- `UnauthorizedAccessException` → 401 (or 500 for file access errors)
- `InvalidOperationException` → 400
- `ArgumentException` → 400
- Default → 500 with `{ error, detail }`

### 12.3 Frontend Error Handling
- `ErrorInterceptor` for global handling
- `TokenInterceptor` retries on 401 after refresh
- Toast messages for user feedback (PrimeNG MessageService)

---

## 13. TESTING TARGETS

1. **Authentication tests** – Login, register, refresh, forgot/reset password, change password
2. **Role permission tests** – Route guards, API role restrictions, permission matrix
3. **Order lifecycle tests** – Create, assign, status transitions, cancel, archive, refund
4. **File upload tests** – Single/multiple upload, validation, approval, download
5. **Notification tests** – Create, read, SignalR delivery
6. **API validation tests** – Invalid input, missing required fields
7. **Security tests** – Unauthorized access, cross-role access, token expiry
8. **Message workflow tests** – Client/Designer mediation, forward, reject
9. **Revision workflow tests** – Request, approve logo
10. **Invoice tests** – Create, mark paid, download
11. **Permission assignment tests** – Assign, revoke, Admin access
12. **Rate limiting tests** – Auth and general limits
13. **Error handling tests** – 400, 401, 403, 404, 500
14. **Real-time tests** – SignalR connection, event delivery
15. **End-to-end workflow tests** – Full order flow from creation to completion

---

## 14. AUTOMATION TEST SCENARIOS (40+)

### Authentication
1. Client logs in successfully with valid credentials
2. Login fails with invalid credentials
3. Register creates new client and returns token
4. Register fails with duplicate email
5. Refresh token returns new access token
6. Expired token triggers refresh and retries request
7. Forgot password sends reset email/link
8. Reset password with token succeeds
9. Change password succeeds for authenticated user
10. Logout clears token and redirects to login

### Role & Permission
11. Client cannot access /users
12. Designer cannot access /designers
13. Admin with ViewAllOrders sees all orders
14. Admin without AssignOrder gets 403 on assign
15. SuperAdmin can access all routes
16. Client sees "Company Design Team" instead of designer name
17. Designer does not see client info in orders

### Order Lifecycle
18. Client creates order successfully
19. Client creates order with files successfully
20. Admin assigns designer to order
21. Order status changes from WaitingForAdminApproval to InProgress on assign
22. Designer uploads preview file
23. Order status changes to PreviewDelivered on designer upload
24. Admin approves preview and sends to client
25. Client approves logo (FinalApproved)
26. Client requests revision from PreviewDelivered
27. Client cancels order in WaitingForAdminApproval
28. Admin cancels order
29. Admin archives order
30. Admin refunds order
31. Locked order rejects file upload
32. Locked order rejects comment

### File Management
33. Client uploads reference file
34. Designer uploads preview file
35. Admin approves file
36. User downloads file they have access to
37. User gets 403 downloading file without access
38. File validation rejects invalid extension
39. File validation rejects oversized file
40. Send files to client updates order

### Messages & Notifications
41. Client sends message; Admin receives notification
42. Designer sends message; Admin receives notification
43. Admin forwards message to client
44. Admin rejects message
45. Client cannot message designer directly (400)
46. Designer cannot message client directly (400)
47. Notification mark as read succeeds
48. SignalR delivers OrderCreated to Admin

### API & Security
49. Unauthenticated request to protected endpoint returns 401
50. Expired token returns 401
51. Rate limit exceeded returns 429
52. Invalid order ID returns 404
53. Cross-order access returns 403

---

## 15. AUTOMATION STRATEGY

### 15.1 Recommended Architecture

**UI Testing:**
- **Tool:** Playwright
- **Language:** TypeScript
- **Scope:** Critical user flows (login, order creation, order detail actions, file upload/download)
- **Parallel:** Run by role (Client, Admin, Designer)

**API Testing:**
- **Tool:** Playwright APIRequestContext or dedicated REST client (e.g. Supertest-style with fetch/axios)
- **Scope:** All endpoints, validation, auth, permissions
- **Data:** Fixtures for users (Client, Admin, Designer, SuperAdmin)

**E2E Workflow:**
- **Tool:** Playwright (UI + API)
- **Scope:** Full order lifecycle, message mediation, revision flow

### 15.2 Test Folder Structure
```
tests/
├── e2e/
│   ├── auth.spec.ts
│   ├── orders.spec.ts
│   ├── files.spec.ts
│   ├── messages.spec.ts
│   └── notifications.spec.ts
├── api/
│   ├── auth.api.spec.ts
│   ├── orders.api.spec.ts
│   ├── files.api.spec.ts
│   ├── users.api.spec.ts
│   └── ...
├── fixtures/
│   ├── users.ts
│   ├── orders.ts
│   └── files.ts
├── utils/
│   ├── auth.ts
│   ├── api-client.ts
│   └── test-data.ts
└── playwright.config.ts
```

### 15.3 Test Data Strategy
- **Seeded users:** SuperAdmin (superadmin@logodesign.com), Admin, Designer, Client
- **Factory:** Create orders, files, messages per test
- **Cleanup:** Delete or soft-delete test data after runs
- **Isolation:** Use unique IDs (UUIDs) to avoid conflicts

### 15.4 CI/CD Automation
- Run API tests on every PR
- Run E2E tests on merge to develop/main
- Use environment variables for API URL, test credentials
- Parallelize by test file or tag (e.g. @smoke, @regression)
- Report: HTML report, JUnit XML for CI integration

### 15.5 SignalR Testing
- Use Playwright's WebSocket support or a SignalR client
- Connect with test user token
- Assert receipt of expected events (e.g. OrderCreated, ReceiveNotification)
- Consider mocking SignalR for faster, more stable tests if needed

---

*End of System Analysis Document*
