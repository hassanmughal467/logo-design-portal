# 🚀 Order Management System - API Quick Reference

## 📋 **Authentication**

All endpoints require JWT token in header:
```
Authorization: Bearer <your-token>
```

Get token via: `POST /api/auth/login`

---

## 📦 **Order Endpoints**

### Create Order (Client)
```
POST /api/orders
Body: {
  "title": "Logo Design",
  "description": "Description",
  "price": 500.00,
  "instructions": "Client instructions",
  "requiredFormats": "PNG, SVG, PDF",
  "deadline": "2024-12-31T00:00:00Z"
}
```

### Get Order
```
GET /api/orders/{id}
```

### Get My Orders (Client)
```
GET /api/orders/my-orders
```

### Get Assigned Orders (Designer)
```
GET /api/orders/assigned-orders
```

### Get All Orders (Admin)
```
GET /api/orders
```

### Assign Order to Designer (Admin)
```
POST /api/orders/{id}/assign
Body: { "designerId": "<guid>" }
```

### Update Order Status
```
PUT /api/orders/{id}/status
Body: { "status": "InProgress", "notes": "Optional notes" }
```

### Request Price Approval (Admin)
```
POST /api/orders/{id}/request-price-approval
Body: {
  "proposedPrice": 750.00,
  "notes": "Additional complexity"
}
```

### Approve Price (Client)
```
POST /api/orders/{id}/approve-price
Body: {
  "approved": true,
  "comment": "Price acceptable"
}
```

### Approve Order (Admin)
```
POST /api/orders/{id}/approve
```

### Send Files to Client (Admin)
```
POST /api/orders/{id}/send-files-to-client
Body: ["<file-id-1>", "<file-id-2>"]
```

---

## 📁 **File Endpoints**

### Upload File
```
POST /api/files/upload/{orderId}
Form Data:
  - file: [file]
  - fileType: "Reference" | "Preview" | "Final"
  - description: "Optional description"
```

### Download File
```
GET /api/files/{id}/download
```

### Get Order Files
```
GET /api/files/order/{orderId}
Returns: Files visible to current user based on role
```

### Approve File (Admin)
```
PUT /api/files/{id}/approve
Body: {
  "approved": true,
  "makeVisibleToClient": true
}
```

### Delete File
```
DELETE /api/files/{id}
```

---

## 🔄 **Revision Endpoints**

### Create Revision (Client)
```
POST /api/revisions/orders/{orderId}
Body: {
  "instructions": "Revision instructions"
}
```

### Get Order Revisions
```
GET /api/revisions/orders/{orderId}
```

### Get Revision
```
GET /api/revisions/{id}
```

### Resolve Revision (Admin/Designer)
```
PUT /api/revisions/{id}/resolve
```

---

## 💬 **Comment Endpoints**

### Create Comment
```
POST /api/comments/orders/{orderId}
Body: {
  "content": "Comment text",
  "isInternal": true  // true for Admin/Designer only
}
```

### Get Order Comments
```
GET /api/comments/orders/{orderId}
Returns: Filtered by role (clients see only external)
```

### Delete Comment
```
DELETE /api/comments/{id}
```

---

## 🔔 **Notification Endpoints**

### Get Notifications
```
GET /api/notifications?unreadOnly=false
```

### Get Unread Count
```
GET /api/notifications/unread-count
```

### Get Notification
```
GET /api/notifications/{id}
```

### Mark as Read
```
PUT /api/notifications/{id}/read
```

### Mark All as Read
```
PUT /api/notifications/mark-all-read
```

---

## 🖼️ **Gallery Endpoints**

### Get My Gallery (Client)
```
GET /api/gallery/my-gallery
```

### Get Gallery Item
```
GET /api/gallery/{id}
```

---

## 💰 **Invoice Endpoints**

### Create Invoice
```
POST /api/invoices
Body: {
  "orderId": "<guid>",
  "taxAmount": 50.00,
  "dueDate": "2024-12-31T00:00:00Z"
}
```

### Get Invoice
```
GET /api/invoices/{id}
```

### Get Invoices
```
GET /api/invoices
```

### Get Client Invoices
```
GET /api/invoices/client/{clientId}
```

### Mark Invoice as Paid
```
PUT /api/invoices/{id}/mark-paid
Body: { "paymentMethod": "Bank Transfer" }
```

---

## 📊 **Order Status Values**

- `WaitingForAdminApproval` (1)
- `PriceApprovalPending` (2)
- `InProgress` (3)
- `PreviewDelivered` (4)
- `RevisionRequested` (5)
- `FinalApproved` (6)
- `Completed` (7)
- `Cancelled` (8)

---

## 📁 **File Type Values**

- `Reference` (1) - Client reference files
- `Preview` (2) - Designer preview files
- `Final` (3) - Final approved files
- `Revision` (4) - Revision files

---

## 🔔 **Notification Type Values**

- `Info` (1)
- `Success` (2)
- `Warning` (3)
- `Error` (4)
- `OrderStatusChange` (5)
- `PriceApproval` (6)
- `RevisionRequest` (7)
- `FileUpload` (8)

---

## 💰 **Invoice Status Values**

- `Pending` (1)
- `Paid` (2)
- `Due` (3)
- `Overdue` (4)

---

## 🔐 **Role-Based Access Summary**

### Client
- ✅ Create orders
- ✅ View own orders
- ✅ See approved files only
- ✅ Request revisions
- ✅ Approve/reject prices
- ✅ View own gallery
- ✅ View own invoices
- ✅ See external comments only

### Designer
- ✅ View assigned orders
- ✅ Upload files
- ✅ See all files for assigned orders
- ✅ See internal comments
- ✅ Resolve revisions
- ❌ Cannot see client identity

### Admin
- ✅ View all orders
- ✅ Approve orders
- ✅ Assign orders
- ✅ Request price approval
- ✅ Approve files
- ✅ Send files to clients
- ✅ See all files (approved + unapproved)
- ✅ See internal comments
- ⚠️ Client data masked (no email/phone)

### SuperAdmin
- ✅ All Admin permissions
- ✅ Full client information access
- ✅ User management

---

## 🧪 **Quick Test Flow**

1. **Login as Client** → Create order
2. **Login as Admin** → Approve order → Assign to designer
3. **Login as Designer** → Upload preview file
4. **Login as Admin** → Approve file → Send to client
5. **Login as Client** → Request revision
6. **Login as Designer** → Upload new preview
7. **Login as Admin** → Approve and send
8. **Login as Client** → Approve final → Order completed

---

## 📝 **Testing Tips**

1. **Use Swagger UI** for interactive testing
2. **Check notifications** after each action
3. **Verify file visibility** from different roles
4. **Test status transitions** in order
5. **Verify role-based access** restrictions
6. **Check status history** for audit trail

---

**For detailed testing instructions, see: `ORDER_MANAGEMENT_SYSTEM_TESTING_GUIDE.md`**
