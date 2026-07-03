# API Route Map

Verified from `Backend/src/LogoDesignPortal.API/Controllers/` and `Program.cs`. Base URL: `https://api.hawkmerchandising.com` (prod) / `https://staging-api.hawkmerchandising.com` (staging) / `https://localhost:44398` (dev).

Conventions:

- Unless stated otherwise, endpoints require a valid JWT (`[Authorize]` at controller level). "Controller auth only" means any authenticated user; the Application service then applies data-level scoping (own orders, own invoices, etc.).
- `[RequirePermission("X")]` checks the seeded permission matrix; **SuperAdmin bypasses permission checks**.
- Swagger UI is available in Development only.
- Responses are camelCase JSON with enums serialized as strings.

## Authentication — `api/auth` (`AuthController`)

| Verb | Route | Access |
|---|---|---|
| POST | `/api/auth/login` | Anonymous (rate-limited 5/min; 5 failures → 30 min lockout) |
| POST | `/api/auth/register` | Anonymous (creates Client + profile) |
| POST | `/api/auth/refresh-token` | Anonymous (body or `ldp_refresh` cookie; rotates refresh token) |
| POST | `/api/auth/logout` | Authenticated (clears auth cookies) |
| POST | `/api/auth/change-password` | Authenticated |
| POST | `/api/auth/reset-password` | SuperAdmin |
| POST | `/api/auth/reset-superadmin-password` | Development + localhost only (404 otherwise) |
| POST | `/api/auth/forgot-password` | Anonymous |
| POST | `/api/auth/reset-password-with-token` | Anonymous (24 h token) |

Cookie mode sets HttpOnly `ldp_access`/`ldp_refresh` + CSRF cookie `ldp_csrf`; mutating requests must echo it in `X-XSRF-TOKEN`.

## Orders — `api/orders` (`OrdersController`)

| Verb | Route | Access |
|---|---|---|
| POST | `/api/orders` | Client + `CreateOrder` |
| POST | `/api/orders/with-files` | Client (atomic order + reference files) |
| POST | `/api/orders/manual-completed` | Admin, SuperAdmin (backfill completed orders) |
| GET | `/api/orders/{id}` | Authenticated (service scopes access) |
| GET | `/api/orders/my-orders` | Client |
| GET | `/api/orders/assigned-orders` | Designer |
| GET | `/api/orders` and `/api/orders/search` | `ViewAllOrders` |
| POST | `/api/orders/{id}/assign` | `AssignOrder` |
| PUT | `/api/orders/{id}/status` | SuperAdmin/Admin/Designer/Client + `UpdateOrderStatus` (allowed targets depend on role; validated by state machine) |
| POST | `/api/orders/{id}/request-price-approval` | Admin, SuperAdmin |
| POST | `/api/orders/{id}/approve-price`, `/{id}/respond-price-approval` | Client |
| POST | `/api/orders/{id}/approve` | Admin, SuperAdmin (→ InProgress or ApprovedUnassigned) |
| GET | `/api/orders/unassigned-count` | Admin, SuperAdmin |
| POST | `/api/orders/{id}/send-files-to-client`, `/{id}/send-preview-batch` | Admin, SuperAdmin + `UpdateOrderStatus` (→ PreviewDelivered) |
| PUT | `/api/orders/{id}` | Client (own, early statuses) |
| POST | `/api/orders/{id}/cancel` | Client (own, pre-approval states) / Admin, SuperAdmin (any non-terminal) |
| POST | `/api/orders/{id}/archive`, `/{id}/unarchive`, `/{id}/refund` | Admin, SuperAdmin |
| PUT | `/api/orders/{id}/client-price` | Admin, SuperAdmin |
| PUT | `/api/orders/{id}/allow-uploads` | Admin, SuperAdmin |
| GET | `/api/orders/{id}/logs` | Authenticated (scoped) |
| DELETE | `/api/orders/{id}` | Obsolete — delegates to cancel |

## Quotes — `api/quotes` (`QuotesController`)

| Verb | Route | Access |
|---|---|---|
| POST | `/api/quotes` | Client (≤10 attachments, 500 MB combined) |
| GET | `/api/quotes`, `/api/quotes/{id}` | Authenticated (scoped) |
| GET | `/api/quotes/{id}/attachments/{fileName}` | Client, Admin, SuperAdmin |
| POST | `/api/quotes/{id}/respond` | Admin, SuperAdmin (sets quoted price → Responded) |
| POST | `/api/quotes/{id}/reject` | Client |
| POST | `/api/quotes/{id}/convert-to-order` | Client (requires Responded/Accepted + admin price) |

## Files — `api/files` (`FilesController`)

| Verb | Route | Access |
|---|---|---|
| POST | `/api/files/upload/{orderId}`, `/upload-multiple/{orderId}` | `UploadFile` + order access (client owns / designer assigned / admin) + `AllowUploads` + non-terminal status |
| PUT | `/api/files/{id}/approve` | Admin, SuperAdmin |
| GET | `/api/files/{id}/download` | `DownloadFile` + visibility rules (clients blocked from unreleased previews) |
| GET | `/api/files`, `/api/files/order/{orderId}` | Authenticated (visibility-filtered) |
| DELETE | `/api/files/{id}` | Admin, SuperAdmin + `DeleteFile` |

Upload validation: extension/MIME/magic-byte checks, path traversal guards, 500 MB multipart cap, 1-hour duplicate window, optional scan hook, `ProductionSafety:DisableFileUploads` kill switch.

## Revisions — `api/revisions` (`RevisionsController`)

| Verb | Route | Access |
|---|---|---|
| POST | `/api/revisions/orders/{orderId}/request` | Client (status PreviewDelivered, within tier limit 2/4/unlimited) |
| GET | `/api/revisions/orders/{orderId}/latest`, `/files/{fileId}/download` | Client, Admin, SuperAdmin, Designer |
| POST | `/api/revisions/orders/{orderId}/approve-logo` | Client (→ ClientApproved) or Admin/SuperAdmin (→ Completed) |
| GET | `/api/revisions/orders/{orderId}/can-request` | Client |
| GET | `/api/revisions/orders/{orderId}/can-approve` | Client, Admin, SuperAdmin |

## Comments — `api/comments` (`CommentsController`)

| Verb | Route | Access |
|---|---|---|
| POST | `/api/comments/orders/{orderId}` | Authenticated (role-based visibility rules applied) |
| GET | `/api/comments/orders/{orderId}` | Authenticated (visibility-filtered; designers masked as "Design Team" to clients) |
| POST | `/api/comments/orders/{orderId}/mark-read`, GET `/unread-counts` | Authenticated |
| PUT | `/api/comments/{id}/visibility` | Admin, SuperAdmin |
| GET | `/api/comments/{id}`, DELETE `/api/comments/{id}` | Authenticated (scoped) |

## Messages — `api/messages` (`MessagesController`)

Admin-mediated relay: client/designer messages go through admin forward/reject.

| Verb | Route | Access |
|---|---|---|
| POST | `/api/messages` | Authenticated |
| GET | `/api/messages`, `/{id}`, `/order/{orderId}` | Authenticated (scoped) |
| PUT | `/api/messages/{id}/read` | Authenticated |
| POST | `/api/messages/forward`, `/api/messages/reject` | Admin, SuperAdmin |

## Invoices — `api/invoices` (`InvoicesController`)

| Verb | Route | Access |
|---|---|---|
| POST | `/api/invoices` | Admin, SuperAdmin (from completed billing-eligible orders) |
| POST | `/api/invoices/generate-flexible` | Admin, SuperAdmin |
| GET | `/api/invoices`, `/{id}`, `/{id}/logs`, `/statistics`, `/report`, `/{id}/download` (PDF) | Authenticated — clients see own invoices only; designers denied |
| PUT | `/api/invoices/{id}`, `/{id}/items` | Admin, SuperAdmin (blocked when Paid; kill switch `DisableInvoiceEditing`) |
| PUT | `/api/invoices/{id}/mark-paid` | Admin, SuperAdmin |
| POST | `/api/invoices/{id}/send` | Admin, SuperAdmin (email TODO — currently logs only) |

## Billing — `api/billing` (`BillingController`) — Admin, SuperAdmin

| Verb | Route |
|---|---|
| GET | `/api/billing/queue` |
| GET | `/api/billing/clients/{clientId}/eligible-orders` |
| POST | `/api/billing/clients/{clientId}/create-invoice` |

Automatic invoicing runs via Hangfire (Mondays for Weekly clients, 1st for Monthly); guarded by `ProductionSafety:DisableBillingGeneration`.

## Payments — `api/payments` (`PaymentsController`)

| Verb | Route | Access |
|---|---|---|
| POST | `/api/payments`, `/link`, `/process` | Authenticated (invoice access enforced) |
| GET | `/api/payments/{id}`, `/invoice/{invoiceId}` | Authenticated (scoped) |
| GET | `/api/payments/bank-details` | Client, Admin, SuperAdmin |
| POST | `/api/payments/verify/paypal` | Authenticated |
| PUT | `/api/payments/{id}/status` | Admin, SuperAdmin |
| POST | `/api/payments/webhook/paypal` | **Anonymous** (PayPal webhook) |

## Designer payout — `api/designer-payout` (`DesignerPayoutController`)

| Verb | Route | Access |
|---|---|---|
| GET | `/api/designer-payout/pricing-info` | Authenticated |
| POST | `/api/designer-payout/pricing/propose`, `/orders/{orderId}/submit-pricing` | Designer |
| PUT | `/api/designer-payout/orders/{orderId}/approve-price` | Admin, SuperAdmin |
| GET | `/api/designer-payout/orders/pending-approval`, `/orders`, `/order-preview/{orderId}`, `/designers/{designerId}/eligible-orders` | Admin, SuperAdmin |
| GET | `/api/designer-payout/me/orders-pending-approval`, `/me/eligible-orders`, `/me/invoices`, `/me/invoices/{invoiceId}` | Designer |
| POST | `/api/designer-payout/generate-invoice` | Admin, SuperAdmin |

## Designer invoices — `api/designer-invoice` (`DesignerInvoiceController`) — Admin, SuperAdmin

`payout-summary`, `eligible-orders`, `payout-overview` (GET); `generate` (POST); `designer/{designerId}`, `{invoiceId}` (GET); `{invoiceId}/mark-paid`, `{invoiceId}/items/{itemId}` (PUT); `{invoiceId}/adjustments` (POST); `{invoiceId}/adjustments/{adjustmentId}` (DELETE). Guarded by `ProductionSafety:DisableDesignerPayout`.

## Pricing — Admin, SuperAdmin

- `api/client-logo-pricing` (`ClientLogoPricingController`): GET `client/{clientId}`, `{id}`; POST; PUT `{id}`; DELETE `{id}`
- `api/designer-logo-pricing` (`DesignerLogoPricingController`): GET `designer/{designerId}`, `{id}`; POST; PUT `{id}`; DELETE `{id}`

## Analytics — Admin, SuperAdmin

- `api/admin/analytics` (`AnalyticsController`): `overview`, `orders`, `revenue`, `designers`, `clients`, `workflow`, `system`, `forecast`, `insights` (all GET; cached ~7 min)
- `api/admin/client-analytics` (`AdminClientAnalyticsController`): `overview`, `top-clients`, `revenue-trend`, `monthly-revenue`, `inactive-clients`, `lifetime-value`, `client-growth`, `client-activity`, `alerts`, `clients-dropdown`, `churn-risk`, `churn-alerts`, `retention-stats`
- `api/admin/financial` (`FinancialController`): `overview`, `revenue-trend`, `orders-vs-revenue`, `package-revenue`, `designer-revenue`, `client-revenue`, `invoice-status`, `weekly-revenue`, `order-value-trend`, `activity-feed`, `revenue-forecast`
- `api/client/financial-insights` (`ClientController`): Client role — own financial summary

## Users, roles, permissions

### `api/users` (`UsersController`)

| Verb | Route | Access |
|---|---|---|
| GET | `/api/users/me/permissions` | Authenticated |
| POST | `/api/users` | Admin, SuperAdmin + `CreateUser` |
| GET | `/api/users/{id}` | Authenticated |
| GET | `/api/users`, `/api/users/search` | Admin, SuperAdmin |
| PUT | `/api/users/{id}` | SuperAdmin |
| PUT | `/api/users/{id}/profile` | Authenticated (own profile) |
| DELETE | `/api/users/{id}`, PUT `/{id}/activate` | SuperAdmin |
| POST/GET | `/api/users/designer-profiles*` | `CreateDesignerProfile` / `ViewDesignerProfiles` |
| GET | `/api/users/clients/{id}/detail`, `/designers/{id}/detail` | Admin, SuperAdmin |

### `api/roles` (`RolesController`)

GET `/api/roles` — Admin, SuperAdmin.

### `api/permissions` (`PermissionsController`) — SuperAdmin only

GET `roles`, ``, `role/{roleId}`; POST `assign`; DELETE `revoke`.

## Misc

- `api/gallery` (`GalleryController`) — Client: `my-gallery`, `{id}` (approved finals)
- `api/reviews` (`ReviewsController`) — POST Client; GET authenticated (``, `{id}`, `order/{orderId}`)
- `api/notifications` (`NotificationsController`) — authenticated: GET ``, `unread-count`, `{id}`; PUT `{id}/read`, `mark-all-read`
- `api/settings` (`SettingsController`) — Admin/SuperAdmin: GET ``; POST `business`, `brand`, `logo`, `invoice-template`, `payment-methods`, `notifications`; POST `invoice` is SuperAdmin only
- `api/auditlogs` (`AuditLogsController`) — Admin/SuperAdmin: GET ``, `{entityType}/{entityId}`
- `api/system/health` (`SystemController`) — **anonymous** simplified health (database/signalr/storage)

## Non-controller endpoints

| Endpoint | Purpose | Access |
|---|---|---|
| `/hubs/notifications` | SignalR hub (bell notifications + entity update events) | `[Authorize]`; JWT via cookie, Bearer, or `access_token` query param |
| `/health` | Full health report | Unauthenticated — restrict at infrastructure level per readiness checklist |
| `/health/ready` | Readiness (DB, Hangfire, storage, disk, SMTP, memory, SignalR; Redis degraded-only) | Unauthenticated |
| `/health/live` | Liveness | Unauthenticated |
| `/hangfire` | Hangfire dashboard | Admin (read-only), SuperAdmin (full) |
| `/swagger` | API docs | Development only |

## Rate limits (staging/production)

| Bucket | Limit |
|---|---|
| Auth endpoints | 5/min |
| Order mutations | 120/min |
| File uploads | 30/min |
| General | 60/min |

Disabled in Development/Testing or with `DISABLE_RATE_LIMIT=true`.
