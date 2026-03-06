# Notification System Audit Summary

This document summarizes the notification system audit and the changes implemented to ensure complete coverage across Admin, Client, and Designer roles.

## Audit Results

### Services Audited
- **OrderService** – Order lifecycle, status changes, assignments
- **MessageService** – Client/Admin messages
- **InvoiceService** – Invoice creation, payment
- **PaymentService** – Payment processing
- **AuthService** – New client registration
- **FileService** – File uploads
- **RevisionService** – Preview approval/rejection, order completion
- **UserService** – No notification events required per spec

### Direct Database Inserts
- **Finding**: No direct `_context.Notifications.Add()` outside `NotificationService`. All notifications go through `INotificationService.CreateNotificationAsync()` or role/user variants.

---

## Admin Notification Coverage

| Event | Recipients | Message Format | SignalR Event | Status |
|-------|------------|----------------|---------------|--------|
| NEW ORDER SUBMITTED | Admin, SuperAdmin | "{ClientName} placed a new order (#OrderNumber)" | OrderCreated | ✅ Implemented |
| CLIENT APPROVES PREVIEW | Admin, SuperAdmin | "{ClientName} approved the preview for order (#OrderNumber)" | PreviewApproved | ✅ Implemented |
| CLIENT REJECTS PREVIEW | Admin, SuperAdmin | "{ClientName} rejected the preview for order (#OrderNumber)" | PreviewRejected | ✅ Implemented |
| CLIENT SENDS MESSAGE | Admin, Assigned Designer | "{ClientName} sent a new message regarding order (#OrderNumber)" | ReceiveNotification | ✅ Implemented |
| PAYMENT RECEIVED | Admin, SuperAdmin | "Payment received for order (#OrderNumber)" | ReceiveNotification | ✅ Implemented |
| NEW CLIENT REGISTERED | Admin, SuperAdmin | "New client registered: {ClientName}" | ReceiveNotification | ✅ Already existed |
| FILE UPLOADED BY CLIENT | Admin | "{ClientName} uploaded files for order (#OrderNumber)" | ReceiveNotification | ✅ Implemented |

---

## Client Notification Coverage

| Event | Recipients | Message Format | SignalR Event | Status |
|-------|------------|----------------|---------------|--------|
| ORDER APPROVED | Client | "Your order (#OrderNumber) has been approved" | OrderStatusChanged | ✅ Already existed |
| PREVIEW DELIVERED | Client | "Preview files were uploaded for order (#OrderNumber)" | PreviewDelivered | ✅ Already existed |
| ORDER STATUS UPDATED | Client | "Order (#OrderNumber) status updated to {Status}" | OrderStatusChanged | ✅ Already existed |
| ORDER COMPLETED | Client | "Your order (#OrderNumber) has been completed" | OrderStatusChanged | ✅ Implemented |
| ADMIN MESSAGE | Client | "{AdminName} sent a new message regarding order (#OrderNumber)" | ReceiveNotification | ✅ Implemented |
| INVOICE GENERATED | Client | "Invoice (#InvoiceNumber) generated for order (#OrderNumber)" | InvoiceGenerated | ✅ Already existed |
| ORDER CANCELLED | Client | "Your order (#OrderNumber) was cancelled" | ReceiveNotification | ✅ Implemented |

---

## Designer Notification Coverage

| Event | Recipients | Message Format | SignalR Event | Status |
|-------|------------|----------------|---------------|--------|
| ORDER ASSIGNED | Designer | "You have been assigned order (#OrderNumber)" | ReceiveNotification | ✅ Implemented |
| CLIENT FEEDBACK (Revision) | Designer | "{ClientName} rejected the preview for order (#OrderNumber)" | PreviewRejected | ✅ Implemented |
| PREVIEW REJECTED | Designer | "{ClientName} rejected the preview for order (#OrderNumber)" | PreviewRejected | ✅ Implemented |
| CLIENT APPROVES PREVIEW | Designer | "{ClientName} approved the preview for order (#OrderNumber)" | PreviewApproved | ✅ Implemented |

---

## SignalR Entity Update Events

| Event | Purpose | Recipients |
|-------|---------|------------|
| OrderCreated | New order in grid | Admin, SuperAdmin |
| OrderStatusChanged | Status update in grid | Client, Admin, Designer (if assigned) |
| PreviewApproved | Client approved preview | Admin, Designer |
| PreviewRejected | Client requested revision | Admin, Designer |
| PreviewDelivered | Preview files sent to client | Client |
| InvoiceGenerated | Invoice created for order | Client |

---

## Files Modified

### Backend
- `IRealtimeEntityUpdateSender.cs` – Added `SendOrderCreatedAsync`, `SendPreviewRejectedAsync`
- `SignalRRealtimeEntityUpdateSender.cs` – Implemented new events
- `OrderService.cs` – OrderCreated emit, AssignOrder notification, Preview approved/rejected notifications, Order cancelled notification, Completed message
- `RevisionService.cs` – INotificationService injection, Preview rejected notifications, Order completed notification, Preview approved notifications
- `MessageService.cs` – Client sends → Admin + Designer; Admin sends → Client
- `FileService.cs` – INotificationService injection, Client file upload → Admin notification
- `InvoiceService.cs` – Payment message uses order number when available
- `PaymentService.cs` – Payment message uses order number when available

### Frontend
- `realtime-notification.service.ts` – Added `OrderCreated`, `PreviewRejected` listeners

---

## Navigation Links

Notifications use `ReferenceType` and `ReferenceId` for navigation:
- **Order**: `/orders/{orderId}`
- **Invoice**: `/invoices/{invoiceId}`
- **Message**: `/orders/{orderId}/messages` (when OrderId present)

---

## Security

- All notifications are created with `UserId` – users only receive their own notifications
- API validates `notification.UserId == currentUserId` when fetching/marking read
- Role-based notifications use `CreateNotificationForRoleAsync` to target Admin/SuperAdmin/Designer

---

## Optional Email Notifications

Email notifications are sent when enabled in settings (`EmailNotifications` or `emailNotifications`). The `NotificationService` calls `SendEmailIfEnabledAsync` after persisting each notification.
