# Production Safety Kill-Switch System

This document describes the production safety and fail-safe protections added to the Logo Design Portal API.

## Overview

The system adds multiple layers of protection to ensure that critical failures do not break business operations. If a bug occurs in production, the system can continue operating while admins can disable affected modules instantly.

---

## 1. Global Error Safety

**Location:** `Backend/src/LogoDesignPortal.API/Middleware/ExceptionMiddleware.cs`

- Catches all unhandled exceptions from controllers
- Returns safe JSON responses with a `message` field
- **Never exposes stack traces** to clients
- Logs full error details internally for incident tracking

**Response mapping:**
- `400 Bad Request` – validation errors, `InvalidOperationException`, `ArgumentException`
- `401 Unauthorized` – `UnauthorizedAccessException`
- `403 Forbidden` – `ForbiddenAccessException`
- `404 Not Found` – `FileNotFoundException`, `KeyNotFoundException`
- `500 Internal Server Error` – unexpected errors (generic message to client)

**Example response:**
```json
{
  "message": "An unexpected error occurred. Please contact support."
}
```

---

## 2. Feature Flag / Kill-Switch System

**Configuration:** `appsettings.json` → `ProductionSafety` section

```json
{
  "ProductionSafety": {
    "DisableBillingGeneration": false,
    "DisableDesignerPayout": false,
    "DisableFileUploads": false,
    "DisableInvoiceEditing": false
  }
}
```

**Usage:** Set any flag to `true` to disable that module. Changes take effect on next request (config is read per request).

| Flag | Effect |
|------|--------|
| `DisableBillingGeneration` | Blocks invoice creation (manual and auto) |
| `DisableDesignerPayout` | Blocks designer invoice generation |
| `DisableFileUploads` | Blocks all file uploads |
| `DisableInvoiceEditing` | Blocks invoice updates (PUT) |

**Example:** If a billing bug is found, set `DisableBillingGeneration: true` in `appsettings.Production.json` and redeploy (or use environment variables) to stop billing until a fix is deployed.

---

## 3. Financial Safety Guard

**Locations:** `InvoiceService`, `DesignerPayoutService`

Before creating invoices or payouts:

- Order status must be `Completed`
- Price fields must not be null
- Amounts must be non-negative

If validation fails: the operation is rejected with a clear error, and the issue is logged. Corrupted invoices are not generated.

---

## 4. Order Workflow Protection

**Location:** `OrderStatusStateMachine` (existing)

Invalid status transitions are rejected, including:

- `Completed` → `InProgress`
- `Cancelled` → `Completed`
- `Refunded` → `InProgress`

`OrderService.UpdateOrderStatusAsync` uses `OrderStatusStateMachine.ValidateTransition()` before applying changes.

---

## 5. Concurrent Operation Protection

**Locations:** `InvoiceService`, `DesignerPayoutService`, `BillingService`

- Invoice creation runs inside `ExecuteInTransactionAsync` (atomic)
- Designer invoice generation uses transactions
- Billing queue validation re-checks order state before invoicing to reduce race conditions

---

## 6. SignalR Failure Tolerance

**Locations:** `SignalRRealtimeNotificationSender`, `SignalRRealtimeEntityUpdateSender`

- If SignalR broadcast fails, the error is logged and the operation continues
- Example: if `OrderCreated` broadcast fails, the order is still created
- Clients can fall back to polling for updates

---

## 7. File System Safety

**Location:** `FileService`

- Validates file size and type before upload
- Ensures storage path stays within the configured base directory
- Wraps file write in try-catch; on failure, logs and returns a safe message without crashing the API

---

## 8. Billing Queue Safety

**Location:** `BillingService`, `BillingAutoInvoiceService`

- Validates `BillingEligible` and `ClientChargePrice` before invoicing
- Skips problematic orders and logs
- Continues processing other orders
- Respects `DisableBillingGeneration` kill-switch

---

## 9. Production Monitoring Logging

Structured logs for critical events:

| Event | Fields |
|-------|--------|
| `OrderCreated` | OrderId, UserId |
| `OrderCompleted` | OrderId, UserId, Timestamp |
| `InvoiceGenerated` | InvoiceId, OrderIds, UserId, Timestamp |
| `PaymentRecorded` | InvoiceId, UserId, Timestamp |
| `DesignerPayoutGenerated` | InvoiceId, DesignerId, UserId, Timestamp |

Use these logs for incident tracking and auditing.

---

## 10. System Health Check Endpoint

**Endpoint:** `GET /api/system/health`  
**Auth:** None (AllowAnonymous)

**Response:**
```json
{
  "database": "ok",
  "signalr": "ok",
  "storage": "ok"
}
```

Use this for load balancer health checks and monitoring. Each component reports `"ok"` or `"error"`.

---

## Configuration Example (Production)

Add to `appsettings.Production.json` or use environment variables:

```json
{
  "ProductionSafety": {
    "DisableBillingGeneration": false,
    "DisableDesignerPayout": false,
    "DisableFileUploads": false,
    "DisableInvoiceEditing": false
  }
}
```

Environment variables (if supported by your config provider):

- `ProductionSafety__DisableBillingGeneration=true`
- `ProductionSafety__DisableDesignerPayout=true`
- etc.

---

## Compatibility

All changes are additive and backward compatible. Existing frontend behavior is unchanged.
